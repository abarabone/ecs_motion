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
        public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
            (
                this IEnumerable<GameObject> gameObjects, Transform tfBase,
                Func<int, Rect> texHashToUvRectFunc = null,
                Transform[] tfBones = null
            )
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        =>
            gameObjects.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, texHashToUvRectFunc, tfBones);


        public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
            (
                this GameObject gameObjectTop, Transform tfBase,
                Func<int, Rect> texHashToUvRectFunc = null,
                Transform[] tfBones = null
            )
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        =>
            gameObjectTop.QueryMeshMatsTransform_IfHaving()
                .BuildCombiner<TIdx, TVtx>(tfBase, texHashToUvRectFunc, tfBones);


        public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
            (
                this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, Transform tfBase,
                Func<int, Rect> texHashToUvRectFunc = null,
                Transform[] tfBones = null
            )
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        {
            var (srcmeshes, p) = mmts.calculateParametors(tfBase, texHashToUvRectFunc, tfBones);

            return () => new TVtx().BuildCombiner<TIdx>(srcmeshes, p);
        }


        /// <summary>
        /// 
        /// </summary>
        public static Task<MeshElements<TIdx, TVtx>> ToTask<TIdx, TVtx>
            (this Func<MeshElements<TIdx, TVtx>> f)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        =>
            Task.Run(f);

        public static Task<IMeshElements> ToTask(this Func<IMeshElements> f)
        =>
            Task.Run(f);




        static IEnumerable<IEnumerable<MeshUnit>> queryMeshData
            (this IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
        {
            var meshes = mmtss.SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(meshes);

            var imesh = 0;

            return
                from mmts in mmtss
                let length = mmts.Count()
                select queryMesh_(imesh, length)
                ;

            IEnumerable<MeshUnit> queryMesh_(int first, int length)
            {
                var baseVertex = 0;
                for (var i = first; i < first + length; i++)
                {
                    yield return new MeshUnit(i, mesharr[i], baseVertex);

                    baseVertex += mesharr[i].vertexCount;
                }
            }
        }
        static IEnumerable<Mesh.MeshData> asEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            for (var i = 0; i < meshDataArray.Length; i++)
            {
                yield return meshDataArray[i];
            }
        }



        static AdditionalParameters calculateParametors
            (
                this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
                Transform tfBase, Func<int, Rect> texHashToUvRectFunc,
                Transform[] tfBones
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


            var result = new AdditionalParameters
            {
                mtBaseInv = mtBaseInv,
                mtPerMesh = qMtPerMesh.ToArray(),
                texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2(),
                //atlasHash = atlas?.GetHashCode() ?? 0,
                //texhashToUvRect = texHashToUvRect,
                texHashToUvRect = texHashToUvRectFunc,
            };
            if (tfBones == null) return result;


            var qBoneWeights =
                from mesh in meshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in meshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts_
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.AsEnumerable().ToArray());
            ;
            result.boneWeightsPerMesh = qBoneWeights.ToArray();
            result.mtInvsPerMesh = qMtInvs.ToArray();
            result.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
            return result;
        }


        static (Mesh.MeshDataArray, AdditionalParameters) calculateParametors
            (
                this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
                Transform tfBase, Func<int, Rect> texHashToUvRectFunc,
                Transform[] tfBones
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

            var result = (srcmeshes, parameters: new AdditionalParameters
            {
                mtBaseInv = mtBaseInv,
                mtPerMesh = qMtPerMesh.ToArray(),
                texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2(),
                //atlasHash = atlas?.GetHashCode() ?? 0,
                //texhashToUvRect = texHashToUvRect,
                texHashToUvRect = texHashToUvRectFunc,
            });
            if (tfBones == null) return result;


            var qBoneWeights =
                from mesh in meshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in meshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts_
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.AsEnumerable().ToArray());
                ;
            result.parameters.boneWeightsPerMesh = qBoneWeights.ToArray();
            result.parameters.mtInvsPerMesh = qMtInvs.ToArray();
            result.parameters.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
            return result;
        }

    }
}
