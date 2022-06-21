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



    public interface IVertexUnit
    { }

    public interface IVertexBuilder// : ISetBufferParams
    {
        //Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;

        //TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //    where TVtx : struct, IVertexUnit;

        void BuildMeshData(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p, Mesh.MeshData dstmesh);
    }

    //public interface IVertexUnitBoned<TVtx> : IVertexUnit<TVtx>
    //    where TVtx : struct, IVertexUnitBoned<TVtx>, ISetBufferParams
    //{
    //    Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
    //        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;
    //}

    //public interface IVertexUnitWithPalette<TVtx> : IVertexUnit<TVtx>
    //    where TVtx : struct, IVertexUnitWithPalette<TVtx>, ISetBufferParams
    //{
    //    Func<MeshElements<TIdx, TVtx>> BuildCombiner<TIdx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
    //        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;
    //}

}
