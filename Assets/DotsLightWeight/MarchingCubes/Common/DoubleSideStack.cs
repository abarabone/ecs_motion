using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes
{


    public unsafe struct DoubleSideStack<T> : IDisposable
        where T : unmanaged
    {
        //NativeArray<T> buffer;
        [NativeDisableUnsafePtrRestriction]
        T* buffer;
        int bufferLength;
        public int FrontCount { get; private set; }
        public int BackCount { get; private set; }

        public DoubleSideStack(int maxLength)
        {
            //this.buffer = new NativeArray<T>(maxLength, Allocator.Persistent);
            this.buffer = (T*)UnsafeUtility.Malloc(sizeof(T) * maxLength, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            this.bufferLength = maxLength;
            this.FrontCount = 0;
            this.BackCount = 0;
        }
        public void Dispose() => UnsafeUtility.Free(this.buffer, Allocator.Persistent);//this.buffer.Dispose();


        public bool PushToFront(T item)
        {
            if (this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.FrontCount++;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }
        public bool PushToBack(T item)
        {
            if (this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.bufferLength - ++this.BackCount;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }

        public bool PopFromFront(out T item)
        {
            if (this.FrontCount > 0)
            {
                var i = --this.FrontCount;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }
        public bool PopFromBack(out T item)
        {
            if (this.BackCount > 0)
            {
                var i = this.bufferLength - this.BackCount--;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }

        public T PopFromFront()
        {
            this.PopFromFront(out var item);
            return item;
        }
        public T PopFromBack()
        {
            this.PopFromBack(out var item);
            return item;
        }
    }

}
