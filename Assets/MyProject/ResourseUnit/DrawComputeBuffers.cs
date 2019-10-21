using System;
using System.Collections;
using System.Collections.Generic;
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

namespace Abss.Draw
{

    public class DrawCsInstanceComputeBufferUnit
    {
        public SimpleComputeBuffer<float4> TransformBuffer;// 使いまわしできれば、個別には不要
        public SimpleIndirectArgsBuffer InstanceArgumentsBuffer;
    }


    public class DrawComputeInstanceBufferHolder : IDisposable
    {


        public List<DrawCsInstanceComputeBufferUnit> ComputeBufferEveryModels { get; }
            = new List<DrawCsInstanceComputeBufferUnit>();
        


        public void Initialize( DrawMeshResourceHolder resources )
        {

            foreach( var x in (resources.Units, this.ComputeBufferEveryModels).Zip() )
            {
                allocateComputeBuffers_( x.x, x.y );

            }

            return;


            void allocateComputeBuffers_
                ( DrawMeshCsResourceUnit resouceUnit, DrawCsInstanceComputeBufferUnit bufferUnit )
            {

                var mesh = resouceUnit.Mesh;
                var mat = resouceUnit.Material;
                var boneLength = mesh.bindposes.Length;

                var transformBuffer = new SimpleComputeBuffer<float4>( "bones", 10000 * 2 * boneLength );
                var instanceArgumentsBuffer = new SimpleIndirectArgsBuffer().CreateBuffer();
                
                mat.SetBuffer( transformBuffer );
                mat.SetInt( "boneLength", boneLength );


                bufferUnit.TransformBuffer = transformBuffer;
                bufferUnit.InstanceArgumentsBuffer = instanceArgumentsBuffer;
            }

        }


        public void Reset()
        {

        }

        

        public void Dispose()
        {
            foreach( var x in this.ComputeBufferEveryModels )
            {
                x.TransformBuffer.Dispose();
                x.InstanceArgumentsBuffer.Dispose();
            }
        }
    }
}
