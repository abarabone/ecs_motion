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
using Unity.Physics.Authoring;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Character;//ObjectMain はここにある、名前変えるべきか？

    using DotsLite.Common.Extension;
    using DotsLite.Structure;
    using Unity.Entities.UniversalDelegates;
    using Unity.Properties;
    using System.CodeDom;
    using DotsLite.Utilities;
    using DotsLite.Misc;






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
            new IMeshModel [] { this.NearModel, this.FarModel };


        public PartColliderMode ColliderMode;
        public enum PartColliderMode
        {
            separate,   // パーツごとにコライダを持つ。
            compound,   // すべてのコライダを結合する。未実装
            mesh,       // １つのメッシュコライダーにする。未実装
        }



        /// <summary>
        /// near と far のモデルエンティティを生成、
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


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

            //conversionSystem.AddHitTargetsAllRigidBody(this, )

        }


    }

    static public class StructureConvertUtility
    {

        static public void CreateStructureEntities
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
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

            setBoneForFarEntity_(gcs, posture, far, top.transform);
            setBoneForPartMultiEntities_(gcs, posture, parts, near.transform);

            trimEntities_(gcs, st);
            //if (env.GetComponent<DynamicToStaticRigidBody>().As() != null) addExcludeTransform_(gcs, far, near);
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
            var top = st;
            var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var posture = st.GetComponentInChildren<PostureAuthoring>();
            var parts = near.GetComponentsInChildren<StructurePartAuthoring>();

            st.QueryModel.CreateMeshAndModelEntitiesWithDictionary(gcs);

            initBinderEntity_(gcs, top, posture);
            initMainEntity_(gcs, top, posture, st.NearModel, st.FarModel, parts.Length);

            initMeshColliderEntity(gcs, near);

            setBoneForFarEntity_(gcs, posture, far, top.transform);
            setBoneForNearSingleEntity_(gcs, posture, near, near.transform);

            trimEntities_(gcs, st);
            orderTrimEntities_(gcs, st);
        }

        static public void CreateStructureEntities_Compound
            (this GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
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

            setBoneForFarEntity_(gcs, posture, far, top.transform);
            setBoneForNearSingleEntity_(gcs, posture, near, near.transform);

            trimEntities_(gcs, st);
            orderTrimEntities_(gcs, st);
        }



        // ---------------------------------------------------------------------

        /// <summary>
        /// main, near, far, part 以外の entity を削除する
        /// </summary>
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
        /// <summary>
        /// part entity の遅延的破棄をセットする
        /// </summary>
        static void orderTrimEntities_(GameObjectConversionSystem gcs, StructureBuildingModelAuthoring st)
        {
            var em = gcs.DstEntityManager;

            var qEnt = st.GetComponentsInChildren<StructurePartAuthoring>()
                .Select(x => gcs.GetPrimaryEntity(x));
            em.AddComponentData(qEnt, new LateDestroyEntityConversion.TargetTag { });
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
                    typeof(Main.SleepTimerData),
                    typeof(DrawInstance.NeedLodCurrentTag),
                    typeof(Main.PartInfoData),
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
                new Main.PartInfoData
                {
                    PartLength = partLength,
                    LivePartLength = partLength,
                }
            );
            em.SetComponentData(mainEntity,
                new Main.SleepTimerData
                {
                    PrePositionAndTime = new float4(0, 0, 0, Main.SleepTimerData.Margin),
                }
            );

            var draw = mainEntity;
            var lods = new IMeshModelLod[] { near, far };
            gcs.AddLod2ComponentToDrawInstanceEntity(draw, top.gameObject, lods);


            gcs.InitPostureEntity(main);//, far.objectTop.transform);


            em.SetName_(mainEntity, $"{top.name} main");
        }



        static void setBoneForFarEntity_
            (GameObjectConversionSystem gcs, PostureAuthoring parent, GameObject far, Transform root)
        {
            var qFar = far.transform.WrapEnumerable();

            gcs.InitBoneEntities(parent, qFar, root, EnBoneType.jobs_per_depth);
        }


        static void setBoneForPartMultiEntities_
            (GameObjectConversionSystem gcs, PostureAuthoring parent, IEnumerable<StructurePartAuthoring> parts, Transform root)
        {
            var qTfPart = parts.Select(pt => pt.transform);

            gcs.InitBoneEntities(parent, qTfPart, root, EnBoneType.jobs_per_depth);
        }

        static void setBoneForNearSingleEntity_
            (GameObjectConversionSystem gcs, PostureAuthoring parent, GameObject near, Transform root)
        {
            var qNear = near.transform.WrapEnumerable();

            gcs.InitBoneEntities(parent, qNear, root, EnBoneType.jobs_per_depth);
        }


        //static void addExcludeTransform_(
        //    GameObjectConversionSystem gcs, GameObject far, GameObject partTop)
        //{
        //    var qFar = far.transform.WrapEnumerable();
        //    var qPart = partTop.GetComponentsInChildren<StructurePartAuthoring>()//true)
        //        .Select(pt => pt.transform);

        //    qFar.Concat(qPart)
        //        .Select(x => gcs.GetPrimaryEntity(x))
        //        .ForEach(x => gcs.DstEntityManager.AddComponent<Model.TransformOption.ExcludeTransformTag>(x));
        //}





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
