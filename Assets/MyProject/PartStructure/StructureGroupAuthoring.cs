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

namespace Abarabone.Structure.Aurthoring
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

        public StructureModelAuthoring[] StructureModelPrefabs;

        //public GameObject[] partMasterPrefabs;
        //public Mesh[] CombinedPartMeshes;
        //public Mesh[] CombinedStructureMeshes;
        //(GameObject go, Mesh mesh)[] objectAndMeshList;
        (GameObject, Mesh)[] objectsAndMeshes;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var structurePrefabs = from x in this.StructureModelPrefabs select x.gameObject;
            referencedPrefabs.AddRange(structurePrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.DestroyEntity(entity);
            //Debug.Log("aaa");
            //create(conversionSystem);

            foreach (var (go, mesh) in this.objectsAndMeshes)
            {
                conversionSystem.AddToStructureMeshDictionary(go, mesh);
            }
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
                .Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Distinct()
                .Do(x => Debug.Log(x.name))
                .ToArray();


            instantiateMasterPrefab_ForConversion_(partMasterPrefabs);

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
                    let objects = st.DescendantsAndSelf()//st.GetComponentsInChildren<MeshFilter>().Select(x=>x.gameObject)
                    let f = objects.GetCombineChildMeshesFunc(st.transform)
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

            void instantiateMasterPrefab_ForConversion_( IEnumerable<GameObject> partMasterPrefabs_ )
            {
                foreach (var pt in from prefab in partMasterPrefabs_ select Instantiate(prefab))
                {
                    pt.AddComponent<PhysicsBodyAuthoring>();
                    pt.AddComponent<ConvertToEntity>();
                }
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
            MeshCombiner.BuildNormalMeshElements(targets, tf, isCombineSubMeshes: true);


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




        ///// <summary>
        ///// 
        ///// </summary>
        //static public Func<MeshElements> GetCombineStructureMeshesFunc(this GameObject structure)
        //{
        //    return Enumerable.Repeat( structure, 1 )
        //        .GetCombineChildMeshesFunc(structure.transform.GetChild(0))
        //        ;
        //}


        ///// <summary>
        ///// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        ///// </summary>
        //static public Func<MeshElements> GetCombinePartMeshesFunc(this GameObject part)
        //{

        //    // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
        //    // （ただしパーツだった場合は、結合対象から除外する）
        //    var buildTargets = queryTargets_Recursive_(part.gameObject).ToArray();
        //    //if (buildTargets.Length == 1) return null;

        //    return buildTargets.GetCombineChildMeshesFunc(part.transform);


        //    IEnumerable<GameObject> queryTargets_Recursive_(GameObject go_)
        //    {
        //        var q =
        //            from child in go_.Children()
        //            where child.GetComponent<StructurePartAuthoring>() == null
        //            from x in queryTargets_Recursive_(child)
        //            select x
        //            ;
        //        return q.Prepend(go_);
        //    }
        //}


    }

    //static class StructureConversionExtension
    //{

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    static public async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) =>
    //        await Task.WhenAll(tasks);



    //    /// <summary>
    //    /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
    //    /// </summary>
    //    static public async Task<Mesh> CombineStructureMeshesAsync( this GameObject structure )
    //    {
    //        var element = await structure.GetComponentsInChildren<MeshFilter>()
    //            .Select(mf => mf.gameObject)
    //            .combineChildMeshesAsync(structure.transform.GetChild(0))
    //            ;
    //        return element.CreateMesh();

    //    }


    //    /// <summary>
    //    /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
    //    /// </summary>
    //    static public async Task<Mesh> CombinePartMeshesAsync( this GameObject part )
    //    {

    //        // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
    //        // （ただしパーツだった場合は、結合対象から除外する）
    //        var buildTargets = queryTargets_Recursive_(part.gameObject).ToArray();
    //        if (buildTargets.Length == 1) return buildTargets.First().GetComponent<MeshFilter>().sharedMesh;

    //        var element = await buildTargets.combineChildMeshesAsync(part.transform);

    //        return element.CreateMesh();


    //        IEnumerable<GameObject> queryTargets_Recursive_(GameObject go_)
    //        {
    //            var q =
    //                from child in go_.Children()
    //                where child.GetComponent<StructurePartAuthoring>() == null
    //                from x in queryTargets_Recursive_(child)
    //                select x
    //                ;
    //            return q.Prepend(go_);
    //        }
    //    }


    //    static async Task<MeshElements> combineChildMeshesAsync(this IEnumerable<GameObject> targets, Transform tf)
    //    {
    //        var combineElementFunc =
    //            MeshCombiner.BuildNormalMeshElements(targets, tf, isCombineSubMeshes: true);

    //        return await Task.Run(combineElementFunc);
    //    }

    //}
}