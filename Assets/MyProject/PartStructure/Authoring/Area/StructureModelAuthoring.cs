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

namespace Abarabone.Structure.Authoring2
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
    public class StructureModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        public Material NearMaterialToDraw;
        public Material FarMaterialToDraw;

        public ObjectAndDistance NearMeshObject;
        public ObjectAndDistance FarMeshObject;

        public GameObject Envelope;




        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    //var parts = this.GetComponentsInChildren<StructurePartAuthoring>();

        //    //var qMasterPrefab = parts
        //    //    .Select(x => x.gameObject)
        //    ////    .Select(x => UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(x))
        //    //    .Select(x => x.MasterPrefab)
        //    //    .Distinct();

        //    //referencedPrefabs.AddRange( qMasterPrefab );
        //}


        /// <summary>
        /// near と far のモデルエンティティを生成、
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var far = this.FarMeshObject.objectTop;
            var near = this.NearMeshObject.objectTop;
            var env = this.Envelope;

            createModelEntity_(conversionSystem, far, this.FarMaterialToDraw, this.GetFarMeshAndFunc);
            createModelEntity_(conversionSystem, near, this.NearMaterialToDraw, this.GetNearMeshFunc);

            initBinderEntity_(conversionSystem, top, env);
            initMainEntity_(conversionSystem, top, env, this.NearMeshObject, this.FarMeshObject);
            setBoneForFarEntity_(conversionSystem, env, far, far.transform.parent);
            setBoneForPartEntities_(conversionSystem, env, near, near.transform);

            return;



            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject go_, Material srcMaterial_,
                    Func<(GameObject go, Func<MeshCombinerElements> f, Mesh mesh)> meshCreateFunc_
                )
            {
                if (gcs_.IsExistsInModelEntityDictionary(go_)) return;

                var mesh = gcs_.GetFromMeshDictionary(go_);
                if(mesh == null)
                {
                    var x = meshCreateFunc_();
                    mesh = x.mesh ?? x.f().CreateMesh();
                    Debug.Log($"st model {go_.name} - {mesh.name}");
                    gcs_.AddToMeshDictionary(go_, mesh);
                }

                var mat = new Material(srcMaterial_);

                const BoneType boneType = BoneType.TR;
                const int boneLength = 1;
                const int vectorOffsetPerInstance = 4;

                gcs_.CreateDrawModelEntityComponents
                    (go_, mesh, mat, boneType, boneLength, vectorOffsetPerInstance);
            }


            void initBinderEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
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

            void initMainEntity_
                (GameObjectConversionSystem gcs_, GameObject top_, GameObject main_, ObjectAndDistance near_, ObjectAndDistance far_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                var mainAddtypes = new ComponentTypes
                (
                    new ComponentType[]
                    {
                        typeof(Structure.StructureMainTag),
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
                        DrawModelEntityCurrent = gcs_.GetFromModelEntityDictionary(top_),
                    }
                );
                em_.SetComponentData(mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );


                var draw = mainEntity;
                var lods = new ObjectAndDistance[] { near_, far_ };
                gcs_.AddLod2ComponentToDrawInstanceEntity(draw, top, lods);


                var qTffar = Enumerable.Repeat(far_.objectTop.transform, 1);
                gcs_.CreatePostureEntities(main_, qTffar);


                em_.SetName_(mainEntity, $"{top_.name} main");
            }

        }

        void setBoneForFarEntity_(GameObjectConversionSystem gcs_, GameObject main_, GameObject far_, Transform root_)
        {
            var qFar = Enumerable.Repeat(far_.transform, 1);

            gcs_.InitBoneEntities(main_, qFar, root_, EnBoneType.jobs_per_depth);
        }


        void setBoneForPartEntities_(GameObjectConversionSystem gcs_, GameObject main_, GameObject partTop_, Transform root_)
        {
            var qPart = partTop_.GetComponentsInChildren<StructurePartAuthoring>()
                .Select(pt => pt.transform);

            gcs_.InitBoneEntities(main_, qPart, root_, EnBoneType.jobs_per_depth);
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

            var f = !isFarSingle ? MeshCombiner.BuildNormalMeshElements(chilren, top.transform) : null;//far.transform) : null;
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

            var f = MeshCombiner.BuildStructureWithPalletMeshElements(objects, top.transform);//near.transform);

            Debug.Log($"near {near.name}");
            return (near, f, null);
        }
    }


}
