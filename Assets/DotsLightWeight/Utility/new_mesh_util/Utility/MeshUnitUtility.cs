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
        where TIdx : struct, IIndexUnit<TIdx>
        where TVtx : struct, IVertexUnit
    {
        public TIdx[] idxs;
        public TVtx[] vtxs;

        public ISetBufferParams idxBuilder;
        public ISetBufferParams vtxBuilder;

        public Mesh CreateMesh() => this.CreateMesh(this.idxBuilder, this.vtxBuilder);

        ///// <summary>
        ///// ƒ^ƒvƒ‹‚©‚ç‚ÌˆÃ–Ù“I•ÏŠ·
        ///// </summary>
        //public static implicit operator MeshElements<TIdx, TVtx> ((TIdx[], TVtx[]) src) =>
        //    new MeshElements<TIdx, TVtx>
        //    {
        //        idxs = src.Item1,
        //        vtxs = src.Item2,
        //    };
    }

    //public static class _
    //{
    //    public static Mesh CreateMesh<TIdx, TVtx>(this (TIdx[], TVtx[]) src)
    //        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
    //        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    //    {
    //        return MeshCreatorUtility.CreateMesh(src);
    //    }
    //}


    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }


}
