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
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.Draw
{

    using Abarabone.Geometry;
    using Abarabone.Misc;


    // UnsafeList とかを使ったほうがよさそう、たぶん同じ用途
    public unsafe struct SimpleNativeBuffer<T> : IDisposable
        where T : unmanaged
    {

        public T* pBuffer { get; private set; }
        public int Length { get; private set; }
        public Allocator Allocator { get; private set; }

        public SimpleNativeBuffer(int length, Allocator allocator = Allocator.Temp)
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();

            this.pBuffer = (T*)UnsafeUtility.Malloc(size * length, align, allocator);
            this.Length = length;
            this.Allocator = allocator;
        }

        public void Dispose()
        {
            if (this.pBuffer == null) return;

            UnsafeUtility.Free((void*)this.pBuffer, this.Allocator);

            this.pBuffer = null;
        }

    }

    public static class SimpleNativeBufferUtility
    {
        static public unsafe NativeArray<T> AsNativeArray<T>(ref this SimpleNativeBuffer<T> buffer)
            where T : unmanaged
        {
            var na = NativeArrayUnsafeUtility
                .ConvertExistingDataToNativeArray<T>(buffer.pBuffer, buffer.Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility
                .SetAtomicSafetyHandle(ref na, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif

            return na;
        }
    }
}