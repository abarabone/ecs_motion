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
            create();
        }


        private void create()
        {

            var structurePrefabs = this.StructureModelPrefabs
                .Select(st => st.gameObject)
                .ToArray();

            var partMasterPrefabs = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                //.Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Select(pt => pt.MasterPrefab)
                .Distinct()
                .Do(x => Debug.Log(x.name))
                .ToArray();
            //this.partMasterPrefabs = partMasterPrefabs;


            combineMeshes_(structurePrefabs, partMasterPrefabs);

            return;


            void combineMeshes_( GameObject[] structurePrefabs_, GameObject[] partMasterPrefabs_ )
            {

                var qPartChildren = 
                    from pt in partMasterPrefabs_
                    select (pt, children: pt.QueryPartBodyObjects_Recursive_().ToArray())
                    ;
                var partChildrens = qPartChildren.ToArray();

                var qPart_multi =
                    from x in partChildrens
                    where x.children.Length > 1
                    select x
                    ;

                var qPart_single =
                    from x in partChildrens
                    where x.children.Length == 1
                    select x
                    ;


                var qStructureElement =
                    from st in structurePrefabs_
                    let objects = st.DescendantsAndSelf()
                    let f = objects.GetCombineStructureMeshesFunc(st.transform)
                    select Task.Run(f)
                    ;

                var qPartElement_multi =
                    from x in qPart_multi
                    let f = x.children.GetCombineChildMeshesFunc(x.pt.transform)
                    select Task.Run(f)
                    ;

                var qPartMesh_single =
                    from x in qPart_single
                    let mesh = x.children.First().GetComponent<MeshFilter>().sharedMesh
                    select mesh
                    ;


                var meshes = qStructureElement
                    .Concat(qPartElement_multi)
                    .WhenAll()
                    .Result
                    .Select(elm => elm.CreateMesh())
                    .Concat(qPartMesh_single);

                var qObject = structurePrefabs_
                    .Concat(qPart_multi.Select(x=>x.pt))
                    .Concat(qPart_single.Select(x=>x.pt));


                this.objectsAndMeshes = (qObject, meshes).Zip().ToArray();

            }

        }


    }

    static class StructureConversionExtension
    {

        /// <summary>
        /// 
        /// </summary>
        static public Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) =>
            Task.WhenAll(tasks);


        /// <summary>
        /// 
        /// </summary>
        static public Func<MeshElements> GetCombineChildMeshesFunc
            (this IEnumerable<GameObject> targets, Transform tf) =>
            MeshCombiner.BuildNormalMeshElements(targets, tf);

        static public Func<MeshElements> GetCombineStructureMeshesFunc
            (this IEnumerable<GameObject> targets, Transform tf) =>
            MeshCombiner.BuildStructureWithPalletMeshElements(targets, tf);



        /// <summary>
        /// 
        /// </summary>
        static public IEnumerable<GameObject> QueryPartBodyObjects_Recursive_(this GameObject go_)
        {
            var q =
                from child in go_.Children()
                where child.GetComponent<StructurePartAuthoring>() == null
                from x in QueryPartBodyObjects_Recursive_(child)
                select x
                ;
            return q.Prepend(go_);
        }

    }

}