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

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionUvVertex : IVertexUnit<PositionUvVertex>, ISetBufferParams
    {

        public Vector3 Position;
        public Vector2 Uv;


        public MeshElements<TIdx, PositionUvVertex> BuildCombiner<TIdx>
            (IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            new MeshElements<TIdx, PositionUvVertex>
            {
                idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray(),
            };


        public IEnumerable<PositionUvVertex> Packing<TIdx>(MeshElements<TIdx, PositionUvVertex> src)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            from x in (src.poss, src.uvs).Zip()
            select new PositionUvVertex
            {
                Position = x.src0,
                Uv = x.src1,
            };


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
