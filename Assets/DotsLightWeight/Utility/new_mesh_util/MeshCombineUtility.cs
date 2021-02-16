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


    public struct srcMeshFromAllModel : IDisposable
    {
        public srcMeshFromAllModel
            (Mesh.MeshDataArray marr, IEnumerable<IEnumerable<SrcMeshCombinePack>> e)
        {
            this.marr = marr;
            this.AsEnumerable = e;
        }
        Mesh.MeshDataArray marr;
        public IEnumerable<IEnumerable<SrcMeshCombinePack>> AsEnumerable { get; private set; }
        public void Dispose() => this.marr.Dispose();
    }

    public struct srcMeshFromSingleModel : IDisposable
    {
        public srcMeshFromSingleModel
            (Mesh.MeshDataArray marr, IEnumerable<SrcMeshCombinePack> e)
        {
            this.marr = marr;
            this.AsEnumerable = e;
        }
        Mesh.MeshDataArray marr;
        public IEnumerable<SrcMeshCombinePack> AsEnumerable { get; private set; }
        public void Dispose() => this.marr.Dispose();
    }


    public struct SrcMeshCombinePack
    {
        public SrcMeshCombinePack
            (IEnumerable<SrcMeshUnit> e, IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        {
            this.AsEnumerable = e;
            this.Mmts = mmts;
        }
        public IEnumerable<SrcMeshUnit> AsEnumerable { get; private set; }
        public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> Mmts { get; private set; }
    }


    public static class MeshCombineUtility
    {


        public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
            (
                this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts, Transform tfBase,
                IEnumerable<SrcMeshUnit> srcmeshes,
                Func<int, Rect> texHashToUvRectFunc = null,
                Transform[] tfBones = null
            )
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        {
            var p = mmts.calculateParameters(tfBase, texHashToUvRectFunc, tfBones);

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


        public static srcMeshFromAllModel QueryMeshDataWithDisposingLast
            (this IEnumerable<IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>>> mmtsss)
        {
            var srcmeshes = mmtsss.SelectMany().SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(srcmeshes);

            var imesh = 0;
            var q =
                from mmtss in mmtsss
                select
                    from mmts in mmtss
                    let len = mmts.Count()
                    let meshes = queryMesh_(imesh, len)
                    select new SrcMeshCombinePack(meshes, mmts)
                ;
            return new srcMeshFromAllModel(mesharr, q);

            IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
            {
                var baseVertex = 0;

                for (var i = 0; i < length; i++)
                {
                    yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

                    baseVertex += mesharr[i].vertexCount;
                }

                imesh += length;
            }
        }

        public static srcMeshFromSingleModel QueryMeshDataWithDisposingLast
            (this IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
        {
            var srcmeshes = mmtss.SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(srcmeshes);

            var imesh = 0;
            var q =
                from mmts in mmtss
                let len = mmts.Count()
                let meshes = queryMesh_(imesh, len)
                select new SrcMeshCombinePack(meshes, mmts)
                ;
            return new srcMeshFromSingleModel(mesharr, q);

            IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
            {
                var baseVertex = 0;

                for (var i = 0; i < length; i++)
                {
                    yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

                    baseVertex += mesharr[i].vertexCount;
                }

                imesh += length;
            }
        }




        static AdditionalParameters calculateParameters
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
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.WrapEnumerable().ToArray());
            ;
            result.boneWeightsPerMesh = qBoneWeights.ToArray();
            result.mtInvsPerMesh = qMtInvs.ToArray();
            result.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
            return result;
        }

    }
}
