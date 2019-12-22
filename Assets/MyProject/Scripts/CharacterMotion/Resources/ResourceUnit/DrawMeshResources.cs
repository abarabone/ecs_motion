using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Arthuring;
using Abss.Motion;

namespace Abss.Draw
{


    public class DrawMeshCsResourceUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
        public int VectorLengthInBone;
        public int MaxInstance;
    }

    public enum BoneType
    {
        T       = 1,
        TR      = 2,
        TRS     = 3,
        Matrix  = 4,
    }
    public struct TRBoneUnit
    {
        public float4 pos;
        public quaternion rot;
    }


    public class DrawMeshResourceHolder : IDisposable
    {

        public List<DrawMeshCsResourceUnit> Units { get; private set; } = new List<DrawMeshCsResourceUnit>();


        public int AddDrawMeshResource( Mesh mesh, Material mat, BoneType boneType, int maxInstance )
        {
            var meshId = this.Units.Count;

            var resource = new DrawMeshCsResourceUnit
            {
                MeshId = meshId,
                Mesh = mesh,
                Material = mat,
                VectorLengthInBone = (int)boneType,
                MaxInstance = maxInstance,
            };
            this.Units.Add( resource );

            return meshId;
        }

        public void Dispose()
        {
            foreach( var x in this.Units )
            {

            }
        }
    }
        

}
