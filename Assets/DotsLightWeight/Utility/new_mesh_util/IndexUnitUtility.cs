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

    public interface IIndexUnit<T>
    {
        T Add(int otherValue);
    }

    public struct UI16 : IIndexUnit<UI16>, ISetBufferParams
    {
        public ushort value;
        public UI16 Add(int otherValue) => new UI16 { value = (ushort)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }
    }

    public struct UI32 : IIndexUnit<UI32>, ISetBufferParams
    {
        public uint value;
        public UI32 Add(int otherValue) => new UI32 { value = (uint)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
        }
    }
}
