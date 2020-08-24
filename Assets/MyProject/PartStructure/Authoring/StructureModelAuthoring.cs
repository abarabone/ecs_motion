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
    public class StructureModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        public Material NearMaterialToDraw;
        public Material FarMaterialToDraw;

        public ObjectAndDistance NearMeshObject;
        public ObjectAndDistance FarMeshObject;




        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            //var parts = this.GetComponentsInChildren<StructurePartAuthoring>();

            //var qMasterPrefab = parts
            //    .Select(x => x.gameObject)
            ////    .Select(x => UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(x))
            //    .Select(x => x.MasterPrefab)
            //    .Distinct();

            //referencedPrefabs.AddRange( qMasterPrefab );
        }


        /// <summary>
        /// 
        /// </summary>
        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var far = this.FarMeshObject.objectTop;
            var near = this.NearMeshObject.objectTop;

            createModelEntity_(conversionSystem, far, this.FarMaterialToDraw, this.GetFarMeshAndFunc);
            createModelEntity_(conversionSystem, near, this.NearMaterialToDraw, this.GetNearMeshFunc);
            //createDrawInstanceEntity_(conversionSystem, top);

            initBinderEntity_(conversionSystem, top, far);
            initMainEntity_(conversionSystem, top, far);

            //setPartLink_(conversionSystem, far, near);
            //setPartLocalPosition_(conversionSystem, far, near);
            //setPartId_(conversionSystem, near);

            var draw = conversionSystem.GetPrimaryEntity(far);
            var lods = new ObjectAndDistance[] { this.NearMeshObject, this.FarMeshObject };
            conversionSystem.AddLod2ComponentToDrawInstanceEntity(draw, top, lods);

            return;



            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject go_, Material srcMaterial_,
                    Func<(GameObject go, Func<MeshElements> f, Mesh mesh)> meshCreateFunc_
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


            //Entity createDrawInstanceEntity_
            //    (GameObjectConversionSystem gcs, GameObject top_)
            //{
            //    var em = gcs.DstEntityManager;


            //    var archetype = em.CreateArchetype
            //    (
            //        typeof(DrawInstance.ModeLinkData),
            //        typeof(DrawInstance.TargetWorkData)
            //    );
            //    var ent = gcs.CreateAdditionalEntity(top_, archetype);

            //    em.SetComponentData(ent,
            //        new DrawInstance.ModeLinkData
            //        {
            //            DrawModelEntity = gcs.GetFromModelEntityDictionary(top_),
            //        }
            //    );
            //    em.SetComponentData(ent,
            //        new DrawInstance.TargetWorkData
            //        {
            //            DrawInstanceId = -1,
            //        }
            //    );


            //    em.SetName_(ent, $"{top_.name} draw");
            //    return ent;
            //}


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

            void initMainEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
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

                em_.SetName_(mainEntity, $"{top_.name} main");
            }

        }


        //void setPartLink_(GameObjectConversionSystem gcs_, GameObject main_, GameObject partTop_)
        //{

        //    var mainEntity = gcs_.GetPrimaryEntity(main_);


        //    var partEntities = partTop_.GetComponentsInChildren<StructurePartAuthoring>()
        //        .Select(pt => pt.gameObject)
        //        .Select(go => gcs_.GetPrimaryEntity(go))
        //        .ToArray();

        //    var qPartLinkData =
        //        from ptent in partEntities.Skip(1).Append(Entity.Null)
        //        select new Structure.PartLinkData
        //        {
        //            NextEntity = ptent,
        //        };

        //    var em = gcs_.DstEntityManager;
        //    em.AddComponentData(mainEntity, new Structure.PartLinkData { NextEntity = partEntities.First() });
        //    em.AddComponentData(partEntities, qPartLinkData);

        //}


        //void setPartLocalPosition_(GameObjectConversionSystem gcs_, GameObject main_, GameObject partTop_)
        //{

        //    var mtMain = main_.transform.worldToLocalMatrix;

        //    var parts = partTop_.GetComponentsInChildren<StructurePartAuthoring>();

        //    var qPartEntity = parts
        //        .Select(pt => pt.gameObject)
        //        .Select(go => gcs_.GetPrimaryEntity(go))
        //        ;
        //    var qPartLocalPosition =
        //        from pt in parts
        //        select new StructurePart.LocalPositionData
        //        {
        //            Translation = mtMain.MultiplyPoint( pt.transform.position ),
        //            Rotation = mtMain.rotation * pt.transform.rotation
        //        };

        //    var em = gcs_.DstEntityManager;
        //    em.AddComponentData(qPartEntity, qPartLocalPosition);

        //}


        //void setPartId_(GameObjectConversionSystem gcs_, GameObject partTop_)
        //{

        //    var parts = partTop_.GetComponentsInChildren<StructurePartAuthoring>();

        //    var qPartEntity = parts
        //        .Select(pt => pt.gameObject)
        //        .Select(go => gcs_.GetPrimaryEntity(go))
        //        ;
        //    var qPartData =
        //        from i in Enumerable.Range(0, parts.Length)
        //        select new StructurePart.PartData
        //        {
        //            PartId = i,
        //        };

        //    var em = gcs_.DstEntityManager;
        //    em.AddComponentData(qPartEntity, qPartData);

        //}


        public (GameObject go, Func<MeshElements> f, Mesh mesh) GetFarMeshAndFunc()
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

        public (GameObject go, Func<MeshElements> f, Mesh mesh) GetNearMeshFunc()
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
