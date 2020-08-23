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
using UnityEditor;

namespace Abarabone.Structure.Authoring
{

    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using Abarabone.Common.Extension;
    using Abarabone.Structure.Authoring;

    public class StructureGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        //public bool IsCombineMesh = true;
        //public bool IsPackTexture = true;


        public StructureModelAuthoring[] StructureModelPrefabs;

        (GameObject, Mesh)[] objectsAndMeshes;
        //GameObject[] partMasterPrefabs;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var structurePrefabs = this.StructureModelPrefabs.Select( x => x.gameObject );
            referencedPrefabs.AddRange(structurePrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.DestroyEntity(entity);

            addToMeshDictionary_(this.objectsAndMeshes);

            //stantiateMasterPrefab_ForConversion_(conversionSystem, this.partMasterPrefabs);

            return;


            void addToMeshDictionary_((GameObject, Mesh)[] objectsAndMeshes_)
            {
                foreach (var (go, mesh) in objectsAndMeshes_)
                {
                    Debug.Log($"to dict {go.name} - {mesh.name}");
                    conversionSystem.AddToMeshDictionary(go, mesh);
                }
            }

            //void stantiateMasterPrefab_ForConversion_
            //    (GameObjectConversionSystem gcs_, IEnumerable<GameObject> partMasterPrefabs_)
            //{
            //    foreach( var pt in partMasterPrefabs_.Select(prefab => Instantiate(prefab)) )
            //    {
            //        var em = gcs_.DstEntityManager;

            //        Debug.Log("pt "+pt.name);
            //        pt.AddComponent<PhysicsBodyAuthoring>();

            //        var flag = GameObjectConversionUtility.ConversionFlags.AssignName;
            //        var settings = new GameObjectConversionSettings(em.World, flag, gcs_.BlobAssetStore);
            //        var ent = GameObjectConversionUtility.ConvertGameObjectHierarchy(pt, settings);
            //        // このコンバートが走るたびに、専用の変換ワールドが作られて消えるみたい、よくないね…
            //        //GameObjectConversionUtility.ConvertGameObjectsToEntitiesField
            //        //    (gcs_, new GameObject[] { pt.gameObject}, out var ents);

            //        var addtype = new ComponentTypes
            //        (
            //            typeof(BinderTrimBlankLinkedEntityGroupTag),
            //            typeof(Prefab)
            //        );
            //        em.AddComponents(ent, addtype);
            //        //em.AddComponents(ents.First(), addtype);

            //        Destroy(pt);
            //    }
            //}
        }

        void Awake()
        {
            Debug.Log("aaa");
            create_();
            Debug.Log("bbb");
        }


        private void create()
        {

            var structureFarPrefabs = this.StructureModelPrefabs
                .Select(st => st.FarMeshObject.objectTop)
                .Do(x => Debug.Log($"st gr far {x.name}"))
                .ToArray();

            var structureNearPrefabs = this.StructureModelPrefabs
                .Select(st => st.NearMeshObject.objectTop)
                .Do(x => Debug.Log($"st gr near {x.name}"))
                .ToArray();

            var partMasterPrefabs = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                //.Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Select(pt => pt.MasterPrefab)
                .Distinct()
                .Do(x => Debug.Log($"st gr part {x.name}"))
                .ToArray();
            //this.partMasterPrefabs = partMasterPrefabs;


            combineMeshes_(structureFarPrefabs, structureNearPrefabs, partMasterPrefabs);

            return;


            void combineMeshes_(GameObject[] farPrefabs_, GameObject[] nearPrefabs_, GameObject[] partMasterPrefabs_)
            { }
        }
        void create_()
        {

            var qNear =
                from st in this.StructureModelPrefabs.Do(x => Debug.Log(x.NearMeshObject.objectTop.name))
                select st.GetNearMeshFunc()
                ;
            var qFar =
                from st in this.StructureModelPrefabs.Do(x => Debug.Log(x.FarMeshObject.objectTop.name))
                select st.GetFarMeshAndFunc()
                ;
            var qPartAll =
                from st in this.StructureModelPrefabs
                from pt in st.GetComponentsInChildren<StructurePartAuthoring>()
                select pt
                ;
            var qPartDistinct =
                    from pt in qPartAll.Distinct(pt => pt.MasterPrefab)
                    select pt.GetPartsMeshesAndFuncs()
                    ;

            var xs = qNear.Concat(qFar).Concat(qPartDistinct).ToArray();

            var qGoTask = xs
                .Where(x => x.f != null)
                .Select(x => (x.go, t: Task.Run(x.f)));

            var qGoMesh = xs
                .Where(x => x.mesh != null)
                .Select(x => (x.go, x.mesh));

            var qGoMeshFromTask = qGoTask
                .Select(x => x.t)
                .WhenAll()
                .Result
                .Select(x => x.CreateMesh())
                .Zip(qGoTask, (x, y) => (y.go, mesh: x));


            this.objectsAndMeshes = qGoMesh.Concat(qGoMeshFromTask)
                .ToArray();
        }




    }

    static class StructureConversionExtension
    {

        /// <summary>
        /// 
        /// </summary>
        static public Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) =>
            Task.WhenAll(tasks);

    }

}