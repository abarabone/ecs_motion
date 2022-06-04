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

    static public partial class MeshCreatorUtility
    {

        //public static Mesh CreateMesh<TIdx, TVtx>(this MeshElements<TIdx, TVtx> meshElements,
        //    ISetBufferParams idxBuilder, ISetBufferParams vtxBuilder)
        //    where TIdx : struct, IIndexUnit<TIdx>
        //    where TVtx : struct, IVertexUnit
        //{
        //    var dstmeshes = Mesh.AllocateWritableMeshData(1);
        //    var dstmesh = new Mesh();

        //    var src = meshElements;
        //    var dst = dstmeshes[0];

        //    var idxs = src.idxs;
        //    idxBuilder.SetBufferParams(dst, idxs.Length);
        //    //dst.setBufferParams<TIdx>(idxs.Length);
        //    dst.GetIndexData<TIdx>().CopyFrom(idxs);

        //    var vtxs = src.vtxs;
        //    vtxBuilder.SetBufferParams(dst, vtxs.Length);
        //    //dst.setBufferParams<TVtx>(vtxs.Length);
        //    dst.GetVertexData<TVtx>().CopyFrom(vtxs);

        //    dst.subMeshCount = 1;
        //    dst.SetSubMesh(0, new SubMeshDescriptor(0, idxs.Length));

        //    Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);
        //    dstmesh.RecalculateBounds();

        //    return dstmesh;
        //}



        //static void setBufferParams<T>(this Mesh.MeshData meshdata, int elementLength)
        //    where T : struct, ISetBufferParams
        //=>
        //    new T().SetBufferParams(meshdata, elementLength);





        public static Mesh.MeshDataArray CreateMeshData<TIdx, TVtx>(
            this SrcMeshesModelCombinePack meshpack,
            IIndexBuilder idxBuilder, IVertexBuilder vtxBuilder,
            AdditionalParameters p)
            where TIdx : struct, IIndexUnit
            where TVtx : struct, IVertexUnit
        {
            var dstmeshes = Mesh.AllocateWritableMeshData(1);

            var src = meshpack.AsEnumerable;
            var dst = dstmeshes[0];

            var idxs = idxBuilder.Build<TIdx>(src, p);
            idxBuilder.SetBufferParams(dst, idxs.Length);
            //dst.setBufferParams<TIdx>(idxs.Length);
            dst.GetIndexData<TIdx>().CopyFrom(idxs);

            var vtxs = vtxBuilder.Build<TVtx>(src, p);
            vtxBuilder.SetBufferParams(dst, vtxs.Length);
            //dst.setBufferParams<TVtx>(vtxs.Length);
            dst.GetVertexData<TVtx>().CopyFrom(vtxs);

            dst.subMeshCount = 1;
            dst.SetSubMesh(0, new SubMeshDescriptor(0, idxs.Length));

            return dstmeshes;
        }

        public static Mesh CreateMesh(this Mesh.MeshDataArray meshdata)
        {
            var dstmesh = new Mesh();

            Mesh.ApplyAndDisposeWritableMeshData(meshdata, dstmesh);
            dstmesh.RecalculateBounds();

            return dstmesh;
        }

    }
}
