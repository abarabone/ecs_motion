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
        public int OffsetInBuffer;

        public int VectorLengthInBone;
    }

    

    public class DrawNativeInstanceBufferHolder : IDisposable
    {
        
        public NativeArray<DrawInstanceNativeBufferUnit> Units;

        public JobAllocatableBuffer<float4, Temp> InstanceBoneTempVectors;



        public unsafe void Initialize( DrawMeshResourceHolder resources )
        {
            var q =
                from x in resources.Units
                select new DrawInstanceNativeBufferUnit
                {
                    InstanceCounter = new ThreadSafeCounter<Persistent>( 0 ),
                    VectorLengthInBone = x.VectorLengthInBone,
                };
            this.Units = q.ToNativeArray( Allocator.Persistent );
        }


        public void ResetOnFrame()
        {
            foreach( var x in this.Units )
                x.InstanceCounter.Reset();

            this.InstanceBoneTempVectors = new JobAllocatableBuffer<float4, Temp>( 0 );
        }

        public void ClearOnFrame()
        {
            this.InstanceBoneTempVectors.Dispose();
        }


        public void Dispose()
        {
            foreach( var x in this.Units )
            {
                x.InstanceCounter.Dispose();
            }

            this.Units.Dispose();
            this.InstanceBoneTempVectors.Dispose();
        }

    }


    public unsafe struct JobAllocatableBuffer<T, Tallocator> : IDisposable
        where T : struct
        where Tallocator : IAllocatorLabel, new()
    {

        [NativeDisableContainerSafetyRestriction]
        NativeArray<int> bufferPointerHolder;

        public void* pBuffer
        {
            get => (void*)this.bufferPointerHolder[ 0 ];
            private set => this.bufferPointerHolder[ 0 ] = (int)value;
        }


        public JobAllocatableBuffer( int length )
        {
            var allocator = new Tallocator().Label;
            this.bufferPointerHolder = new NativeArray<int>( 1, allocator, NativeArrayOptions.ClearMemory );

            if( length > 0 ) this.AllocateBuffer( length );
        }

        public void AllocateBuffer( int length )
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            var allocator = new Tallocator().Label;

            this.pBuffer = UnsafeUtility.Malloc( size * length, align, allocator );
        }

        public void Dispose()
        {
            if( !this.bufferPointerHolder.IsCreated ) return;

            if( this.pBuffer != null )
                disposeBuffer_( this.pBuffer );

            this.bufferPointerHolder.Dispose();
            this.bufferPointerHolder = new NativeArray<int>();


            void disposeBuffer_( void* pBuffer )
            {
                var allocator = new Tallocator().Label;
                UnsafeUtility.Free( (void*)pBuffer, allocator );
            }
        }

    }
}
