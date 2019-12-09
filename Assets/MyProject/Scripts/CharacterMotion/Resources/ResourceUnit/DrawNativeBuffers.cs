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
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Abss.Cs;
using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    public unsafe struct DrawInstanceNativeBufferUnit
    {
        public ThreadSafeCounter<Persistent> InstanceCounter;
        //public NativeSlice<float4> InstanceBoneVectors;
        public float4* pInstanceBoneVectors;
        public int VectorLengthOfBone;
        public int OffsetInBuffer;
    }

    

    public class DrawNativeInstanceBufferHolder : IDisposable
    {
        
        public NativeArray<DrawInstanceNativeBufferUnit> Units;

        public NativeArray<float4> InstanceBoneVectors;


        public unsafe void Initialize( DrawMeshResourceHolder resources )
        {
            var arrayLengths = resources.Units
                .Select( x => x.VectorLengthOfBone * x.Mesh.bindposes.Length * x.MaxInstance )
                .ToArray();

            this.Units = 
                new NativeArray<DrawInstanceNativeBufferUnit>( arrayLengths.Length, Allocator.Persistent );

            this.InstanceBoneVectors =
                new NativeArray<float4>( arrayLengths.Sum(), Allocator.Persistent );

            var start = 0;
            for( var i = 0; i < arrayLengths.Length; i++ )
            {
                this.Units[ i ] = new DrawInstanceNativeBufferUnit
                {
                    //InstanceBoneVectors = this.InstanceBoneVectors.Slice( start, arrayLengths[ i ] ),
                    pInstanceBoneVectors = (float4*)NativeArrayUnsafeUtility.GetUnsafePtr( this.InstanceBoneVectors ) + start,
                    InstanceCounter = new ThreadSafeCounter<Persistent>( 0 ),
                    OffsetInBuffer = start,
                    VectorLengthOfBone = resources.Units[i].VectorLengthOfBone,
                };

                start += arrayLengths[ i ];
            }
        }

        public void Reset()
        {
            foreach( var x in this.Units )
                x.InstanceCounter.Reset();
        }


        public void Dispose()
        {
            foreach( var x in this.Units )
            {
                x.InstanceCounter.Dispose();
            }

            this.Units.Dispose();
            this.InstanceBoneVectors.Dispose();
        }

    }

}
