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

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;

    public class StructurePartAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public int PartId;
        //public int Life;

        public Material MaterialToDraw;

        public GameObject MasterPrefab;



        /// <summary>
        /// 
        /// </summary>
        public async void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            Debug.Log("pt auth "+this.name);

            var topAuth = this.gameObject.Ancestors().Select( go => go.GetComponent<StructureModelAuthoring>() ).First(x => x != null);
            var top = topAuth.gameObject;
            var main = topAuth.FarMeshObject.objectTop;

            //setMainLink_(conversionSystem, main, this.gameObject);
            initPartData_(conversionSystem, this.gameObject, this.PartId);
            //setLocalPosition_(conversionSystem, main, this.gameObject);

            createModelEntity_(conversionSystem, this.MasterPrefab, this.MaterialToDraw);
            createDebrisPrefab_(conversionSystem, this.gameObject, this.MasterPrefab);

            return;


            //void setMainLink_(GameObjectConversionSystem gcs_, GameObject main_, GameObject part_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var partent = gcs_.GetPrimaryEntity(part_);
            //    var mainent = gcs_.GetPrimaryEntity(main_);

            //    em.AddComponentData(partent,
            //        new Bone.MainEntityLinkData
            //        {
            //            MainEntity = mainent,
            //        }
            //    );
            //}

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

            //void setLocalPosition_(GameObjectConversionSystem gcs_, GameObject main_, GameObject part_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var mtInvMain = main_.transform.worldToLocalMatrix;

            //    var partent = gcs_.GetPrimaryEntity(part_);
            //    em.AddComponentData( partent,
            //        new StructurePart.LocalPositionData
            //        {
            //            Translation = mtInvMain.MultiplyPoint(part_.transform.position),
            //            Rotation = mtInvMain.rotation * part_.transform.rotation
            //        }
            //    );

            //}


            void createModelEntity_
                (GameObjectConversionSystem gcs_, GameObject master_, Material srcMaterial_)
            {
                if (gcs_.IsExistsInModelEntityDictionary(master_)) return;

                var mat = new Material(srcMaterial_);
                var mesh = conversionSystem.GetFromMeshDictionary(this.MasterPrefab);
                if(mesh == null)
                {
                    var x = this.GetPartsMeshesAndFuncs();
                    mesh = x.mesh ?? x.f().CreateMesh();
                    gcs_.AddToMeshDictionary(master_, mesh);
                }

                const BoneType BoneType = BoneType.TR;// あとでＳもつける
                const int boneLength = 1;

                var modelEntity_ = gcs_.CreateDrawModelEntityComponents(master_, mesh, mat, BoneType, boneLength);
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


        public (GameObject go, Func<MeshElements> f, Mesh mesh) GetPartsMeshesAndFuncs()
        {

            var part = this.MasterPrefab;
            var children = queryPartBodyObjects_Recursive_(part).ToArray();

            var isSingle = children.Length == 1;
            var f = !isSingle ? MeshCombiner.BuildNormalMeshElements(children, part.transform) : null;
            var mesh = isSingle ? children.First().GetComponent<MeshFilter>().sharedMesh : null;

            return (part, f, mesh);


            IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go_)
            {
                var q =
                    from child in go_.Children()
                    where child.GetComponent<StructurePartAuthoring>() == null
                    from x in queryPartBodyObjects_Recursive_(child)
                    select x
                    ;
                return q.Prepend(go_);
            }
        }
    }
}
