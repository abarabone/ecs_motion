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

    public class StructureGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        public StructureModelAuthoring[] StructureModelPrefabs;

        public GameObject[] partMasterPrefabs;
        public Mesh[] CombinedPartMeshes;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(this.StructureModelPrefabs.Select(x=>x.gameObject));
        }

        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
        }
        
        private async void Awake()
        {

            this.partMasterPrefabs = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                .Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Distinct()
                .Do(x => Debug.Log(x.name))
                .ToArray();

            var q = this.partMasterPrefabs
                .Select( ptgo => ptgo.GetComponent<StructurePartAuthoring>() )
                .Select(pt => pt.CombinePartMeshesAsync());

            var meshElementsList = await Task.WhenAll(q);

            this.CombinedPartMeshes = meshElementsList.ToArray();


            this.partMasterPrefabs
                .Select( x => Instantiate(x) )
                .Do(x => x.AddComponent<PhysicsBodyAuthoring>())
                .ForEach( x => x.AddComponent<ConvertToEntity>() );


        }


    }


    static class StructureConversionExtension
    {

        /// <summary>
        /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        /// </summary>
        static public async Task<Mesh> CombinePartMeshesAsync( this StructurePartAuthoring part )
        {

            // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
            // （ただしパーツだった場合は、結合対象から除外する）
            var buildTargets = queryTargets_Recursive_(part.gameObject).ToArray();
            if (buildTargets.Length == 1) return buildTargets.First().GetComponent<MeshFilter>().sharedMesh;

            var meshElements_ = await combineChildMeshesAsync_(buildTargets, part.transform);

            return meshElements_.CreateMesh();


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

            async Task<MeshElements> combineChildMeshesAsync_(IEnumerable<GameObject> targets_, Transform tf_)
            {
                var combineElementFunc =
                    MeshCombiner.BuildNormalMeshElements(targets_, tf_, isCombineSubMeshes: true);

                return await Task.Run(combineElementFunc).ConfigureAwait(false);
            }
        }
    }
}