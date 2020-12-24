using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;
using System;

namespace Abarabone.Structure.Authoring
{

    using Abarabone.Model;
    using Abarabone.Draw.Authoring;
	using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Character;//ObjectMain はここにある、名前変えるべきか？
    using Abarabone.Structure;
    using Unity.Physics;
    using Unity.Transforms;
    using Abarabone.Geometry;
    using Abarabone.Utilities;

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;

    public class StructurePartAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public int PartId;
        //public int Life;

        public Material MaterialToDraw;

        public GameObject MasterPrefab;



        /// <summary>
        /// パーツインスタンスエンティティを生成する。モデルは同じパーツで１つ。
        /// 破壊されたときの落下プレハブもインスタンス単位で作成しているが、破壊時に位置を仕込んでいるため、１つでよいのでは？
        /// </summary>
        public async void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            Debug.Log("pt auth "+this.name);

            var topAuth = this.gameObject
                .Ancestors()
                .Select(go => go.GetComponent<StructureModelAuthoring>())
                .First(x => x != null);
            var top = topAuth.gameObject;
            var main = topAuth.FarMeshObject.objectTop;

            initPartData_(conversionSystem, this.gameObject, this.PartId);

            createModelEntity_(conversionSystem, this.MasterPrefab, this.MaterialToDraw);
            createDebrisPrefab_(conversionSystem, this.gameObject, this.MasterPrefab);

            return;



            void initPartData_
                (GameObjectConversionSystem gcs, GameObject part, int partId)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(part);
                var addtypes = new ComponentTypes
                (
                    typeof(StructurePart.PartData),
                    typeof(Disabled)
                );
                em.AddComponents(ent, addtypes);


                em.SetComponentData(ent,
                    new StructurePart.PartData
                    {
                        PartId = partId,
                        //Life = 
                    }
                );
            }


            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject masterPrefab, Material shader)
            {
                if (gcs.IsExistsInModelEntityDictionary(masterPrefab)) return;

                var mat = new Material(shader);
                var mesh = conversionSystem.GetFromMeshDictionary(masterPrefab);
                if(mesh == null)
                {
                    var x = this.GetPartsMeshesAndFuncs();
                    mesh = x.mesh ?? x.f().CreateMesh();
                    gcs.AddToMeshDictionary(masterPrefab, mesh);
                }

                const BoneType BoneType = BoneType.TR;// あとでＳもつける
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(masterPrefab, mesh, mat, BoneType, boneLength);
            }

            void createDebrisPrefab_(GameObjectConversionSystem gcs_, GameObject part_, GameObject master_)
            {
                var em_ = gcs_.DstEntityManager;


                var types = em_.CreateArchetype
                (
                    typeof(StructurePartDebris.Data),
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(PhysicsVelocity),//暫定
                    typeof(PhysicsGravityFactor),//暫定
                    typeof(PhysicsMass),//暫定
                    typeof(Rotation),
                    typeof(Translation),
                    //typeof(NonUniformScale),//暫定
                    typeof(Prefab)
                );
                //var prefabEnt = gcs_.CreateAdditionalEntity(part_, types);
                var prefabEnt = em_.CreateEntity(types);


                em_.SetComponentData(prefabEnt,
                    new StructurePartDebris.Data
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
                    new DrawInstance.ModeLinkData
                    {
                        DrawModelEntityCurrent = gcs_.GetFromModelEntityDictionary(master_),
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
                    PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1.0f)
                );
                em_.SetComponentData(prefabEnt,
                    new PhysicsGravityFactor
                    {
                        Value = 1.0f,
                    }
                );


                var partEnt = gcs_.GetPrimaryEntity(part_);

                em_.AddComponentData(partEnt,
                    new StructurePart.DebrisPrefabData
                    {
                        DebrisPrefab = prefabEnt,
                    }
                );


                em_.SetName_(prefabEnt, $"{part_.name} debris");
            }
        }


        /// <summary>
        /// 元となったプレハブから自身と子を合成する関数を取得する。ただし子からパーツは除外する。
        /// また、オブジェクトが１つの時は直接メッシュを取得する。
        /// </summary>
        public (GameObject go, Func<MeshCombinerElements> f, Mesh mesh) GetPartsMeshesAndFuncs()
        {

            var part = this.MasterPrefab;
            var children = queryPartBodyObjects_Recursive_(part);//.ToArray();

            var isSingle = children.IsSingle();//children.Length == 1;
            var f = !isSingle
                ? MeshCombiner.BuildNormalMeshElements(children, part.transform)
                : null;
            var mesh = isSingle
                ? children.First().GetComponent<MeshFilter>().sharedMesh
                : null;

            return (part, f, mesh);


            static IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go)
            {
                var q =
                    from child in go.Children()
                    where child.GetComponent<StructurePartAuthoring>() == null
                    from x in queryPartBodyObjects_Recursive_(child)
                    select x
                    ;
                return q.Prepend(go);
            }
        }
    }
}
