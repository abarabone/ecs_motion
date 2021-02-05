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
    public struct PositionNormalUvBonedVertex : IVertexUnit<PositionNormalUvBonedVertex>, ISetBufferParams
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;
        public uint BoneIndex4;
        public uint BoneWeight4;


        public MeshElements<TIdx, PositionNormalUvBonedVertex> BuildCombiner<TIdx>(Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            new MeshElements<TIdx, PositionNormalUvBonedVertex>
            {
                idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                nms = srcmeshes.QueryConvertNormals(p).ToArray(),
                uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray(),
                
            };


        public IEnumerable<PositionNormalUvBonedVertex> Packing<TIdx>(MeshElements<TIdx, PositionNormalUvBonedVertex> src)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            from x in (src.poss, src.nms, src.uvs, src.bis, src.bws).Zip()
            select new PositionNormalUvBonedVertex
            {
                Position = x.src0,
                Normal = x.src1,
                Uv = x.src2,
                BoneIndex4 = x.src3,
                BoneWeight4 = x.src4,
            };


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt8, 4),
                new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.SNorm8, 4),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
