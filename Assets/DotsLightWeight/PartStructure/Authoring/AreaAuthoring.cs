using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Threading.Tasks;
using Unity.Linq;
using UnityEditor;
using Unity.Physics;

using Collider = Unity.Physics.Collider;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Structure.Authoring;


    //binder
    //main
    // posture
    // draw instance
    // collider
    // destructions
    //part
    // debris prefab
    //bone
    // bone id



    public class AreaAuthoring : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public StructureModel<UI32, StructureVertex> NearModel;

        public GameObject Envelope;
        public AreaAuthoring MasterPrefab;


        public override IEnumerable<IMeshModel> QueryModel =>
            new IMeshModel[] { this.NearModel };



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            CreateStructureEntities_Compound(conversionSystem);

        }

        void CreateStructureEntities_Compound(GameObjectConversionSystem gcs)
        {
            var top = this;
            var near = this.NearModel.Obj;
            var env = this.Envelope;
            var posture = this.GetComponentInChildren<PostureAuthoring>();
            var parts = near.GetComponentsInChildren<StructureAreaPartAuthoring>();
            var bones = near.GetComponentsInChildren<StructureBone>();

            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, posture);
            initMainEntity_(gcs, top, posture, this.NearModel, parts.Length);

            var bone0 = bones.First();
            var qParts0 = parts
                .Where(x => x.GetComponentInParent<StructureBone>().BoneId == 0);
            initStructureBone(gcs, bone0, qParts0);

            //setBoneForFarEntity_(gcs, posture, far, top.transform);
            //setBoneForNearSingleEntity_(gcs, posture, near, near.transform);

            //trimEntities_(gcs, st);
            //orderTrimEntities_(gcs, st);
        }

        static void initBinderEntity_
            (GameObjectConversionSystem gcs, AreaAuthoring top, PostureAuthoring main)
        {
            var em_ = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var binderAddtypes = new ComponentTypes
            (
                typeof(BinderTrimBlankLinkedEntityGroupTag),
                typeof(ObjectBinder.MainEntityLinkData)
            );
            em_.AddComponents(binderEntity, binderAddtypes);

            em_.SetComponentData(binderEntity,
                new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


            em_.SetName_(binderEntity, $"{top.name} binder");
        }

        static void initMainEntity_(
            GameObjectConversionSystem gcs, AreaAuthoring top, PostureAuthoring main,
            IMeshModelLod near, int partLength)
        {
            var em = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var mainAddtypes = new ComponentTypes
            (
                new ComponentType[]
                {
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.NeedLodCurrentTag),
                    //typeof(NonUniformScale),//暫定
                    //typeof(ObjectMain.ObjectMainTag),
                    typeof(Main.MainTag),
                    typeof(Main.BinderLinkData),//暫定
                    typeof(Main.PartDestructionData),
                    typeof(Main.PartLengthData),
                    //typeof(Main.SleepTimerData),
                    //typeof(Collision.Hit.TargetData),
                    //typeof(PhysicsCollider),
                }
            );
            em.AddComponents(mainEntity, mainAddtypes);

            //em.SetComponentData(mainEntity, new NonUniformScale
            //{
            //    Value = new float3(1, 1, 1)
            //});
            em.SetComponentData(mainEntity, new Main.BinderLinkData
            {
                BinderEntity = binderEntity,
            });
            em.SetComponentData(mainEntity, new DrawInstance.ModelLinkData
            {
                DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(near.Obj),
                //DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                //DrawModelEntityCurrent = mainEntity,// ダミーとして、モデルでないものを入れとく（危険かなぁ…）
                // 最初のＬＯＤ判定で Null もタグ付けさせるため
            });
            em.SetComponentData(mainEntity, new DrawInstance.TargetWorkData
            {
                DrawInstanceId = -1,
            });
            em.SetComponentData(mainEntity, new Main.PartLengthData
            {
                TotalPartLength = partLength,
            });


            //em.SetComponentData(mainEntity,
            //    new Main.SleepTimerData
            //    {
            //        PrePositionAndTime = new float4(0, 0, 0, Main.SleepTimerData.Margin),
            //    }
            //);

            var draw = mainEntity;
            gcs.AddLod1ComponentToDrawInstanceEntity(draw, top.gameObject, near);


            gcs.InitPostureEntity(main);//, far.objectTop.transform);


            em.SetName_(mainEntity, $"{top.name} main");
        }

        //static void initCompoundColliderEntity(GameObjectConversionSystem gcs, GameObject main, StructureAreaPartAuthoring[] parts)
        //{
        //    var em = gcs.DstEntityManager;
        //    Debug.Log(main.name);

        //    var ent = gcs.GetPrimaryEntity(main);
        //    em.AddComponentData(ent, new LateBuildCompoundColliderConversion.TargetData
        //    {
        //        Dst = main,
        //        Srcs = parts.Select(x => x.gameObject),
        //    });
        //}

        public void initStructureBone(
            GameObjectConversionSystem gcs, PostureAuthoring main, StructureBone bone,
            IEnumerable<StructureAreaPartAuthoring> parts)
        {
            var em = gcs.DstEntityManager;
            var ent = gcs.GetPrimaryEntity(bone);

            var addtypes = new ComponentTypes(new ComponentType[]
            {
                typeof(Structure.Bone.ColliderInitializeData),
                typeof(Structure.Bone.PartDestructionResourceData),
                typeof(Structure.Bone.PartInfoData),
                typeof(Collision.Hit.TargetData),
                typeof(PhysicsCollider),
            });
            em.AddComponents(ent, addtypes);

            em.SetComponentData(ent, new Collision.Hit.TargetData
            {
                MainEntity = gcs.GetPrimaryEntity(main),
                HitType = Collision.HitType.part,
            });

            var partLength = parts.Count();
            em.SetComponentData(ent, new Structure.Bone.PartInfoData
            {
                PartLength = partLength,
                LivePartLength = partLength,
            });

            var resbuf = em.AddBuffer<Structure.Bone.PartDestructionResourceData>(ent);


            var mtinv = bone.transform.worldToLocalMatrix;
            var initbuf = em.AddBuffer<Structure.Bone.ColliderInitializeData>(ent);
            foreach (var pt in parts)
            {
                var tf = pt.transform;

                initbuf.Add(new Structure.Bone.ColliderInitializeData
                {
                    ChildPartEntity = gcs.GetPrimaryEntity(pt),
                    RigidTransform = new RigidTransform
                    {
                        pos = mtinv.MultiplyPoint3x4(tf.position),
                        rot = tf.rotation * mtinv.rotation,
                    },
                    PartId = pt.PartId,
                    DebrisPrefab = Entity.Null,
                });
            }
        }
        //public void initBoneColliders(
        //    GameObjectConversionSystem gcs, StructureBone bone,
        //    IEnumerable<StructureAreaPartAuthoring> parts, CollisionFilter filter)
        //{
        //    var em = gcs.DstEntityManager;
        //    var mtinv = bone.transform.worldToLocalMatrix;

        //    var ent = gcs.GetPrimaryEntity(bone);
        //    var buffer = em.AddBuffer<>

        //    foreach (var p in parts)
        //    {
        //        var mt = p.transform.localToWorldMatrix * mtinv;


        //    }
        //}
        //public BlobAssetReference<Collider> createCompoundCollider(StructureAreaPartAuthoring[] parts, CollisionFilter filter)
        //{
        //    var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
        //        parts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        //    for (var i = 0; i < parts.Length; i++)
        //    {
        //        dst[i] = new CompoundCollider.ColliderBlobInstance
        //        {
        //            Collider = BlobAssetReference<Collider>.Null,
        //            CompoundFromChild = new RigidTransform
        //            {
        //                pos = float3.zero,
        //                rot = quaternion.identity,
        //            }
        //        };
        //    }

        //    var res = CompoundCollider.Create(dst);
        //    dst.Dispose();
        //    return res;
        //}
    }
}
