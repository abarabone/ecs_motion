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

    public struct DrawComputeInstanceNativeBufferUnit
    {
        public ThreadSafeCounter<Persistent> InstanceCounter;
        public NativeSlice<float4> InstanceBoneVectors;
        //public int BoneLength;
        public int VectorLengthOfBone;
        public int OffsetInBuffer;
    }

    

    public class DrawNativeInstanceBufferHolder : IDisposable
    {


        public List<DrawCsInstanceComputeBufferUnit> ComputeBufferEveryModels { get; }
            = new List<DrawCsInstanceComputeBufferUnit>();


        public NativeArray<DrawComputeInstanceNativeBufferUnit> NativeBufferEveryModels { get; private set; }


        public Initialize( DrawMeshResourceHolder resources )
        {

            void allocateComputeBuffers_
                ( DrawMeshCsResourceUnit resouceUnit, DrawComputeInstanceNativeBufferUnit bufferUnit )
            {

            }
            void setValues_
                ( DrawMeshCsResourceUnit resourceUnit, DrawComputeInstanceNativeBufferUnit bufferUnit )
            {
                bufferUnit.VectorLengthOfBone = 
            }
        }
        

        public void Dispose()
        {
            foreach( var x in this.NativeBufferEveryModels )
            {
                x.InstanceCounter.Dispose();
            }

            this.NativeBufferEveryModels.Dispose();
            //this.instanceBoneVectors.Dispose();
        }

    }

}
