using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace DotsLite.MarchingCubes.Data
{

    public enum BitGridType
    {
        none,
        //Grid32x32x32 = 0b_100000_100000_100000,//32x32x32,
        //Grid16x16x16 = 0b_010000_010000_010000,//16x16x16,
        //Grid32x4x32 = 0b_100000_000100_100000,//32x4x32,
        Grid32x32x32,
        Grid16x16x16,
        Grid32x4x32,
    }

    public static class Ext
    {
        //public static int4 ToInt4(this BitGridType x)
        //{
        //    var i = (uint)x;
        //    return (int4)new uint4(i >> 12 & 0b111111, i >> 6 & 0b111111, i >> 0 & 0b111111, 0);
        //}
        public static int4 ToInt4(this BitGridType x) => x switch
        {
            BitGridType.Grid32x32x32 => new int4(32, 32, 32, 0),
            BitGridType.Grid16x16x16 => new int4(16, 16, 16, 0),
            BitGridType.Grid32x4x32 => new int4(32, 4, 32, 0),
            _ => default,
        };
        //public static int CalcXLineBufferSize()
    }


    public enum GridFillMode
    {
        NotFilled = -1,
        Blank = 0,
        Solid = 1,
        Null = 2,
    };

    public static partial class BitGrid
    {
        public static partial class Tools
        {
            static public unsafe uint* Alloc(int bufferLength, GridFillMode fillMode)
            {
                ;
                const int align = 32;

                var p = UnsafeUtility.Malloc(bufferLength * sizeof(uint), align, Allocator.Persistent);

                return Fill((uint*)p, bufferLength, fillMode);
            }

            static public unsafe uint* Fill(uint* p, int bufferLength, GridFillMode fillMode)
            {
                switch (fillMode)
                {
                    case GridFillMode.Solid:
                        {
                            UnsafeUtility.MemSet(p, 0xff, bufferLength * sizeof(uint));
                            return p;
                        }
                    case GridFillMode.Blank:
                        {
                            UnsafeUtility.MemClear(p, bufferLength * sizeof(uint));
                            return p;
                        }
                    default:
                        return p;
                }
            }


            static public unsafe uint* Dispose(uint* p)
            {
                UnsafeUtility.Free(p, Allocator.Persistent);
                return null;
            }


            public struct IndexInArea
            {
                public int4 value;
                public int4 span;

                public int3 index => value.xyz;
                public int serial => value.w;

                public IndexInArea(int3 index, int3 span)
                {
                    var serial = math.dot(index, span);
                    this.value = new int4(index, serial);
                    this.span = new int4(span, 0);
                }

                public IndexInArea Create(int3 offset) =>
                    new IndexInArea(this.index + offset, this.span.xyz);

                public IndexInArea Create(int ofsx, int ofsy, int ofsz) =>
                    new IndexInArea(this.index + new int3(ofsx, ofsy, ofsz), this.span.xyz);
            }
        }
    }

}
