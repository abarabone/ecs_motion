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


    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalUvVertex : IVertexUnit<PositionNormalUvVertex>, ISetBufferParams
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;


        public MeshElements<TIdx, PositionNormalUvVertex> BuildCombiner<TIdx>
            (IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            new MeshElements<TIdx, PositionNormalUvVertex>
            {
                idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                nms = srcmeshes.QueryConvertNormals(p).ToArray(),
                uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray(),
            };


        public IEnumerable<PositionNormalUvVertex> Packing<TIdx>(MeshElements<TIdx, PositionNormalUvVertex> src)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            from x in (src.poss, src.nms, src.uvs).Zip()
            select new PositionNormalUvVertex
            {
                Position = x.src0,
                Normal = x.src1,
                Uv = x.src2,
            };


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
