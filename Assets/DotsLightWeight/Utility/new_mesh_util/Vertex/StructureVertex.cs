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
    public struct StructureVertex : IVertexUnit
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color32 PardId;
        public Vector2 Uv;
    }

    public struct StructureVertexBuilder : IVertexBuilder<StructureVertex>, ISetBufferParams
    {


        public StructureVertex[] Build(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        {
            var poss = srcmeshes.QueryConvertPositions(p).ToArray();
            var nms = srcmeshes.QueryConvertNormals(p).ToArray();
            var pids = srcmeshes.QueryConvertPartId(p).ToArray();
            var cids = srcmeshes.QueryColorPaletteSubIndex(p).ToArray();
            var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
            var qVtx =
                from x in (poss, nms, pids, cids, uvs).Zip()
                select new StructureVertex
                {
                    Position = x.src0,
                    Normal = x.src1,
                    PardId = new Color32(x.src2.r, x.src2.g, x.src2.b, x.src3.a),
                    Uv = x.src4,
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
