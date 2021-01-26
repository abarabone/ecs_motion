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

    public interface IVertexUnit<TVtx>
        where TVtx : struct, IVertexUnit<TVtx>
    {
        MeshElements<TIdx, TVtx> BuildCombiner<TIdx>(Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>;

        IEnumerable<TVtx> Packing<TIdx>(MeshElements<TIdx, TVtx> src)
            where TIdx : struct, IIndexUnit<TIdx>;
    }
}
