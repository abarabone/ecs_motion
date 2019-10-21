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
using Abss.Cs;

namespace Abss.Draw
{


    public class DrawMeshCsResourceUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
    }

    public struct bone_unit
    {
        public float4 pos;
        public quaternion rot;
    }


    public class DrawMeshResourceHolder : IDisposable
    {


        public List<DrawMeshCsResourceUnit> Units { get; private set; } = new List<DrawMeshCsResourceUnit>();

        //public int MaxInstance;

        

        public int AddDrawMeshResource( Mesh mesh, Material mat )
        {
            var meshId = this.Units.Count;

            var resource = new DrawMeshCsResourceUnit
            {
                MeshId = meshId,
                Mesh = mesh,
                Material = mat,
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
