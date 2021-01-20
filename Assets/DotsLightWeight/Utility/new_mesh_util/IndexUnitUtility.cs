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

    public interface IIndexUnit<T>
    {
        //public int Get();
        //public T Set(int newValue);
        T Add(int otherValue);
    }

    public struct UI16 : IIndexUnit<UI16>, ISetBufferParams
    {
        public ushort value;
        //public int Get() => this.value; 
        //public UI16 Set(int newValue) { this.value = (ushort)newValue; return this; }
        public UI16 Add(int otherValue) => new UI16 { value = (ushort)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }
    }

    public struct UI32 : IIndexUnit<UI32>, ISetBufferParams
    {
        public uint value;
        //public int Get() => (int)this.value;
        //public UI32 Set(int newValue) { this.value = (uint)newValue; return this; }
        public UI32 Add(int otherValue) => new UI32 { value = (uint)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
        }
    }
}
