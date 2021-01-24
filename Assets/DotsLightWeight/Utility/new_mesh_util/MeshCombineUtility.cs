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
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, (Texture2D atlas, HashToRect texhashToUvRect) tex = default)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        {
            var (srcmeshes, p) = gameObjects.calculateParametors(tfBase, tex);

            return () => new TVtx().BuildCombiner<TIdx>(gameObjects, srcmeshes, p);
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
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, (Texture2D atlas, HashToRect texhashToUvRect) tex)
        {
            var mmts = gameObjects.QueryMeshMatsTransform_IfHaving().Do(x => Debug.Log(x.mesh.name)).ToArray();

            var meshesPerMesh = mmts.Select(x => x.mesh).ToArray();
            var mtsPerMesh = mmts.Select(x => x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select mat.mainTexture?.GetHashCode() ?? default
            ).ToArrayRecursive2();

            var mtBaseInv = tfBase.worldToLocalMatrix;

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshesPerMesh);

            return (srcmeshes, new AdditionalParameters
            {
                mtsPerMesh = mtsPerMesh,
                texhashPerSubMesh = texhashesPerSubMesh,
                atlasHash = tex.atlas.GetHashCode(),
                mtBaseInv = mtBaseInv,
                texhashToUvRect = tex.texhashToUvRect,
            });
        }

    }
}
