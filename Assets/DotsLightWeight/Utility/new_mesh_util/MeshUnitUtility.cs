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


    public interface IMeshElements
    {
        Mesh CreateMesh();
    }

    public class MeshElements<TIdx, TVtx> : IMeshElements
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public TIdx[] idxs;
        public Vector3[] poss;
        public Vector2[] uvs;
        public Vector3[] nms;
        public uint[] bids;
        public Vector4[] bws;
        public Color32[] pids;

        public Mesh CreateMesh() => MeshCreatorUtility.CreateMesh(this);
    }

    public static class _
    {
        public static Mesh CreateMesh2<TIdx, TVtx>(this (TIdx[], TVtx[]) src)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        {
            return MeshCreatorUtility.CreateMesh(src);
        }
    }


    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }


}
