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
    public struct PositionNormalUvBonedVertex : IVertexUnit
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;
        public Vector4 BoneWeight4;
        public uint BoneIndex4;
    }


    public struct PositionNormalUvBonedVertexBuilder :
        IVertexBuilder<PositionNormalUvBonedVertex>, ISetBufferParams
    {


        public PositionNormalUvBonedVertex[] Build(
            IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        {
            var poss = srcmeshes.QueryConvertPositionsWithBone(p).ToArray();
            var nms = srcmeshes.QueryConvertNormals(p).ToArray();
            var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
            var bws = srcmeshes.QueryConvertBoneWeights(p).ToArray();
            var bids = srcmeshes.QueryConvertBoneIndices(p).ToArray();
            var qVtx =
                from x in (poss, nms, uvs, bws, bids).Zip()
                select new PositionNormalUvBonedVertex
                {
                    Position = x.src0,
                    Normal = x.src1,
                    Uv = x.src2,
                    BoneWeight4 = x.src3,
                    BoneIndex4 = x.src4,
                };
            return qVtx.ToArray();
        }


        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt8, 4),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }



    static public class BoneConversionUtility
    {

        public struct BoneConversionDictionary
        {
            Dictionary<(int mesh, int bone), int> dict;

            public BoneConversionDictionary(Dictionary<(int mesh, int bone), int> dict) => this.dict = dict;
            public int this[int meshIndex, int boneIndex]
            {
                get => this.dict.GetOrDefault((meshIndex, boneIndex), 0);
            }
        }

        public static BoneConversionDictionary ToBoneIndexConversionDictionary(
            this (IEnumerable<Transform[]> tfSrcBonesPerMesh, Transform[] tfDstBones) src)
        {
            var qTfToMeshAndBoneIndex =
                from m in src.tfSrcBonesPerMesh.WithIndex()
                from b in m.src.WithIndex()
                select (tf: b.src, index: (mesh: m.i, bone: b.i))
                ;
            var q =
                from srcbone in qTfToMeshAndBoneIndex
                join dstbone in src.tfDstBones.WithIndex()
                on srcbone.tf equals dstbone.src
                select (srcbone.index, dstbone.i)
                ;
            return new BoneConversionDictionary(q.ToDictionary());
        }

    }

}
