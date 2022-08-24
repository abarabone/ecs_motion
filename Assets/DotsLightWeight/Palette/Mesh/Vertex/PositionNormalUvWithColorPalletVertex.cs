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
    public struct PositionUvNormalWithColorPaletteVertex : IVertexUnit
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Color32 PaletteId;
        public Vector2 Uv;

    }


    public class PositionUvNormalWithColorPaletteVertexBuilder : IVertexBuilder//, ISetBufferParams
    {
        //public TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //    where TVtx : struct, IVertexUnit
        //{
        //    var poss = srcmeshes.QueryConvertPositions(p).ToArray();
        //    var nms = srcmeshes.QueryConvertNormals(p).ToArray();
        //    var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
        //    var cids = srcmeshes.QueryColorPaletteSubIndex(p).ToArray();
        //    var qVtx =
        //        from x in (poss, nms, uvs, cids).Zip()
        //        select new PositionNormalUvWithPaletteVertex
        //        {
        //            Position = x.src0,
        //            Normal = x.src1,
        //            PaletteId = x.src3,
        //            Uv = x.src2,
        //        };
        //    return qVtx.Cast<TVtx>().ToArray();
        //}

        //public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        //{
        //    var layout = new[]
        //    {
        //        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        //        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        //        new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
        //        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        //    };
        //    meshdata.SetVertexBufferParams(vertexLength, layout);
        //}


        public void BuildMeshData(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p, Mesh.MeshData dstmesh)
        {

            var vtxs = buildVtxs_(srcmeshes, p);
            setVtxBufferParams_(dstmesh, vtxs.Length);
            copyVtxs_(dstmesh, vtxs);
            return;


            static PositionUvNormalWithColorPaletteVertex[] buildVtxs_(
                IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            {
                var poss = srcmeshes.QueryConvertPositions(p).ToArray();
                var nms = srcmeshes.QueryConvertNormals(p).ToArray();
                var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
                var cids = srcmeshes.QueryColorPaletteSubIndex(p).ToArray();
                var qVtx =
                    from x in (poss, nms, uvs, cids).Zip()
                    select new PositionUvNormalWithColorPaletteVertex
                    {
                        Position = x.src0,
                        Uv = x.src2,
                        PaletteId = new Color32(0, 0, 0, (byte)x.src3),
                        Normal = x.src1,
                    };
                return qVtx.ToArray();
            }

            static void setVtxBufferParams_(Mesh.MeshData meshdata, int vtxLength)
            {
                var layout = new[]
                {
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                };
                meshdata.SetVertexBufferParams(vtxLength, layout);
            }

            static void copyVtxs_(Mesh.MeshData meshdata, PositionUvNormalWithColorPaletteVertex[] vtxs)
            {
                meshdata.GetVertexData<PositionUvNormalWithColorPaletteVertex>().CopyFrom(vtxs);
            }
        }
    }

}
