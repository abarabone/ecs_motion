using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;
using System;

namespace DotsLite.Structure.Authoring
{

    using DotsLite.Model;
    using DotsLite.Draw.Authoring;
	using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Character;//ObjectMain はここにある、名前変えるべきか？
    using DotsLite.Structure;
    using Unity.Physics;
    using Unity.Transforms;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;

    public class StructurePartAuthoring : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public int PartId;
        //public int Life;

        //public Material MaterialToDraw;

        //public GameObject MasterPrefab;

        public PartModel<UI32, PositionNormalUvVertex> PartModelOfMasterPrefab;



        public override IEnumerable<IMeshModel> QueryModel =>
            //new PartModel<UI32, PositionNormalUvVertex>(this.MasterPrefab, this.MaterialToDraw.shader)
            this.PartModelOfMasterPrefab
            .WrapEnumerable();



        /// <summary>
        /// パーツインスタンスエンティティを生成する。モデルは同じパーツで１つ。
        /// 破壊されたときの落下プレハブもインスタンス単位で作成しているが、破壊時に位置を仕込んでいるため、１つでよいのでは？
        /// </summary>
        public void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            //Debug.Log("pt auth "+this.name);

            //var topAuth = this.gameObject
            //    //.Ancestors()
            //    //.Select(go => go.GetComponent<StructureBuildingModelAuthoring>())
            //    //.First(x => x != null);
            //    .GetComponentsInParent<StructureBuildingModelAuthoring>(true)
            //    .FirstOrDefault();
            //var top = topAuth.gameObject;
            //var main = topAuth.FarMeshObject.objectTop;
            //var main = topAuth.FarModel.Obj;

            //return;
            //if (!dstManager.Exists(entity)) return;//


            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(conversionSystem);



            var posture = this.FindParent<StructureBuildingModelAuthoring>()
                .GetComponentInChildren<PostureAuthoring>();

            initPartData_(conversionSystem, posture, this, this.PartId);

            //createMeshAndSetToDictionary_(conversionSystem, this.MasterPrefab, this.GetPartsMeshesAndFuncs);
            //createModelEntity_IfNotExists_(conversionSystem, this.MasterPrefab, this.MaterialToDraw);
            var prefab = createDebrisPrefab_(conversionSystem, this.gameObject, this.PartModelOfMasterPrefab.Obj);
            var part = conversionSystem.GetPrimaryEntity(this.gameObject);

            setPrefabToPart_(conversionSystem, part, prefab);

            return;



            static void initPartData_
                (GameObjectConversionSystem gcs, PostureAuthoring main, StructurePartAuthoring part, int partId)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(part);
                var addtypes = new ComponentTypes
                (
                    typeof(Part.PartData),
                    typeof(Collision.Hit.TargetData),
                    typeof(Disabled)
                );
                em.AddComponents(ent, addtypes);


                em.SetComponentData(ent,
                    new Part.PartData
                    {
                        PartId = partId,
                        //Life = 
                    }
                );
                em.SetComponentData(ent,
                    new Collision.Hit.TargetData
                    {
                        MainEntity = gcs.GetPrimaryEntity(main),
                        HitType = Collision.HitType.part,
                    }
                );
            }


            //static void createMeshAndSetToDictionary_
            //    (GameObjectConversionSystem gcs, GameObject go, Func<(GameObject go, Func<MeshCombinerElements> f, Mesh mesh)> meshHolder)
            //{
            //    if (gcs.IsExistingInMeshDictionary(go)) return;

            //    var x = meshHolder();
            //    var newmesh = x.mesh ?? x.f().CreateMesh();

            //    Debug.Log($"part model {go.name} - {newmesh.name}");

            //    gcs.AddToMeshDictionary(go, newmesh);
            //}


            //static void createModelEntity_IfNotExists_
            //    (GameObjectConversionSystem gcs, GameObject masterPrefab, Material shader)
            //{
            //    if (gcs.IsExistsInModelEntityDictionary(masterPrefab)) return;

            //    var mesh = gcs.GetFromMeshDictionary(masterPrefab);
            //    var mat = new Material(shader);

            //    const BoneType BoneType = BoneType.TR;// あとでＳもつける
            //    const int boneLength = 1;

            //    var modelEntity_ = gcs.CreateDrawModelEntityComponents(masterPrefab, mesh, mat, BoneType, boneLength);
            //}

            // 同じプレハブをまとめることはできないだろうか？
            static Entity createDebrisPrefab_(GameObjectConversionSystem gcs, GameObject part, GameObject master)
            {
                var em_ = gcs.DstEntityManager;


                var types = em_.CreateArchetype
                (
                    typeof(PartDebris.Data),
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(PhysicsVelocity),//暫定
                    typeof(PhysicsGravityFactor),//暫定
                    typeof(PhysicsMass),//暫定
                    typeof(AddTransformConversion.Rotation),
                    typeof(AddTransformConversion.Translation),
                    //typeof(NonUniformScale),//暫定
                    typeof(Prefab)
                );
                //var prefabEnt = gcs_.CreateAdditionalEntity(part_, types);
                var prefabEnt = em_.CreateEntity(types);


                em_.SetComponentData(prefabEnt,
                    new PartDebris.Data
                    {
                        LifeTime = 5.0f,
                    }
                );

                //em_.SetComponentData(mainEntity,
                //    new NonUniformScale
                //    {
                //        Value = new float3(1, 1, 1)
                //    }
                //);
                em_.SetComponentData(prefabEnt,
                    new DrawInstance.ModelLinkData
                    {
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(master),
                    }
                );
                em_.SetComponentData(prefabEnt,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                //var mass = part_.GetComponent<PhysicsBodyAuthoring>().CustomMassDistribution;
                //var mass = em_.GetComponentData<PhysicsCollider>(gcs_.GetPrimaryEntity(part_)).MassProperties;
                em_.SetComponentData(prefabEnt,
                    //PhysicsMass.CreateDynamic( mass, 1.0f )
                    PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1.0f)// 暫定だっけ？
                );
                em_.SetComponentData(prefabEnt,
                    new PhysicsGravityFactor
                    {
                        Value = 1.0f,
                    }
                );


                em_.SetName_(prefabEnt, $"{part.name} debris");

                return prefabEnt;
            }

            void setPrefabToPart_(GameObjectConversionSystem gcs, Entity part, Entity prefab)
            {
                var em = gcs.DstEntityManager;

                em.AddComponentData(part,
                    new Part.DebrisPrefabData
                    {
                        DebrisPrefab = prefab,
                    }
                );
            }
        }



        ///// <summary>
        ///// 元となったプレハブから自身と子を合成する関数を取得する。ただし子からパーツは除外する。
        ///// また、オブジェクトが１つの時は直接メッシュを取得する。
        ///// </summary>
        //public (GameObject go, Func<MeshCombinerElements> f, Mesh mesh) GetPartsMeshesAndFuncs()
        //{

        //    var part = this.MasterPrefab;
        //    var children = queryPartBodyObjects_Recursive_(part);//.ToArray();

        //    var isSingle = children.IsSingle();//children.Length == 1;
        //    var f = !isSingle
        //        ? MeshCombiner.BuildNormalMeshElements(children, part.transform)
        //        : null;
        //    var mesh = isSingle
        //        ? children.First().GetComponent<MeshFilter>().sharedMesh
        //        : null;

        //    return (part, f, mesh);


        //    static IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go)
        //    {
        //        var q =
        //            from child in go.Children()
        //            where child.GetComponent<StructurePartAuthoring>() == null
        //            from x in queryPartBodyObjects_Recursive_(child)
        //            select x
        //            ;
        //        return q.Prepend(go);
        //    }
        //}
    }
}
