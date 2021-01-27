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
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, TextureAtlasParameter tex = default)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            gameObjects.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, tex);


        public static Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx, TVtx>
            (this GameObject gameObjectTop, Transform tfBase, TextureAtlasParameter tex = default)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            gameObjectTop.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, tex);


        public static Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx, TVtx>
            (this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, Transform tfBase, TextureAtlasParameter tex = default)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        {
            var (srcmeshes, p) = mmts.calculateParametors
                (tfBase, tex.atlas, (tex.texhashes, tex.uvRects).ToDictionaryOrNull());

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
                Transform tfBase, Texture2D atlas, HashToRect texHashToUvRect
            )
        {
            var mmts_ = mmts.ToArray();

            var meshesPerMesh = mmts_.Select(x => x.mesh).ToArray();
            var mtsPerMesh = mmts_.Select(x => x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts_
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
                atlasHash = atlas?.GetHashCode() ?? 0,
                mtBaseInv = mtBaseInv,
                texhashToUvRect = texHashToUvRect,
            });
        }

    }
}
