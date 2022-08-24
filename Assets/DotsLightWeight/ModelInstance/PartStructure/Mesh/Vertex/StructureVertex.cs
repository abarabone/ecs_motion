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
        public Color32 PartId;
        public Vector2 Uv;
    }

    public class StructureVertexBuilder : IVertexBuilder//, ISetBufferParams
    {
        //public TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //    where TVtx : struct, IVertexUnit
        //{
        //    var poss = srcmeshes.QueryConvertPositions(p).ToArray();
        //    var nms = srcmeshes.QueryConvertNormals(p).ToArray();
        //    var pids = srcmeshes.QueryConvertPartId(p).ToArray();
        //    var cids = srcmeshes.QueryColorPaletteSubIndex(p).ToArray();
        //    var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
        //    var qVtx =
        //        from x in (poss, nms, pids, cids, uvs).Zip()
        //        select new StructureVertex
        //        {
        //            Position = x.src0,
        //            Normal = x.src1,
        //            PardId = new Color32(x.src2.r, x.src2.g, x.src2.b, x.src3.a),
        //            Uv = x.src4,
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


            static StructureVertex[] buildVtxs_(
                IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
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
                        PartId = new Color32
                        {
                            r = (byte)(x.src2 & 0x1f),
                            g = (byte)(x.src2 >> 5 & 0xff),// 32bit ‚È‚Ì‚Å >> 5
                            a = (byte)x.src3,
                        },
                        Uv = x.src4,
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

            static void copyVtxs_(Mesh.MeshData meshdata, StructureVertex[] vtxs)
            {
                meshdata.GetVertexData<StructureVertex>().CopyFrom(vtxs);
            }
        }
    }

}
