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

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;


    public static class MeshCombineUtility
    {


        //public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>(
        //    this SrcMeshesModelCombinePack srcmeshpack,
        //    AdditionalParameters p)
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexBuilder, ISetBufferParams
        //{
        //    return new TVtx().BuildCombiner<TIdx>(srcmeshpack.AsEnumerable, p);
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        //public static Task<MeshElements<TIdx, TVtx>> ToTask<TIdx, TVtx>(
        //    this Func<MeshElements<TIdx, TVtx>> f)
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        //=>
        //    Task.Run(f);

        //public static Task<IMeshElements> ToTask(this Func<IMeshElements> f) =>
        //    Task.Run(f);

        //public static Task<(TIdx[], TVtx[])> ToTask<TIdx, TVtx>(this Func<(TIdx[], TVtx[])> f)
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        //=>
        //    Task.Run(f);



        /// <summary>
        /// 別の関数で
        /// 必要なパラメータだけ計算するようにしたほうがいいかも
        /// </summary>
        public static AdditionalParameters calculateParameters(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
            Transform tfBase, Transform[] tfBones,
            Func<int, Rect> texHashToUvRectFunc, Func<int, int> texHashToUvIndexFunc)
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
                    select mat?.mainTexture?.GetHashCode() ?? 0
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
                texHashToUvIndex = texHashToUvIndexFunc,// マテリアルのテクスチャから、uv index を取得するためのラムダ
            };

            // こんへんは別の関数にして、必要な場合だけ呼び出すようにしたい

            var qPartIdPerMesh =
                from mmt in mmts_
                    //.Do(x => Debug.Log($"part id is {x.tf.getInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.getInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                    //.Do(x => Debug.Log($"part id is {x.tf.findInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.findInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                select mmt.tf.getInParent<IStructurePart>()?.partId ?? -1
                //select mmt.tf.gameObject.GetComponentInParent<StructurePartAuthoring>()?.PartId ?? -1
                ;
            result.partIdPerMesh = qPartIdPerMesh.ToArray();

            if (tfBones == null) return result;
            if (!tfBones.Any()) return result;


            var qBoneWeights =
                from mesh in meshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in meshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts_
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.WrapEnumerable().ToArray());
            ;
            result.boneWeightsPerMesh = qBoneWeights.ToArray();
            result.mtInvsPerMesh = qMtInvs.ToArray();
            result.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
            return result;
        }


        //static T findInParent<T>(this GameObject obj)
        //    where T : MonoBehaviour
        //=> obj
        //    .AncestorsAndSelf()
        //    .Select(x => x.GetComponent<T>())
        //    .FirstOrDefault()
        //    ;

        //static T findInParent<T>(this Transform tf) where T : MonoBehaviour => tf.gameObject.findInParent<T>();

        static T getInParent<T>(this GameObject obj)
            where T : IStructurePart
        //=> obj.GetComponentsInParent<T>(true).FirstOrDefault();
        => obj.GetComponentInParent<T>(true);

        static T getInParent<T>(this Transform tf)
            where T : IStructurePart
        => tf.gameObject.getInParent<T>();
    }
}