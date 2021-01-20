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

    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }

    public interface IVertexUnit<TVtx>
        where TVtx : struct
    {
        MeshElements<TIdx> BuildCombiner<TIdx>
            (IEnumerable<GameObject> gameObjects, Mesh.MeshData srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>;

        IEnumerable<TVtx> SelectAll<TIdx>(MeshElements<TIdx> src) where TIdx : struct;
    }
}
