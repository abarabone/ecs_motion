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


    public struct DrawMeshCsResourceUnit : IDisposable
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
        public SimpleComputeBuffer<float4> TransformBuffer;// 使いまわしできれば、個別には不要
        public InstancingIndirectArguments Arguments;

        public void Dispose()
        {
        }
    }



    public class DrawMeshResourceHolder : IDisposable
    {


        public List<DrawMeshCsResourceUnit> Units { get; private set; } = new List<DrawMeshCsResourceUnit>();



        public int AddDrawMeshResource( Mesh mesh, Material mat )
        {
            var meshId = this.Units.Count;

            var resource = new DrawMeshCsResourceUnit
            {
                MeshId = meshId,
                Mesh = mesh,
                Material = mat,
                //TransformBuffer = new SimpleComputeBuffer<float4>( "bones", 1024 * 4 ),
                //Arguments = new InstancingIndirectArguments( mesh, allocator:Allocator.Persistent ),
            };
            this.Units.Add( resource );

            return meshId;
        }

        public void Dispose()
        {
        }
    }
        

}
