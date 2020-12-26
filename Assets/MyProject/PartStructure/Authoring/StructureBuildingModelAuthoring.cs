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

    /// <summary>
    /// 
    /// </summary>
    public class StructureBuildingModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Material NearMaterialToDraw;
        public Material FarMaterialToDraw;

        public ObjectAndDistance NearMeshObject;
        public ObjectAndDistance FarMeshObject;

        public GameObject Envelope;
        public StructureBuildingModelAuthoring MasterPrefab;



        /// <summary>
        /// near と far のモデルエンティティを生成、
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefab = this.MasterPrefab;
            if (prefab == null) return;

            conversionSystem.CreateStructureEntities(prefab);
        }


        public (GameObject go, Func<MeshCombinerElements> f, Mesh mesh) GetFarMeshAndFunc()
        {
            var top = this.gameObject;
            var far = this.FarMeshObject.objectTop;

            var chilren = far
                .DescendantsAndSelf()
                .Where(child => child.GetComponent<MeshFilter>() != null)
                .ToArray();

            var isFarSingle = chilren.Length == 1 && isSameTransform_(chilren.First(), far);

            var f = !isFarSingle ? MeshCombiner.BuildNormalMeshElements(chilren, top.transform) : null;//far.transform) : null;//
            var mesh = isFarSingle ? chilren.First().GetComponent<MeshFilter>().sharedMesh : null;

            Debug.Log($"far {far.name} {chilren.Length} {isFarSingle}");
            return (far, f, mesh);


            bool isSameTransform_(GameObject target_, GameObject top_) =>
                (target_.transform.localToWorldMatrix * top_.transform.worldToLocalMatrix).isIdentity;
        }

        public (GameObject go, Func<MeshCombinerElements> f, Mesh mesh) GetNearMeshFunc()
        {
            var top = this.gameObject;
            var near = this.NearMeshObject.objectTop;

            var objects = near.DescendantsAndSelf();

            var f = MeshCombiner.BuildStructureWithPalletMeshElements(objects, top.transform);//near.transform);//

            Debug.Log($"near {near.name}");
            return (near, f, null);
        }
    }

    static public class StructureConvertUtility
    {

        static public void CreateStructureEntities
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring structureModel)
        {
            var top = structureModel.gameObject;
            var far = structureModel.FarMeshObject.objectTop;
            var near = structureModel.NearMeshObject.objectTop;
            var env = structureModel.Envelope;// main object

            createModelEntity_(gcs, far, structureModel.FarMaterialToDraw, structureModel.GetFarMeshAndFunc);
            createModelEntity_(gcs, near, structureModel.NearMaterialToDraw, structureModel.GetNearMeshFunc);

            initBinderEntity_(gcs, top, env);
            initMainEntity_(gcs, top, env, structureModel.NearMeshObject, structureModel.FarMeshObject);

            setBoneForFarEntity_(gcs, env, far, top.transform);// far.transform.parent);
            setBoneForPartEntities_(gcs, env, near, top.transform);// near.transform);
        }

        static public void CreateStructureEntitiesInArea
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring structureModel)
        {
            var top = structureModel.gameObject;
            var far = structureModel.FarMeshObject.objectTop;
            var near = structureModel.NearMeshObject.objectTop;
            var env = structureModel.Envelope;// main object

            createModelEntity_(gcs, near, structureModel.NearMaterialToDraw, structureModel.GetNearMeshFunc);

            initBinderEntity_(gcs, top, env);
            initMainEntity_(gcs, top, env, structureModel.NearMeshObject, structureModel.FarMeshObject);

            setBoneForFarEntity_(gcs, env, far, top.transform);// far.transform.parent);
            setBoneForPartEntities_(gcs, env, near, top.transform);// near.transform);
        }


        // ---------------------------------------------------------------------

        // メッシュの位置は、main object の位置となるようにすること
        static void createModelEntity_
            (
                GameObjectConversionSystem gcs, GameObject go, Material srcMaterial,
                Func<(GameObject, Func<MeshCombinerElements>, Mesh)> meshCreateFunc
            )
        {
            if (gcs.IsExistsInModelEntityDictionary(go)) return;


            var mesh = getOrCreateMesh_(gcs, go, meshCreateFunc);
            var mat = new Material(srcMaterial);

            const BoneType boneType = BoneType.TR;
            const int boneLength = 1;
            const int vectorOffsetPerInstance = 4;

            gcs.CreateDrawModelEntityComponents
                (go, mesh, mat, boneType, boneLength, vectorOffsetPerInstance);

            return;


            static Mesh getOrCreateMesh_(
                GameObjectConversionSystem gcs, GameObject go,
                Func<(GameObject go, Func<MeshCombinerElements> f, Mesh mesh)> meshCreateFunc)
            {
                var existingMesh = gcs.GetFromMeshDictionary(go);
                if (existingMesh != null) return existingMesh;

                var x = meshCreateFunc();
                var newmesh = x.mesh ?? x.f().CreateMesh();

                Debug.Log($"st model {go.name} - {newmesh.name}");

                gcs.AddToMeshDictionary(go, newmesh);
                return newmesh;
            }
        }


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
            (GameObjectConversionSystem gcs, GameObject top, GameObject main, ObjectAndDistance near, ObjectAndDistance far)
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
            var lods = new ObjectAndDistance[] { near, far };
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
            var qPart = partTop.GetComponentsInChildren<StructurePartAuthoring>()
                .Select(pt => pt.transform)
                .Append(partTop.transform);// うまくできてない

            gcs.InitBoneEntities(parent, qPart, root, EnBoneType.jobs_per_depth);
        }

    }
}
