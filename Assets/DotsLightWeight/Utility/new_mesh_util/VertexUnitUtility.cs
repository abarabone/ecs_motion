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

    public interface IVertexUnit<TVtx>
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        Func<(TIdx[], TVtx[])> BuildCombiner2<TIdx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;

        MeshElements<TIdx, TVtx> BuildCombiner<TIdx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;

        IEnumerable<TVtx> Packing<TIdx>(MeshElements<TIdx, TVtx> src)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams;
    }
}
