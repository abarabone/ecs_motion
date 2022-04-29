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
    public struct PositionNormalUvWithPalletVertex : IVertexUnit<PositionNormalUvWithPalletVertex>, ISetBufferParams
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;
        public Color32 PalletId;


        public Func<MeshElements<TIdx, PositionNormalUvWithPalletVertex>> BuildCombiner<TIdx>(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        {
            return () =>
            {
                var idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtPerMesh).ToArray();

                var poss = srcmeshes.QueryConvertPositions(p).ToArray();
                var nms = srcmeshes.QueryConvertNormals(p).ToArray();
                var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
                var cids = srcmeshes.QueryColorPalletSubIndex(p).ToArray();
                var qVtx =
                    from x in (poss, nms, uvs, cids).Zip()
                    select new PositionNormalUvWithPalletVertex
                    {
                        Position = x.src0,
                        Normal = x.src1,
                        Uv = x.src2,
                        PalletId = x.src3,
                    };
                var vtxs = qVtx.ToArray();

                return (idxs, vtxs);
            };
        }

        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
