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

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;

    public interface IIndexSelector : IIndexBuilder { }

    [Serializable]
    public class I16 : UI16Builder, IIndexSelector
    { }

    //[Serializable]
    //public class I32 : IIndexSelector
    //{ }
}

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;

    public interface IIndexUnit
    {
        uint Value { get; set; }
    }


    public interface IIndexBuilder// : ISetBufferParams
    {
        int BuildMeshData(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p, Mesh.MeshData dstmesh);
    }


    public struct UI16 : IIndexUnit
    {
        public ushort _value;
        public uint Value
        {
            get => this._value;
            set => this._value = (ushort)value;
        }

        public static implicit operator UI16(ushort src) => new UI16 { _value = src };
    }

    public class UI16Builder : IIndexBuilder
    {
        public int BuildMeshData(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p, Mesh.MeshData dstmesh)
        {

            var idxs = buildIdxs_(srcmeshes, p);
            setIdxBufferParams_(dstmesh, idxs.Length);
            copyIdxs_(dstmesh, idxs);
            return idxs.Length;


            static UI16[] buildIdxs_(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p) =>
                srcmeshes.QueryConvertIndexData<UI16>(p.mtPerMesh).ToArray();

            static void setIdxBufferParams_(Mesh.MeshData dstmesh, int idxLength)
            {
                dstmesh.SetIndexBufferParams(idxLength, IndexFormat.UInt16);
            }

            static void copyIdxs_(Mesh.MeshData dstmesh, UI16[] idxs)
            {
                dstmesh.GetIndexData<UI16>().CopyFrom(idxs);
            }
        }
    }


    //public struct UI32 : IIndexUnit<UI32>
    //{
    //    public uint value;
    //    public UI32 Add(uint otherValue) => new UI32 { value = (uint)(otherValue + this.value) };

    //    public static implicit operator UI32(uint src) => new UI32 { value = src };
    //}

    //public struct UI32Builder : IIndexBuilder<UI32>, ISetBufferParams
    //{
    //    public UI32[] Build(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p) =>
    //        srcmeshes.QueryConvertIndexData<UI32>(p.mtPerMesh).ToArray();

    //    public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
    //    {
    //        meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
    //    }
    //}

}
