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

namespace Abss.Draw
{
    

    public class DrawCsInstanceComputeBufferUnit
    {
        public SimpleComputeBuffer<float4> TransformBuffer;// 使いまわしできれば、個別には不要
        public SimpleIndirectArgsBuffer InstanceArgumentsBuffer;
    }

    public struct DrawCsInstanceNativeBufferUnit
    {
        public ThreadSafeCounter<Persistent> InstanceCounter;
        public NativeSlice<float4> InstanceBoneVectors;
        //public int BoneLength;
        public int VectorLengthOfBone;
        public int OffsetInBuffer;
    }


    public class DrawCsInstanceBufferHolder : IDisposable
    {


        public List<DrawCsInstanceComputeBufferUnit> CsBufferEveryModels { get; }
            = new List<DrawCsInstanceComputeBufferUnit>();

        public NativeArray<DrawCsInstanceNativeBufferUnit> NativeBufferEveryModels { get; private set; }


        


        public void Initialize( DrawMeshResourceHolder resources )
        {


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
            void allocateNativeBuffers_()
            {

            }
        }

        

        public void Dispose()
        {
            foreach( var x in this.CsBufferEveryModels )
            {
                x.TransformBuffer.Dispose();
                x.InstanceArgumentsBuffer.Dispose();
            }
            
            foreach( var x in this.NativeBufferEveryModels )
            {
                x.InstanceCounter.Dispose();
            }

            this.NativeBufferEveryModels.Dispose();
            //this.instanceBoneVectors.Dispose();
        }
    }
}
