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
        public IEnumerable<Matrix4x4> mtsPerMesh;
        
        //public int atlasHash;
        //public IEnumerable<IEnumerable<int>> texhashPerSubMesh;
        //public HashToRect texhashToUvRect;
        public Func<int, Rect> texHashToUvRect;
    }


    public struct MeshUnit
    {
        public MeshUnit(int i, Mesh.MeshData meshdata, int baseVertex)
        {
            this.MeshIndex = i;
            this.MeshData = meshdata;
            this.BaseVertex = baseVertex;
        }
        public readonly int MeshIndex;
        public readonly Mesh.MeshData MeshData;
        public readonly int BaseVertex;
    }


    public struct SubMeshUnit<T> where T : struct
    {
        public SubMeshUnit(int i, SubMeshDescriptor descriptor, Func<IEnumerable<T>> elements)
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

