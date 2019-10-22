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

using Abss.Cs;
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

        const int MaxInstance = 10000;

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
                ( DrawMeshCsResourceUnit resouceUnit, DrawInstanceComputeBufferUnit bufferUnit )
            {

                var mesh = resouceUnit.Mesh;
                var mat = resouceUnit.Material;

                var boneLength = mesh.bindposes.Length;
                var stride = Marshal.SizeOf( typeof( float4 ) ) * resouceUnit.VectorLengthOfBone;
                var bufferLength = MaxInstance * resouceUnit.VectorLengthOfBone * boneLength;

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
