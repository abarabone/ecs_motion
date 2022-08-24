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

    public struct PositionNormalVertexBuilder : IVertexBuilder//, ISetBufferParams
    {

        //public TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //    where TVtx : struct, IVertexUnit
        //{
        //    var poss = srcmeshes.QueryConvertPositions(p).ToArray();
        //    var nms = srcmeshes.QueryConvertNormals(p).ToArray();
        //    var qVtx =
        //        from x in (poss, nms).Zip()
        //        select new PositionNormalVertex
        //        {
        //            Position = x.src0,
        //            Normal = x.src1,
        //        };
        //    return qVtx.Cast<TVtx>().ToArray();
        //}


        //public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        //{
        //    var layout = new[]
        //    {
        //        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        //        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
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


            static PositionNormalVertex[] buildVtxs_(
                IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
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
                return qVtx.ToArray();
            }

            static void setVtxBufferParams_(Mesh.MeshData meshdata, int vtxLength)
            {
                var layout = new[]
                {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                };
                meshdata.SetVertexBufferParams(vtxLength, layout);
            }

            static void copyVtxs_(Mesh.MeshData meshdata, PositionNormalVertex[] vtxs)
            {
                meshdata.GetVertexData<PositionNormalVertex>().CopyFrom(vtxs);
            }
        }
    }

}
