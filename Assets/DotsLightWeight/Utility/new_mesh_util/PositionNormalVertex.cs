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
    public struct PositionNormalVertex : IVertexUnit<PositionNormalVertex>, ISetBufferParams
    {

        public Vector3 Position;
        public Vector3 Normal;


        public MeshElements<TIdx, PositionNormalVertex> BuildCombiner<TIdx>
            (IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            new MeshElements<TIdx, PositionNormalVertex>
            {
                idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                nms = srcmeshes.QueryConvertNormals(p).ToArray()
            };


        public Func<(TIdx[], PositionNormalVertex[])> BuildCombiner2<TIdx>
            (IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams => default;

        public IEnumerable<PositionNormalVertex> Packing<TIdx>(MeshElements<TIdx, PositionNormalVertex> src)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        =>
            from x in (src.poss, src.nms).Zip()
            select new PositionNormalVertex
            {
                Position = x.src0,
                Normal = x.src1,
            };


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
