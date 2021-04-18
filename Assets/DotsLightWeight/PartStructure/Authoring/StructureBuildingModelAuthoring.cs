﻿using System.Collections;
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
using Unity.Physics.Authoring;

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


        public PartColliderMode ColliderMode;
        public enum PartColliderMode
        {
            separate,
            compound,
            mesh,
        }



        /// <summary>
        /// near と far のモデルエンティティを生成、
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            switch (this.ColliderMode)
            {
                case PartColliderMode.separate:
                    conversionSystem.CreateStructureEntities(this);
                    break;
                case PartColliderMode.compound:
                    conversionSystem.CreateStructureEntities_Compound(this);
                    break;
                case PartColliderMode.mesh:
                    conversionSystem.CreateStructureEntities_MeshCollider(this);
                    break;
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
            setBoneForPartMultiEntities_(gcs, main, near, near.transform);

            trimEntities_(gcs, st);
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


        static public void CreateStructureEntities_MeshCollider
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var top = st.gameObject;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var main = env;
            //var patrs = near.GetComponentsInChildren<StructurePartAuthoring>();

            st.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, main);
            initMainEntity_(gcs, top, main, st.NearModel, st.FarModel);

            initMeshColliderEntity(gcs, near);

            setBoneForFarEntity_(gcs, main, far, top.transform);
            setBoneForNearSingleEntity_(gcs, main, near, near.transform);

            trimEntities_(gcs, st);
            orderTrimEntities(gcs, st);
        }

        static public void CreateStructureEntities_Compound
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var top = st.gameObject;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var main = env;
            //var patrs = near.GetComponentsInChildren<StructurePartAuthoring>();

            st.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, main);
            initMainEntity_(gcs, top, main, st.NearModel, st.FarModel);

            initCompoundColliderEntity(gcs, near);

            setBoneForFarEntity_(gcs, main, far, top.transform);
            setBoneForNearSingleEntity_(gcs, main, near, near.transform);

            trimEntities_(gcs, st);
            orderTrimEntities(gcs, st);
        }



        // ---------------------------------------------------------------------

        static void trimEntities_(GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var em = gcs.DstEntityManager;

            //var top = st.gameObject;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var main = env;

            var needs = st.GetComponentsInChildren<StructurePartAuthoring>()
                .Select(x => x.gameObject)
                //.Append(top)
                .Append(main)
                .Append(near)
                .Append(far)
                ;
            foreach (var obj in st.gameObject.Descendants().Except(needs))
            {
                var ent = gcs.GetPrimaryEntity(obj);
                em.DestroyEntity(ent);
            }
        }
        static void orderTrimEntities(GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var em = gcs.DstEntityManager;

            var qEnt = st.GetComponentsInChildren<StructurePartAuthoring>()
                .Select(x => gcs.GetPrimaryEntity(x));
            em.AddComponentData(qEnt, new LateDestroyEntityConversion.TargetTag { });
        }

        static void initBinderEntity_(GameObjectConversionSystem gcs, GameObject top, GameObject main)
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

        static void initMainEntity_
            (GameObjectConversionSystem gcs, GameObject top, GameObject main, IMeshModelLod near, IMeshModelLod far)
        {
            var em = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var mainAddtypes = new ComponentTypes
            (
                new ComponentType[]
                {
                    typeof(Structure.MainTag),
                    typeof(DrawInstance.MeshTag),
                    //typeof(NonUniformScale),//暫定
                    //typeof(ObjectMain.ObjectMainTag),
                    typeof(ObjectMain.BinderLinkData),
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Structure.PartDestructionData),
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
                new ObjectMain.BinderLinkData
                {
                    BinderEntity = binderEntity,
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.ModeLinkData
                {
                    DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );


            var draw = mainEntity;
            var lods = new IMeshModelLod[] { near, far };
            gcs.AddLod2ComponentToDrawInstanceEntity(draw, top, lods);


            gcs.InitPostureEntity(main);//, far.objectTop.transform);


            em.SetName_(mainEntity, $"{top.name} main");
        }


        static void setBoneForFarEntity_(GameObjectConversionSystem gcs, GameObject parent, GameObject far, Transform root)
        {
            var qFar = far.transform.WrapEnumerable();

            gcs.InitBoneEntities(parent, qFar, root, EnBoneType.jobs_per_depth);
        }


        static void setBoneForPartMultiEntities_(GameObjectConversionSystem gcs, GameObject parent, GameObject partTop, Transform root)
        {
            var qPart = partTop.GetComponentsInChildren<StructurePartAuthoring>()//true)
                .Select(pt => pt.transform);

            gcs.InitBoneEntities(parent, qPart, root, EnBoneType.jobs_per_depth);
        }

        static void setBoneForNearSingleEntity_(GameObjectConversionSystem gcs, GameObject parent, GameObject near, Transform root)
        {
            var qNear = near.transform.WrapEnumerable();

            gcs.InitBoneEntities(parent, qNear, root, EnBoneType.jobs_per_depth);
        }





        static void initMeshColliderEntity(GameObjectConversionSystem gcs, GameObject near)
        {
            var em = gcs.DstEntityManager;

            var mesh = gcs.GetFromMeshDictionary(near);
            using var data = Mesh.AcquireReadOnlyMeshData(mesh);
            using var vtxs = new NativeArray<Vector3>(mesh.vertexCount, Allocator.Temp);
            using var idxs = new NativeArray<int>((int)mesh.GetIndexCount(0) * 3, Allocator.Temp);
            data[0].GetVertices(vtxs);
            data[0].GetIndices(idxs, 0);
            var collider = Unity.Physics.MeshCollider.Create
                (vtxs.Reinterpret<float3>(), idxs.Reinterpret<int3>(sizeof(int)));

            var ent = gcs.GetPrimaryEntity(near);
            em.AddComponentData(ent, new PhysicsCollider
            {
                Value = collider,
            });
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

        //static BlobAssetReference<Unity.Physics.Collider> getProperty(this PhysicsShapeAuthoring shape)
        //{
        //    switch (shape.ShapeType)
        //    {
        //        case Unity.Physics.Authoring.ShapeType.Box:
        //            return Unity.Physics.BoxCollider.Create(shape.GetBoxProperties());

        //        case Unity.Physics.Authoring.ShapeType.Capsule:
        //            return Unity.Physics.CapsuleCollider.Create(shape.GetCapsuleProperties().ToRuntime());

        //        case Unity.Physics.Authoring.ShapeType.ConvexHull:
        //            //=> Unity.Physics.ConvexCollider.Create(shape.GetConvexHullProperties()),
        //            break;

        //        case Unity.Physics.Authoring.ShapeType.Cylinder:
        //            return Unity.Physics.CylinderCollider.Create(shape.GetCylinderProperties());

        //        case Unity.Physics.Authoring.ShapeType.Mesh:
        //            {
        //                using var v = new NativeList<float3>(Allocator.Temp);
        //                using var t = new NativeList<int3>(Allocator.Temp);
        //                shape.GetMeshProperties(v, t);
        //                return Unity.Physics.MeshCollider.Create(v, t);
        //            }
        //        case Unity.Physics.Authoring.ShapeType.Plane:
        //            //shape.GetPlaneProperties(out var c, out var s, out var o);
        //            //return Unity.Physics.PolygonCollider.CreateQuad(c, s, o);
        //            break;
        //        case Unity.Physics.Authoring.ShapeType.Sphere:
        //            var g = shape.GetSphereProperties(out var o);
        //            return Unity.Physics.SphereCollider.Create(g);
        //    }
        //    return new BlobAssetReference<Unity.Physics.Collider>();
        //}

    }
}
