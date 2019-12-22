using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;
using Abss.Common.Extension;
using Abss.Utilities;

namespace Abss.Draw
{

    public class DrawInstanceComputeBufferUnit
    {
        public ComputeBuffer TransformBuffer;
        public ComputeBuffer InstanceArgumentsBuffer;
    }


    public class DrawComputeInstanceBufferHolder : IDisposable
    {
        
        public List<DrawInstanceComputeBufferUnit> Units { get; }
            = new List<DrawInstanceComputeBufferUnit>();
        


        public void Initialize( DrawMeshResourceHolder resources )
        {

            foreach( var resource in resources.Units )
            {
                var buf = new DrawInstanceComputeBufferUnit();

                allocateComputeBuffers_( resource, buf );

                this.Units.Add( buf );
            }

            return;


            void allocateComputeBuffers_
                ( DrawMeshCsResourceUnit resourceUnit, DrawInstanceComputeBufferUnit bufferUnit )
            {

                var mesh = resourceUnit.Mesh;
                var mat = resourceUnit.Material;

                var boneLength = mesh.bindposes.Length;
                var stride = Marshal.SizeOf( typeof( float4 ) ) * resourceUnit.VectorLengthInBone;
                var bufferLength = resourceUnit.MaxInstance * resourceUnit.VectorLengthInBone * boneLength;

                bufferUnit.TransformBuffer =
                    new ComputeBuffer( bufferLength, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable );
                bufferUnit.InstanceArgumentsBuffer =
                    ComputeShaderUtility.CreateIndirectArgumentsBuffer();

                mat.SetBuffer( "bones", bufferUnit.TransformBuffer );
                mat.SetInt( "boneLength", boneLength );
            }

        }


        

        public void Dispose()
        {
            foreach( var x in this.Units )
            {
                x.TransformBuffer.Dispose();
                x.InstanceArgumentsBuffer.Dispose();
            }
        }
    }
}
