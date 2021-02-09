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


    public interface IMeshElements
    {
        Mesh CreateMesh();
    }

    public class MeshElements<TIdx, TVtx> : IMeshElements
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public Mesh.MeshDataArray src;

        public TIdx[] idxs;
        public Vector3[] poss;
        public Vector2[] uvs;
        public Vector3[] nms;
        public uint[] bis;
        public Vector4[] bws;

        public Mesh CreateMesh() => MeshCreatorUtility.CreateMesh(this);
    }


    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }


}
