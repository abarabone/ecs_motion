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
    public struct PositionNormalVertex : IVertexUnit
    {

        public Vector3 Position;
        public Vector3 Normal;

    }

    public struct PositionNormalVertexBuilder : IVertexBuilder, ISetBufferParams
    {

        public TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TVtx : struct, IVertexUnit
        {
            var poss = srcmeshes.QueryConvertPositions(p).ToArray();
            var nms = srcmeshes.QueryConvertNormals(p).ToArray();
            var qVtx =
                from x in (poss, nms).Zip()
                select new PositionNormalVertex
                {
                    Position = x.src0,
                    Normal = x.src1,
                };
            return qVtx.Cast<TVtx>().ToArray();
        }


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3)
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
