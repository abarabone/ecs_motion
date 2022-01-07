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

    public class AreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (!this.isActiveAndEnabled) return;


        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) return;


        }

        static public void CreateStructureEntities_Compound
            (GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var top = st;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var posture = st.GetComponentInChildren<PostureAuthoring>();
            var parts = near.GetComponentsInChildren<StructurePartAuthoring>();

            st.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, posture);
            initMainEntity_(gcs, top, posture, st.NearModel, st.FarModel, parts.Length);

            initCompoundColliderEntity(gcs, near);

            //setBoneForFarEntity_(gcs, posture, far, top.transform);
            //setBoneForNearSingleEntity_(gcs, posture, near, near.transform);

            //trimEntities_(gcs, st);
            //orderTrimEntities_(gcs, st);
        }

        static void initBinderEntity_
            (GameObjectConversionSystem gcs, StructureBuildingModelAuthoring top, PostureAuthoring main)
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
            GameObjectConversionSystem gcs, StructureBuildingModelAuthoring top, PostureAuthoring main,
            IMeshModelLod near, IMeshModelLod far, int partLength)
        {
            var em = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var mainAddtypes = new ComponentTypes
            (
                new ComponentType[]
                {
                    typeof(Main.MainTag),
                    typeof(DrawInstance.MeshTag),
                    //typeof(NonUniformScale),//暫定
                    //typeof(ObjectMain.ObjectMainTag),
                    typeof(Main.BinderLinkData),//暫定
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Main.PartDestructionData),
                    typeof(Collision.Hit.TargetData),
                    //typeof(Main.SleepTimerData),
                    typeof(DrawInstance.NeedLodCurrentTag)
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
                    //DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                    DrawModelEntityCurrent = mainEntity,// ダミーとして、モデルでないものを入れとく（危険かなぁ…）
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
                new Main.PartDestructionData
                {
                    partLength = partLength,
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

        static void initCompoundColliderEntity(GameObjectConversionSystem gcs, GameObject near)
        {
            var em = gcs.DstEntityManager;

            var nearent = gcs.GetPrimaryEntity(near);
            em.AddComponentData(nearent, new LateBuildCompoundColliderConversion.TargetData
            {
                Dst = near,
                Srcs = near.GetComponentsInChildren<StructurePartAuthoring>().Select(x => x.gameObject),
            });
        }

    }
}
