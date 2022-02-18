using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;

using Collider = Unity.Physics.Collider;


namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.EntityTrimmer.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;

    public class StructureBone : MonoBehaviour//, IConvertGameObjectToEntity
    {


        public int BoneId;




        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            var top = this.GetComponentInParent<StructureAreaAuthoring>();
            var main = top.GetComponentInChildren<PostureAuthoring>();
            var parts = this.GetComponentsInChildren<StructureAreaPartAuthoring>();
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


                var mtinv = bone.transform.worldToLocalMatrix;
                var infobuf = em.AddBuffer<PartBone.PartInfoData>(ent);
                var resbuf = em.AddBuffer<PartBone.PartColliderResourceData>(ent);
                foreach (var pt in parts)
                {
                    var tf = pt.transform;
                    var ptent = gcs.GetPrimaryEntity(pt);

                    if (!em.HasComponent<PhysicsCollider>(ptent)) continue;

                    //Debug.Log($"add collider part {pt.name}");

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
                    infobuf.Add(new PartBone.PartInfoData
                    {
                        PartId = pt.PartId,
                        //DebrisPrefab = StructurePartUtility.CreateDebrisPrefab(gcs, pt.gameObject),
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
