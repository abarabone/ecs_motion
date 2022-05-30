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

    public interface IIndexUnit<TIdx>
    {
        TIdx Add(uint otherValue);
    }

    public struct UI16 : IIndexUnit<UI16>, ISetBufferParams
    {
        public ushort value;
        public UI16 Add(uint otherValue) => new UI16 { value = (ushort)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }

        public static implicit operator UI16 (ushort src) => new UI16 { value = src };
    }

    public struct UI32 : IIndexUnit<UI32>, ISetBufferParams
    {
        public uint value;
        public UI32 Add(uint otherValue) => new UI32 { value = (uint)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
        }

        public static implicit operator UI32(uint src) => new UI32 { value = src };
    }




    public interface IIndexBuilder<TIdx>
        where TIdx : IIndexUnit<TIdx>
    {
        TIdx[] Build(IEnumerable<SrcMeshUnit> srcmeshes);
    }

    public struct UI16_ : IIndexUnit<UI16_>
    {
        public ushort value;
        public UI16_ Add(uint otherValue) => new UI16_ { value = (ushort)(otherValue + this.value) };
    }
    public struct UI16Builder : IIndexBuilder<UI16_>, ISetBufferParams
    {
        public UI16_[] Build(IEnumerable<SrcMeshUnit> srcmeshes) =>
            srcmeshes.QueryConvertIndexData<UI16_>(p.mtPerMesh).ToArray();

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }

        public static implicit operator UI16_(ushort src) => new UI16_ { value = src };
    }

}
