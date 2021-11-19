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
        Grid32x32x32 = 32,
        Grid16x16x16 = 16,
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

                public int3 index => value.xyz;
                public int serial => value.w;

                public IndexInArea(int3 index, int3 span)
                {
                    var serial = math.dot(index, span);
                    this.value = new int4(index, serial);
                }
                public IndexInArea Create(int3 offset, int3 span) =>
                    new IndexInArea(this.index + offset, span);
            }
        }
    }

}
