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



    public struct MeshElements<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>
        where TVtx : struct, IVertexUnit<TVtx>
    {
        public TIdx[] idxs;
        public Vector3[] poss;
        public Vector2[] uvs;
        public Vector3[] nms;
        public uint4[] bis;
        public uint4[] bws;
    }


    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }


}
