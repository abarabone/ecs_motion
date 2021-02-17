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

namespace Abarabone.Geometry.inner.unit
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner;

    public struct AdditionalParameters
    {
        public Matrix4x4 mtBaseInv;
        public Matrix4x4[] mtPerMesh;

        public Matrix4x4[][] mtInvsPerMesh;//
        public BoneWeight[][] boneWeightsPerMesh;//
        public BoneConversionUtility.BoneConversionDictionary srcBoneIndexToDstBoneIndex;//

        //public int atlasHash;
        public IEnumerable<IEnumerable<int>> texhashPerSubMesh;
        //public HashToRect texhashToUvRect;
        public Func<int, Rect> texHashToUvRect;
    }

    //public struct MeshSourceUnit
    //{
    //    public MeshSourceUnit(IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> src)
    //    {
    //        this.e = src;
    //    }
    //    public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> e { get; private set; }
    //}
    //public static class m
    //{
    //    public static MeshSourceUnit ToSourceUnit(this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> src) =>
    //        new MeshSourceUnit(src);
    //}


    //public struct ModelUnit
    //{
    //    public ModelUnit(Abarabone.Model.Authoring.ModelGroupAuthoring.ModelAuthoringBase model)
    //    {

    //    }
    //    public readonly IEnumerable<MeshUnit> meshes;
    //}

    public struct SrcMeshUnit
    {
        public SrcMeshUnit(int indexInCombined, Mesh.MeshData meshdata, int baseVertex)
        {
            //this.MeshIndex = indexInCombined;
            this.MeshData = meshdata;
            this.BaseVertex = baseVertex;
        }
        //public readonly int MeshIndex;
        public readonly Mesh.MeshData MeshData;
        public readonly int BaseVertex;
    }


    public struct SrcSubMeshUnit<T> where T : struct
    {
        public SrcSubMeshUnit(int i, SubMeshDescriptor descriptor, Func<IEnumerable<T>> elements)
        {
            this.SubMeshIndex = i;
            this.Descriptor = descriptor;
            this.Elements = elements;
        }
        public readonly int SubMeshIndex;
        public readonly SubMeshDescriptor Descriptor;
        public readonly Func<IEnumerable<T>> Elements;
    }

}

