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
            //var bones = near.GetComponentsInChildren<StructureBone>();

            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, posture);
            initMainEntity_(gcs, top, posture, this.NearModel, parts.Length);

            //initCompoundColliderEntity(gcs, top.gameObject, parts);
            gcs.DstEntityManager.SetComponentData(gcs.GetPrimaryEntity(top), new PhysicsCollider
            {
                Value = createCompoundCollider(parts, CollisionFilter.Default)
            });

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
                    typeof(Main.PartDestructionResourceData),
                    typeof(Main.PartInfoData),
                    //typeof(Main.SleepTimerData),
                    typeof(Collision.Hit.TargetData),
                    typeof(PhysicsCollider),
                }
            );
            em.AddComponents(mainEntity, mainAddtypes);

            //em_.SetComponentData(mainEntity,
            //    new NonUniformScale
            //    {
            //        Value = new float3(1, 1, 1)
            //    }
            //);
            em.SetComponentData(mainEntity,
                new Main.BinderLinkData
                {
                    BinderEntity = binderEntity,
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(near.Obj),
                    //DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                    //DrawModelEntityCurrent = mainEntity,// ダミーとして、モデルでないものを入れとく（危険かなぁ…）
                    // 最初のＬＯＤ判定で Null もタグ付けさせるため
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            em.SetComponentData(mainEntity,
                new Collision.Hit.TargetData
                {
                    MainEntity = gcs.GetPrimaryEntity(main),
                    HitType = Collision.HitType.envelope,
                }
            );


            em.SetComponentData(mainEntity,
                new Main.PartInfoData
                {
                    PartLength = partLength,
                    LivePartLength = partLength,
                }
            );
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

        public void initColliders(
            GameObjectConversionSystem gcs, PostureAuthoring main,
            StructureAreaPartAuthoring[] parts, CollisionFilter filter)
        {
            var em = gcs.DstEntityManager;
            var mtinv = main.transform.worldToLocalMatrix;

            for (var i = 0; i < parts.Length; i++)
            {

            }
        }
        public BlobAssetReference<Collider> createCompoundCollider(StructureAreaPartAuthoring[] parts, CollisionFilter filter)
        {
            var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
                parts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < parts.Length; i++)
            {
                dst[i] = new CompoundCollider.ColliderBlobInstance
                {
                    Collider = BlobAssetReference<Collider>.Null,
                    CompoundFromChild = new RigidTransform
                    {
                        pos = float3.zero,
                        rot = quaternion.identity,
                    }
                };
            }

            var res = CompoundCollider.Create(dst);
            dst.Dispose();
            return res;
        }
    }
}
