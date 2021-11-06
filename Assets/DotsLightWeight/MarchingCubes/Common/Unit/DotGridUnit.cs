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

namespace DotsLite.MarchingCubes
{

    public enum DotGridType
    {
        DotGrid32x32x32 = 32,
        DotGrid16x16x16 = 16,
    }


    public enum GridFillMode
    {
        NotFilled = -1,
        Blank = 0,
        Solid = 1,
        Null = 2,
    };

    public unsafe interface IDotGrid<TGrid> : IDisposable
        where TGrid : struct, IDotGrid<TGrid>
    {
        uint* pXline { get; }
        //int CubeCount { get; }

        int UnitOnEdge { get; }
        int XLineBufferLength { get; }

        TGrid Alloc(GridFillMode fillmode);

        void Fill();
    }


    public static partial class DotGrid
    {
        public static TGrid Create<TGrid>(GridFillMode fillmode) where TGrid : struct, IDotGrid<TGrid> =>
            new TGrid().Alloc(fillmode);


        public static class Allocater<TGrid>
            where TGrid : struct, IDotGrid<TGrid>
        {

            static public unsafe void* Alloc(GridFillMode fillMode)
            {
                //var align = UnsafeUtility.AlignOf<uint4>();
                const int align = 32;

                var p = UnsafeUtility.Malloc(new TGrid().XLineBufferLength * sizeof(uint), align, Allocator.Persistent);

                return Fill(p, fillMode);
            }

            static public unsafe void* Fill(void* p, GridFillMode fillMode)
            {
                switch (fillMode)
                {
                    case GridFillMode.Solid:
                        {
                            UnsafeUtility.MemSet(p, 0xff, new TGrid().XLineBufferLength * sizeof(uint));
                            return p;
                        }
                    case GridFillMode.Blank:
                        {
                            UnsafeUtility.MemClear(p, new TGrid().XLineBufferLength * sizeof(uint));
                            return p;
                        }
                    default:
                        return p;
                }
            }


            static public unsafe void Dispose(void* p)
            {
                UnsafeUtility.Free(p, Allocator.Persistent);
            }
        }
    }

}
