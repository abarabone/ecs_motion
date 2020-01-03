using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using System.Runtime.InteropServices;
using System;

using Abss.Geometry;
using Abss.Misc;

namespace Abss.Draw
{

    // シングルトン -----------------------

    public class DrawSystemComputeTransformBufferData : IComponentData
    {
        public ComputeBuffer Transforms;
    }

    public struct DrawSystemNativeTransformBufferData : IComponentData
    {
        public SimpleNativeBuffer<float4, Temp> Transforms;
    }
    
    
    // メッシュごと -----------------------

    //public struct DrawModelBufferLinkerData : IComponentData
    //{
    //    public Entity BufferEntity;
    //}

    public struct DrawModelBoneUnitSizeData : IComponentData
    {
        public int VectorLengthInBone;
        public int BoneLength;
    }

    public struct DrawModelInstanceCounterData : IComponentData
    {
        public ThreadSafeCounter<Persistent> InstanceCounter;
    }
    public unsafe struct DrawModelInstanceOffsetData : IComponentData
    {
        public float4 *pVectorOffsetInBuffer;
    }

    public class DrawModelComputeArgumentsBufferData : IComponentData
    {
        public ComputeBuffer InstanceArgumentsBuffer;
    }

    public class DrawModelGeometryData : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }



    public unsafe struct SimpleNativeBuffer<T, Tallocator> : IDisposable
        where T : unmanaged
        where Tallocator : IAllocatorLabel, new()
    {

        public T* pBuffer { get; private set; }
        public int length_;


        public SimpleNativeBuffer( int length )
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            var allocator = new Tallocator().Label;

            this.pBuffer = (T*)UnsafeUtility.Malloc( size * length, align, allocator );
            this.length_ = length;//
        }

        public void Dispose()
        {
            var allocator = new Tallocator().Label;
            UnsafeUtility.Free( (void*)this.pBuffer, allocator );
        }

    }

}
