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

    // 全体で１つ -----------------------

    public class DrawComputeTransformBufferData : IComponentData
    {
        public ComputeBuffer TransformBuffer;
    }

    public unsafe struct DrawNativeTransformBufferData : IComponentData
    {
        public JobAllocatableBuffer<float4, Temp> TransformBuffer;
    }


    // メッシュごと -----------------------

    public struct DrawBoneInfoData : IComponentData
    {
        public int VectorLengthInBone;
        public int BoneLength;
    }

    public struct DrawInstanceCounterData : IComponentData
    {
        public ThreadSafeCounter<Persistent> InstanceCounter;
    }
    public struct DrawInstanceOffsetData : IComponentData
    {
        public int VectorOffsetInBuffer;
    }

    public class DrawComputeArgumentsBufferData : IComponentData
    {
        public ComputeBuffer InstanceArgumentsBuffer;
    }

    public class DrawMeshData : IComponentData
    {
        public Mesh Mesh;
        public Material Material;
    }



    public unsafe struct JobAllocatableBuffer<T, Tallocator> : IDisposable
        where T : unmanaged
        where Tallocator : IAllocatorLabel, new()
    {

        public T* p { get; private set; }


        public void Allocate( int length )
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            var allocator = new Tallocator().Label;

            this.p = (T*)UnsafeUtility.Malloc( size * length, align, allocator );
        }

        public void Dispose()
        {
            var allocator = new Tallocator().Label;
            UnsafeUtility.Free( (void*)this.p, allocator );
        }

    }

}
