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
        (GameObject go, Mesh mesh)[] objectAndMeshList;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var structurePrefabs = from x in this.StructureModelPrefabs select x.gameObject;
            referencedPrefabs.AddRange(structurePrefabs);
        }

        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //dstManager.DestroyEntity(entity);
            Debug.Log("aaa");
            create(conversionSystem);
        }


        private void create(GameObjectConversionSystem gcs)
        {

            var partMasterPrefabs = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                .Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Distinct()
                .Do(x => Debug.Log(x.name))
                .ToArray();

            var structurePrefabs = this.StructureModelPrefabs
                .Select(st => st.gameObject)
                .ToArray();


            combineMeshes_(gcs, partMasterPrefabs, structurePrefabs);

            createMasterPrefabObjectForConversion_(partMasterPrefabs);

            return;


            async void combineMeshes_(GameObjectConversionSystem gcs_, GameObject[] partMasterPrefabs_, GameObject[] structurePrefabs_ )
            {
                var qMasterPartPrefabMesh = partMasterPrefabs_
                    .Select(pt => pt.CombinePartMeshesAsync())
                    ;

                var qStructureMesh = structurePrefabs_
                    .Select(st => st.CombineStructureMeshesAsync())
                    ;

                var meshes = await Enumerable.Concat(qMasterPartPrefabMesh, qStructureMesh).WhenAll();
                var qObject = Enumerable.Concat(partMasterPrefabs, structurePrefabs);

                foreach( var (go, mesh) in (qObject, meshes).Zip() )
                {
                    gcs_.AddToStructureMeshDictionary(go, mesh);
                }
            }

            void createMasterPrefabObjectForConversion_( IEnumerable<GameObject> partMasterPrefabs_ )
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
        static public async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => await Task.WhenAll(tasks);//.ConfigureAwait(false);



        /// <summary>
        /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        /// </summary>
        static public async Task<Mesh> CombineStructureMeshesAsync( this GameObject structure )
        {
            var element = await structure.GetComponentsInChildren<MeshFilter>()
                .Select(mf => mf.gameObject)
                .combineChildMeshesAsync(structure.transform.GetChild(0))
                ;
            return element.CreateMesh();
        }


        /// <summary>
        /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        /// </summary>
        static public async Task<Mesh> CombinePartMeshesAsync( this GameObject part )
        {

            // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
            // （ただしパーツだった場合は、結合対象から除外する）
            var buildTargets = queryTargets_Recursive_(part.gameObject).ToArray();
            if (buildTargets.Length == 1) return buildTargets.First().GetComponent<MeshFilter>().sharedMesh;

            var element = await buildTargets.combineChildMeshesAsync(part.transform);

            return element.CreateMesh();


            IEnumerable<GameObject> queryTargets_Recursive_(GameObject go_)
            {
                var q =
                    from child in go_.Children()
                    where child.GetComponent<StructurePartAuthoring>() == null
                    from x in queryTargets_Recursive_(child)
                    select x
                    ;
                return q.Prepend(go_);
            }
        }


        static async Task<MeshElements> combineChildMeshesAsync(this IEnumerable<GameObject> targets, Transform tf)
        {
            var combineElementFunc =
                MeshCombiner.BuildNormalMeshElements(targets, tf, isCombineSubMeshes: true);

            return await Task.Run(combineElementFunc);//.ConfigureAwait(false);
        }

    }
}