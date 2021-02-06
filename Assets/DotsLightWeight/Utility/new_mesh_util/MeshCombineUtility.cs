using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.Geometry
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner;
    using Abarabone.Geometry.inner.unit;

    public static class MeshCombineUtility
    {

        /// <summary>
        /// 
        /// </summary>
        public static Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx, TVtx>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, Func<int, Rect> texHashToUvRectFunc = null)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            gameObjects.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, texHashToUvRectFunc);


        public static Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx, TVtx>
            (this GameObject gameObjectTop, Transform tfBase, Func<int, Rect> texHashToUvRectFunc = null)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            gameObjectTop.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, texHashToUvRectFunc);


        public static Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx, TVtx>
            (this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, Transform tfBase, Func<int, Rect> texHashToUvRectFunc = null)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        {
            var (srcmeshes, p) = mmts.calculateParametors(tfBase, texHashToUvRectFunc);

            return () => new TVtx().BuildCombiner<TIdx>(srcmeshes, p);
        }


        /// <summary>
        /// 
        /// </summary>
        public static Task<MeshElements<TIdx, TVtx>> ToTask<TIdx, TVtx>(this Func<MeshElements<TIdx, TVtx>> f)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            Task.Run(f);



        static (Mesh.MeshDataArray, AdditionalParameters) calculateParametors
            (
                this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
                Transform tfBase, Func<int, Rect> texHashToUvRectFunc
            )
        {
            var mmts_ = mmts.ToArray();
            var meshes = mmts_
                .Select(x => x.mesh)
                .ToArray();

            var qMtPerMesh = mmts_
                .Select(x => x.tf.localToWorldMatrix);
            var qTexhashPerSubMesh =
                from mmt in mmts_
                select
                    from mat in mmt.mats
                    select mat.mainTexture?.GetHashCode() ?? default
                ;

            var mtBaseInv = tfBase.worldToLocalMatrix;

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshes);

            var qBoneWeight =
                from mesh in meshes
                select
                    from w in mesh.boneWeights
                    select w
                ;
            var mtInvs = meshes.First().bindposes;

            return (srcmeshes, new AdditionalParameters
            {
                mtsPerMesh = qMtPerMesh.ToArray(),
                texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2(),
                //atlasHash = atlas?.GetHashCode() ?? 0,
                mtBaseInv = mtBaseInv,
                boneWeights = qBoneWeight.ToArray(),
                mtInvs = mtInvs,
                //texhashToUvRect = texHashToUvRect,
                texHashToUvRect = texHashToUvRectFunc,
            });
        }

    }
}
