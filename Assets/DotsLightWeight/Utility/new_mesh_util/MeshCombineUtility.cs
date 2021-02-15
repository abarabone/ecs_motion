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
        //public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
        //    (
        //        this IEnumerable<GameObject> gameObjects, Transform tfBase,
        //        IEnumerable<MeshUnit> srcmeshes,
        //        Func<int, Rect> texHashToUvRectFunc = null,
        //        Transform[] tfBones = null
        //    )
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        //=>
        //    gameObjects.QueryMeshMatsTransform_IfHaving()
        //        .BuildCombiner<TIdx, TVtx>(tfBase, srcmeshes, texHashToUvRectFunc, tfBones);


        //public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>
        //    (
        //        this GameObject gameObjectTop, Transform tfBase,
        //        IEnumerable<MeshUnit> srcmeshes,
        //        Func<int, Rect> texHashToUvRectFunc = null,
        //        Transform[] tfBones = null
        //    )
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        //=>
        //    gameObjectTop.QueryMeshMatsTransform_IfHaving()
        //        .BuildCombiner<TIdx, TVtx>(tfBase, srcmeshes, texHashToUvRectFunc, tfBones);


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




        //public static IEnumerable<SrcMeshUnit> QueryMeshDataWithDisposingLast
        //    (this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        //{
        //    var meshes = mmts.Select(x => x.mesh).ToArray();
        //    var mesharr = Mesh.AcquireReadOnlyMeshData(meshes);

        //    return query_();

        //    IEnumerable<SrcMeshUnit> query_()
        //    {
        //        using (new DevUtil.dispona(mesharr))// mesharr)
        //        {
        //            var baseVertex = 0;

        //            for (var i = 0; i < mesharr.Length; i++)
        //            {
        //                yield return new SrcMeshUnit(i, mesharr[i], baseVertex);

        //                baseVertex += mesharr[i].vertexCount;
        //            }
        //        }
        //    }
        //}
        //public struct MeshUnitsWithDisposing : IDisposable
        //{
        //    Mesh.MeshDataArray arr;
        //    public IEnumerable<MeshUnit> e { get; private set; }
        //    public MeshUnitsWithDisposing(IEnumerable<MeshUnit> e, Mesh.MeshDataArray arr)
        //    {
        //        this.e = e;
        //        this.arr = arr;
        //    }
        //    public void Dispose() => this.arr.Dispose();
        //}
        //public static IEnumerable<IEnumerable<SrcMeshUnit>> QueryMeshDataWithDisposingLastIn
        //    (this IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
        //{
        //    var meshes = mmtss.SelectMany().Select(x => x.mesh).ToArray();
        //    var mesharr = Mesh.AcquireReadOnlyMeshData(meshes);

        //    return query_();

        //    IEnumerable<IEnumerable<SrcMeshUnit>> query_()
        //    {
        //        using (new DevUtil.dispona(mesharr))// mesharr)
        //        {
        //            var imesh = 0;

        //            foreach (var mmts in mmtss)
        //            {
        //                var len = mmts.Count();

        //                yield return queryMesh_(imesh, len);

        //                imesh += len;
        //            }
        //        }
        //    }
        //    IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
        //    {
        //        var baseVertex = 0;

        //        for (var i = 0; i < length; i++)
        //        {
        //            yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

        //            baseVertex += mesharr[i].vertexCount;
        //        }
        //    }
        //}

        //public static IEnumerable<IEnumerable<SrcMeshUnit>> QueryMeshDataWithDisposingLast
        //    (this IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
        //=>
        //    mmtss.WrapEnumerable().QueryMeshDataWithDisposingLast().First();


        public struct aaa : IDisposable
        {
            public aaa(Mesh.MeshDataArray marr, IEnumerable<IEnumerable<IEnumerable<SrcMeshUnit>>> e)
            {
                this.marr = marr;
                this.AsEnumerable = e;
            }
            Mesh.MeshDataArray marr;
            public IEnumerable<IEnumerable<IEnumerable<SrcMeshUnit>>> AsEnumerable { get; private set; }
            public void Dispose() => this.marr.Dispose();
        }

        public static aaa QueryMeshDataWithDisposingLast
            (this IEnumerable<IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>>> mmtsss)
        {
            var meshes = mmtsss.SelectMany().SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(meshes);

            var imesh = 0;
            var q =
                from mmtss in mmtsss
                select //queryInModel_(mmtss)
                    from mmts in mmtss
                    let len = mmts.Count()
                    select queryMesh_(imesh, len)
                ;

            return new aaa(mesharr, q);// query_());

            //IEnumerable<IEnumerable<SrcMeshUnit>> queryInModel_
            //    (IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
            //{
            //    foreach (var mmts in mmtss)
            //    {
            //        var len = mmts.Count();

            //        yield return queryMesh_(imesh, len);

            //        imesh += len;
            //    }
            //}

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
            //IEnumerable<IEnumerable<IEnumerable<SrcMeshUnit>>> query_()
            //{
            //    var imesh = 0;

            //    foreach (var mmtss in mmtsss)
            //    {
            //        yield return queryInModel_(mmtss);
            //    }

            //    IEnumerable<IEnumerable<SrcMeshUnit>> queryInModel_
            //        (IEnumerable<IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>> mmtss)
            //    {
            //        foreach (var mmts in mmtss)
            //        {
            //            var len = mmts.Count();

            //            yield return queryMesh_(imesh, len);

            //            imesh += len;
            //        }
            //    }

            //    IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
            //    {
            //        var baseVertex = 0;

            //        for (var i = 0; i < length; i++)
            //        {
            //            yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

            //            baseVertex += mesharr[i].vertexCount;
            //        }
            //    }
            //}
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


        //static (Mesh.MeshDataArray, AdditionalParameters) calculateParameters
        //    (
        //        this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
        //        Transform tfBase, Func<int, Rect> texHashToUvRectFunc,
        //        Transform[] tfBones
        //    )
        //{
        //    var mmts_ = mmts.ToArray();
        //    var meshes = mmts_
        //        .Select(x => x.mesh)
        //        .ToArray();

        //    var qMtPerMesh = mmts_
        //        .Select(x => x.tf.localToWorldMatrix);
        //    var qTexhashPerSubMesh =
        //        from mmt in mmts_
        //        select
        //            from mat in mmt.mats
        //            select mat.mainTexture?.GetHashCode() ?? default
        //        ;

        //    var mtBaseInv = tfBase.worldToLocalMatrix;

        //    var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshes);

        //    var result = (srcmeshes, parameters: new AdditionalParameters
        //    {
        //        mtBaseInv = mtBaseInv,
        //        mtPerMesh = qMtPerMesh.ToArray(),
        //        texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2(),
        //        //atlasHash = atlas?.GetHashCode() ?? 0,
        //        //texhashToUvRect = texHashToUvRect,
        //        texHashToUvRect = texHashToUvRectFunc,
        //    });
        //    if (tfBones == null) return result;


        //    var qBoneWeights =
        //        from mesh in meshes
        //        select mesh.boneWeights
        //        ;
        //    var qMtInvs =
        //        from mesh in meshes
        //        select mesh.bindposes
        //        ;
        //    var qSrcBones = mmts_
        //        .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.AsEnumerable().ToArray());
        //        ;
        //    result.parameters.boneWeightsPerMesh = qBoneWeights.ToArray();
        //    result.parameters.mtInvsPerMesh = qMtInvs.ToArray();
        //    result.parameters.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
        //    return result;
        //}

    }
}
