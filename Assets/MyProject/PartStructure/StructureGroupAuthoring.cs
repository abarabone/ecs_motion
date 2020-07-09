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

namespace Abarabone.Structure.Aurthoring
{

    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;




    public class StructureGroupAuthoring : MonoBehaviour
    {

        public StructureModelAuthoring[] StructureModelPrefabs;


        private void Awake()
        {

            var q = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                .Select(pt => pt.CombinePartMeshesAsync());

            Task.WhenAll(q);

            Debug.Log("e");

        }


    }


    static class StructureConversionExtension
    {

        /// <summary>
        /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        /// </summary>
        static public async Task<MeshElements> CombinePartMeshesAsync( this StructurePartAuthoring part )
        //static public MeshElements CombinePartMeshes(StructurePartAuthoring part)
        {

            // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
            // （ただしパーツだった場合は、結合対象から除外する）
            var buildTargets = queryTargets_Recursive_(part.gameObject).ToArray();
            if (buildTargets.Length == 1) return new MeshElements { };

            var meshElements_ = await combineChildMeshesAsync_(buildTargets, part.transform);
            //var meshElements_ = combineChildMeshes_(buildTargets, part.transform);

            return meshElements_;


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
            //MeshElements combineChildMeshes_(IEnumerable<GameObject> targets_, Transform tf_)
            //{
            //    var combineElementFunc =
            //        MeshCombiner.BuildNormalMeshElements(targets_, tf_, isCombineSubMeshes: true);

            //    return combineElementFunc();
            //}
        }
    }
}