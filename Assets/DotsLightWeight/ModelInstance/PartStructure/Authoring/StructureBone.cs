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

                    typeof(PhysicsWorldIndex),
                });
                em.AddComponents(ent, addtypes);

                em.CopyTransformToMarker(ent, bone.transform);


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

                {
                    var tf = bone.transform;
                    em.SetComponentData(ent, new Marker.Translation
                    {
                        Value = tf.position
                    });
                    em.SetComponentData(ent, new Marker.Rotation
                    {
                        Value = tf.rotation
                    });
                }

                {
                    var qColliderPart =
                        from pt in parts
                        let ptent = gcs.GetPrimaryEntity(pt)
                        where em.HasComponent<PhysicsCollider>(ptent)
                        select (pt, ptent)
                        ;
                    var colliderParts = qColliderPart.ToArray();

                    var qPartInfoData =
                        from x in colliderParts
                        let model = x.pt.QueryModel.First()
                        let modelEntity = gcs.GetFromModelEntityDictionary(model)
                        //let modelEntity = gcs.GetPrimaryEntity(x.pt.PartModel.AsGameObject)
                        select new PartBone.PartInfoData
                        {
                            PartId = x.pt.PartId,
                            DebrisPrefab = gcs.CreateDebrisPrefab(x.pt.gameObject, modelEntity),
                        };
                    using var partInfos = qPartInfoData.ToNativeArray(Allocator.Temp);

                    var mtinv = bone.transform.worldToLocalMatrix;
                    var qPartColliderResource =
                        from x in colliderParts
                        let tf = x.pt.transform
                        select new PartBone.PartColliderResourceData
                        {
                            ColliderInstance = new CompoundCollider.ColliderBlobInstance
                            {
                                Collider = em.GetComponentData<PhysicsCollider>(x.ptent).Value,
                                CompoundFromChild = new RigidTransform
                                {
                                    pos = mtinv.MultiplyPoint3x4(tf.position),
                                    rot = tf.rotation * mtinv.rotation,
                                }
                            }
                        };
                    using var partColliderResources = qPartColliderResource.ToNativeArray(Allocator.Temp);

                    var infobuf = em.AddBuffer<PartBone.PartInfoData>(ent);
                    infobuf.AddRange(partInfos);

                    var resbuf = em.AddBuffer<PartBone.PartColliderResourceData>(ent);
                    resbuf.AddRange(partColliderResources);

                    var destructiondata = em.GetComponentData<Main.PartDestructionData>(mainent);
                    var compoundColider = resbuf.BuildCompoundCollider(destructiondata);
                    em.SetComponentData(ent, compoundColider);
                }
            }
        }

    }
}
