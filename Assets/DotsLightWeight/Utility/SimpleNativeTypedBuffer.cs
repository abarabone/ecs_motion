﻿using System.Collections;
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


    public unsafe struct SimpleNativeTypedBuffer<T, Tallocator> : IDisposable
        where T : unmanaged
        where Tallocator : IAllocatorLabel, new()
    {

        public T* pBuffer { get; private set; }
        public int Length { get; private set; }
        public Allocator Allocator => new Tallocator().Label;

        public SimpleNativeTypedBuffer(int length)
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            var allocator = new Tallocator().Label;

            this.pBuffer = (T*)UnsafeUtility.Malloc(size * length, align, allocator);
            this.Length = length;
        }

        public void Dispose()
        {
            var allocator = new Tallocator().Label;
            UnsafeUtility.Free((void*)this.pBuffer, allocator);
        }

    }

}