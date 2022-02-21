using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;

using Collider = Unity.Physics.Collider;


namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.EntityTrimmer.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    public class StructureBone : MonoBehaviour//, IConvertGameObjectToEntity
    {


        public int BoneId;




        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            var top = this.GetComponentInParent<StructureAreaAuthoring>();
            var main = top.GetComponentInChildren<PostureAuthoring>();
            var parts = this.GetComponentsInChildren<StructureAreaPartAuthoring>();
            Debug.Log($"part length {parts.Length}");
            initStructureBone_(conversionSystem, main, this, parts);

            return;


            static void initStructureBone_(
                GameObjectConversionSystem gcs, PostureAuthoring main, StructureBone bone,
                IEnumerable<StructureAreaPartAuthoring> parts)
            {
                var em = gcs.DstEntityManager;
                var ent = gcs.GetPrimaryEntity(bone);
                var mainent = gcs.GetPrimaryEntity(main);

                var addtypes = new ComponentTypes(new ComponentType[]
                {
                    //typeof(Bone.ColliderInitializeData),
                    typeof(PartBone.PartInfoData),
                    typeof(PartBone.PartColliderResourceData),
                    //typeof(PartBone.LengthData),
                    typeof(PartBone.LinkToMainData),

                    typeof(Collision.Hit.TargetData),
                    typeof(PhysicsCollider),
                    typeof(Marker.Rotation),
                    typeof(Marker.Translation),
                    typeof(Part.PartData),//とりあえずテスト的に
                });
                em.AddComponents(ent, addtypes);

                em.SetComponentData(ent, new PartBone.LinkToMainData
                {
                    MainEntity = mainent,
                });

                em.SetComponentData(ent, new Collision.Hit.TargetData
                {
                    MainEntity = mainent,
                    HitType = Collision.HitType.part,
                });

                em.SetComponentData(ent, new Part.PartData
                {
                    PartId = -1,// コンポジットコライダーであることを示す
                });



                //var qSrc =
                //    from pt in parts
                //    let ptent = gcs.GetPrimaryEntity(pt)
                //    where em.HasComponent<PhysicsCollider>(ptent)
                //    select (pt, ptent)
                //    ;
                //var srcs = qSrc.ToArray();

                //var qInfo =
                //    from x in srcs
                //    let key = x.pt.QueryModel.First().SourcePrefabKey
                //    let modelEntity = gcs.GetFromModelEntityDictionary(key)
                //    select new PartBone.PartInfoData
                //    {
                //        PartId = x.pt.PartId,
                //        DebrisPrefab = Entity.Null,//gcs.CreateDebrisPrefab(x.pt.gameObject, modelEntity),
                //    };
                var infobuf = em.AddBuffer<PartBone.PartInfoData>(ent);
                //using var infos = qInfo.ToNativeArray(Allocator.Temp);
                //infobuf.AddRange(infos);

                var mtinv = bone.transform.worldToLocalMatrix;
                //var qRes =
                //    from x in srcs
                //    let tf = x.pt.transform
                //    select new PartBone.PartColliderResourceData
                //    {
                //        ColliderInstance = new CompoundCollider.ColliderBlobInstance
                //        {
                //            Collider = em.GetComponentData<PhysicsCollider>(x.ptent).Value,
                //            CompoundFromChild = new RigidTransform
                //            {
                //                pos = mtinv.MultiplyPoint3x4(tf.position),
                //                rot = tf.rotation * mtinv.rotation,
                //            }
                //        }
                //    };
                var resbuf = em.AddBuffer<PartBone.PartColliderResourceData>(ent);
                //using var ress = qRes.ToNativeArray(Allocator.Temp);
                //resbuf.AddRange(ress);


                foreach (var pt in parts)
                {
                    var tf = pt.transform;
                    var ptent = gcs.GetPrimaryEntity(pt);

                    Debug.Log($"pre part {pt.name}");
                    if (!em.HasComponent<PhysicsCollider>(ptent)) continue;
                    Debug.Log($"add collider part {pt.name}");

                    resbuf.Add(new PartBone.PartColliderResourceData
                    {
                        ColliderInstance = new CompoundCollider.ColliderBlobInstance
                        {
                            Collider = em.GetComponentData<PhysicsCollider>(ptent).Value,
                            CompoundFromChild = new RigidTransform
                            {
                                pos = mtinv.MultiplyPoint3x4(tf.position),
                                rot = tf.rotation * mtinv.rotation,
                            },
                        },
                    });

                    var modelEntity = gcs.GetFromModelEntityDictionary(pt.QueryModel.First().SourcePrefabKey);
                    infobuf.Add(new PartBone.PartInfoData
                    {
                        PartId = pt.PartId,
                        DebrisPrefab = Entity.Null,//gcs.CreateDebrisPrefab(pt.gameObject, modelEntity),
                    });
                }

                var destructiondata = em.GetComponentData<Main.PartDestructionData>(mainent);
                var compoundColider = resbuf.BuildCompoundCollider(destructiondata);
                em.SetComponentData(ent, compoundColider);

                //em.SetComponentData(ent, new PartBone.LengthData
                //{
                //    PartLength = parts.Count(),
                //    NumSubkeyBits = compoundColider.Value.Value.NumColliderKeyBits,
                //});

            }
        }

    }
}
