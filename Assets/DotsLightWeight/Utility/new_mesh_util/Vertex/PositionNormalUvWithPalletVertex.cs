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
    public struct PositionNormalUvWithPaletteVertex : IVertexUnit
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Color32 PaletteId;
        public Vector2 Uv;

    }


    public struct PositionNormalUvWithPaletteVertexBuilder :
        IVertexBuilder<PositionNormalUvWithPaletteVertex>, ISetBufferParams
    {
        public PositionNormalUvWithPaletteVertex[] Build(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        {
            var poss = srcmeshes.QueryConvertPositions(p).ToArray();
            var nms = srcmeshes.QueryConvertNormals(p).ToArray();
            var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
            var cids = srcmeshes.QueryColorPaletteSubIndex(p).ToArray();
            var qVtx =
                from x in (poss, nms, uvs, cids).Zip()
                select new PositionNormalUvWithPaletteVertex
                {
                    Position = x.src0,
                    Normal = x.src1,
                    PaletteId = x.src3,
                    Uv = x.src2,
                };
            return qVtx.ToArray();
        }

        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
