using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System;

namespace Abarabone.MarchingCubes
{

    public enum GridFillMode
    {
        Blank,
        Solid,
    };

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CubeGrid32x32x32Unsafe
    {
        [FieldOffset(0)]
        public uint* pUnits;
        [FieldOffset(4)]
        public int CubeCount;

        [FieldOffset(0)]
        public ulong Value;

        public bool IsFullOrEmpty => ( this.CubeCount & 0x7fff ) == 0;
        public bool IsFull => this.CubeCount == 0x8000;
        public bool IsEmpty => this.CubeCount == 0;


        public CubeGrid32x32x32Unsafe(GridFillMode fillmode) : this()
        {
            const int size = sizeof( uint ) * 1 * 32 * 32;

            var x = CubeGridAllocater.Alloc(fillmode, size);
            this.pUnits = x.pUnits;
            this.CubeCount = x.CubeCount;
        }
        public CubeGrid32x32x32Unsafe(UIntPtr p, int cubeCount) : this()
        {
            this.pUnits = (uint*)p;
            this.CubeCount = cubeCount;
        }

        public void Dispose()
        {
            if( this.pUnits == null ) return;// struct なので、複製された場合はこのチェックも意味がない

            CubeGridAllocater.Dispose((UIntPtr)this.pUnits);
            this.pUnits = null;
        }


        public uint this[ int ix, int iy, int iz ]
        {
            get
            {
                return (uint)( this.pUnits[ ( iy << 5 ) + iz ] >> ix & 1 );
            }
            set
            {
                var maskedValue = value & 1;

                var i = ( iy << 5 ) + iz;
                var b = this.pUnits[ i ];
                this.pUnits[ i ] ^= (uint)( (b & 1 << ix) ^ (maskedValue << ix) );

                this.CubeCount += (int)( (maskedValue << 1) - 1 );
            }
        }


        static public CubeGrid32x32x32Unsafe CreateDefaultCube(GridFillMode fillmode)
        {
            //const int size = sizeof(uint) * 4;// 工夫すると 16 bytes ですむ、でもなんか遅い？？不思議だけど
            const int size = sizeof(uint) * 1 * 32 * 32;
            return CubeGridAllocater.Alloc(fillmode, size);
        }


    }



    static class CubeGridAllocater
    {

        static public unsafe CubeGrid32x32x32Unsafe Alloc(GridFillMode fillMode, int size = 1 * 32 * 32)
        {
            //var align = UnsafeUtility.AlignOf<uint4>();
            const int align = 16;

            var p = (UIntPtr)UnsafeUtility.Malloc(size, align, Allocator.Persistent);

            return Fill(p, fillMode, size);
        }

        static public unsafe CubeGrid32x32x32Unsafe Fill(UIntPtr p, GridFillMode fillMode, int size = 1 * 32 * 32)
        {
            if (fillMode == GridFillMode.Solid)
            {
                UnsafeUtility.MemSet((void*)p, 0xff, size);
                var cubeCount = 32 * 32 * 32;

                return new CubeGrid32x32x32Unsafe(p, cubeCount);
            }
            else
            {
                UnsafeUtility.MemClear((void*)p, size);
                var cubeCount = 0;

                return new CubeGrid32x32x32Unsafe(p, cubeCount);
            }
        }
        

        static public unsafe void Dispose(UIntPtr p)
        {
            UnsafeUtility.Free((uint*)p, Allocator.Persistent);
        }
    }


}
