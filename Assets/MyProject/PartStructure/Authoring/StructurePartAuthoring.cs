using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;

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

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;

    public class StructurePartAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        public int PartId;
        //public int Life;

        public Material Material;

        public GameObject MasterPrefab;



        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    Debug.Log(this.name+" prefab");
        //    //var go = Instantiate(this.gameObject);
        //    //referencedPrefabs.Add(go);
        //}


        /// <summary>
        /// 
        /// </summary>
        public async void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            Debug.Log("pt auth "+this.name);

            var top = this.gameObject.Ancestors().First( go => go.GetComponent<StructureModelAuthoring>() );
            var objA = top.transform.GetChild(0).gameObject;

            //var go = Instantiate(this.gameObject);
            //go.AddComponent<PhysicsBodyAuthoring>();
            //Debug.Log(conversionSystem.GetPrimaryEntity(go));

            setMainLink_(conversionSystem, objA, this.gameObject);
            initPartData_(conversionSystem, this.gameObject, this.PartId);

            createModelEntity_(conversionSystem, this.MasterPrefab, this.Material);
            createDebrisPrefab_(conversionSystem, this.gameObject, this.MasterPrefab);

            return;


            void setMainLink_(GameObjectConversionSystem gcs, GameObject main, GameObject part)
            {
                var em = gcs.DstEntityManager;

                var partent = gcs.GetPrimaryEntity(part);
                var mainent = gcs.GetPrimaryEntity(main);

                em.AddComponentData(partent,
                    new Bone.MainEntityLinkData
                    {
                        MainEntity = mainent,
                    }
                );
            }

            void initPartData_
                (GameObjectConversionSystem gcs, GameObject part, int partId)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(part);

                em.AddComponentData(ent,
                    new StructurePart.PartData
                    {
                        PartId = partId,
                        //Life = 
                    }
                );
            }



            void createModelEntity_
                (GameObjectConversionSystem gcs_, GameObject master_, Material srcMaterial_)
            {
                if (gcs_.IsExistsInModelEntityDictionary(master_)) return;

                var mat = new Material(srcMaterial_);
                var mesh = conversionSystem.GetFromStructureMeshDictionary(this.MasterPrefab);

                const BoneType BoneType = BoneType.TR;// あとでＳもつける
                const int boneLength = 1;

                var modelEntity_ = gcs_.CreateDrawModelEntityComponents(master_, mesh, mat, BoneType, boneLength);
            }

            void createDebrisPrefab_(GameObjectConversionSystem gcs_, GameObject part_, GameObject master_)
            {
                var em_ = gcs_.DstEntityManager;


                var types = em_.CreateArchetype
                (
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


                //em_.SetComponentData(mainEntity,
                //    new NonUniformScale
                //    {
                //        Value = new float3(1, 1, 1)
                //    }
                //);
                em_.SetComponentData(prefabEnt,
                    new DrawInstance.ModeLinkData
                    {
                        DrawModelEntity = gcs_.GetFromModelEntityDictionary(master_),
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


    }
}
