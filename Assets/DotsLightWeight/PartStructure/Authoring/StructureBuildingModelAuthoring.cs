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
using Unity.Physics;
using Unity.Linq;

namespace Abarabone.Structure.Authoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Character;//ObjectMain はここにある、名前変えるべきか？

    using Abarabone.Common.Extension;
    using Abarabone.Structure;
    using Unity.Entities.UniversalDelegates;
    using Unity.Properties;
    using System.CodeDom;
    using Abarabone.Utilities;
    using Abarabone.Misc;


    /// <summary>
    /// 
    /// </summary>
    public class StructureBuildingModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public StructureModel<UI32, StructureVertex> NearModel;
        public LodMeshModel<UI32, PositionNormalUvVertex> FarModel;

        public GameObject Envelope;
        public StructureBuildingModelAuthoring MasterPrefab;


        public override IEnumerable<IMeshModel> QueryModel =>
            new[] { this.NearModel as IMeshModel, this.FarModel };



        /// <summary>
        /// near と far のモデルエンティティを生成、
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            conversionSystem.CreateStructureEntities(this);

            trimEntities_(conversionSystem, this);

            return;


            static void trimEntities_(GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
            {
                var em = gcs.DstEntityManager;
                var near = st.NearModel.Obj;
                var nearent = gcs.GetPrimaryEntity(near);
                em.RemoveComponent<Translation>(nearent);
                em.RemoveComponent<Rotation>(nearent);
                em.RemoveComponent<Scale>(nearent);
                em.RemoveComponent<NonUniformScale>(nearent);

                foreach (var obj in st.gameObject.Children())
                {
                    var ent = gcs.GetPrimaryEntity(obj);
                    var shouldRemoving =
                        !em.HasComponent<LinkedEntityGroup>(ent) &&
                        !em.HasComponent<PhysicsCollider>(ent)
                        ;
                    if (shouldRemoving) em.DestroyEntity(ent);
                }
            }
        }


    }

    static public class StructureConvertUtility
    {

        static public void CreateStructureEntities
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var top = st.gameObject;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var main = env;

            st.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, main);
            initMainEntity_(gcs, top, main, st.NearModel, st.FarModel);

            setBoneForFarEntity_(gcs, main, far, top.transform);
            setBoneForPartEntities_(gcs, main, near, near.transform);
            //setBoneForPartEntities_(gcs, main, main, near.transform);
        }

        static public void CreateStructureEntitiesInArea
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring structureModel)
        {
            //var top = structureModel.gameObject;
            //var far = structureModel.FarMeshObject.objectTop;
            //var near = structureModel.NearMeshObject.objectTop;
            //var env = structureModel.Envelope;
            //var main = env;

            //createMeshAndSetToDictionary_(gcs, near, structureModel.GetNearMeshFunc);

            //createModelEntity_IfNotExists_(gcs, near, structureModel.NearMaterialToDraw);

            //initBinderEntity_(gcs, top, main);
            //initMainEntity_(gcs, top, main, structureModel.NearMeshObject, structureModel.FarMeshObject);

            //setBoneForFarEntity_(gcs, main, far, top.transform);
            //setBoneForPartEntities_(gcs, main, near, near.transform);
        }


        // ---------------------------------------------------------------------


        static void initBinderEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
        {
            var em_ = gcs_.DstEntityManager;

            var binderEntity = gcs_.GetPrimaryEntity(top_);
            var mainEntity = gcs_.GetPrimaryEntity(main_);


            var binderAddtypes = new ComponentTypes
            (
                typeof(BinderTrimBlankLinkedEntityGroupTag),
                typeof(ObjectBinder.MainEntityLinkData)
            );
            em_.AddComponents(binderEntity, binderAddtypes);

            em_.SetComponentData(binderEntity,
                new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


            em_.SetName_(binderEntity, $"{top_.name} binder");
        }

        static void initMainEntity_
            (GameObjectConversionSystem gcs, GameObject top, GameObject main, IMeshModelLod near, IMeshModelLod far)
        {
            var em_ = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var mainAddtypes = new ComponentTypes
            (
                new ComponentType[]
                {
                        typeof(Structure.MainTag),
                        typeof(DrawInstance.MeshTag),
                        //typeof(NonUniformScale),//暫定
                        typeof(ObjectMain.ObjectMainTag),
                        typeof(ObjectMain.BinderLinkData),
                        typeof(DrawInstance.ModeLinkData),
                        typeof(DrawInstance.TargetWorkData),
                        typeof(Structure.PartDestructionData),
                }
            );
            em_.AddComponents(mainEntity, mainAddtypes);

            //em_.SetComponentData(mainEntity,
            //    new NonUniformScale
            //    {
            //        Value = new float3(1, 1, 1)
            //    }
            //);
            em_.SetComponentData(mainEntity,
                new ObjectMain.BinderLinkData
                {
                    BinderEntity = binderEntity,
                }
            );
            em_.SetComponentData(mainEntity,
                new DrawInstance.ModeLinkData
                {
                    DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                    }
            );
            em_.SetComponentData(mainEntity,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );


            var draw = mainEntity;
            var lods = new IMeshModelLod[] { near, far };
            gcs.AddLod2ComponentToDrawInstanceEntity(draw, top, lods);


            gcs.InitPostureEntity(main);//, far.objectTop.transform);


            em_.SetName_(mainEntity, $"{top.name} main");
        }

        static void setBoneForFarEntity_(GameObjectConversionSystem gcs, GameObject parent, GameObject far, Transform root)
        {
            var qFar = Enumerable.Repeat(far.transform, 1);

            gcs.InitBoneEntities(parent, qFar, root, EnBoneType.jobs_per_depth);
        }


        static void setBoneForPartEntities_(GameObjectConversionSystem gcs, GameObject parent, GameObject partTop, Transform root)
        {
            var qPart = partTop.GetComponentsInChildren<StructurePartAuthoring>(true)
                .Select(pt => pt.transform);

            gcs.InitBoneEntities(parent, qPart, root, EnBoneType.jobs_per_depth);
        }



    }
}
