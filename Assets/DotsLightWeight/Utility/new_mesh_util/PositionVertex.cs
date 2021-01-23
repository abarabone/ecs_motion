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
    public struct PositionVertex : IVertexUnit<PositionVertex>, ISetBufferParams
    {

        public Vector3 Position;


        public MeshElements<TIdx, PositionVertex> BuildCombiner<TIdx>
            (IEnumerable<GameObject> gameObjects, Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            new MeshElements<TIdx, PositionVertex>
            {
                idxs = srcmeshes.QueryConvertIndexData<TIdx>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
            };


        public IEnumerable<PositionVertex> Packing<TIdx>(MeshElements<TIdx, PositionVertex> src)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            from x in src.poss
            select new PositionVertex
            {
                Position = x
            };


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

}
