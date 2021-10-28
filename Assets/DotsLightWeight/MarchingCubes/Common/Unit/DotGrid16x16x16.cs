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

    public unsafe partial struct DotGrid16x16x16 : IDotGrid<DotGrid16x16x16>, IDisposable
    {
        public const int dotNum = 16 * 16 * 16;
        public const int xlineInGrid = 1 * 16 * 16;
        public const int shiftNum = 4;
        //public const int maxbitNum = 16;

        public int UnitOnEdge => 16;


        public uint* pXline { get; private set; }
        public int CubeCount;// DotCount に変更　あとで


        //public bool IsFullOrEmpty => (this.CubeCount & (dotNum - 1) ) == 0;
        //public bool IsFull => this.CubeCount == dotNum;
        //public bool IsEmpty => this.CubeCount == 0;

        //public GridFillMode FillModeBlankOrSolid => (GridFillMode)(this.CubeCount >> (16 - 1));
        //public GridFillMode FillMode
        //{
        //    get
        //    {
        //        var notfilled = math.select(-1, 0, (this.CubeCount & (dotNum - 1)) != 0);
        //        var solid = this.CubeCount >> (16 - 1);
        //        return (GridFillMode)( notfilled | solid );
        //    }
        //}


        public DotGrid16x16x16(GridFillMode fillmode) : this()
        {
            //const int size = sizeof( uint ) * xlineInGrid;

            var x = Allocater.Alloc(fillmode);//, size);
            this.pXline = x.pXline;
            this.CubeCount = x.CubeCount;
        }
        public DotGrid16x16x16(UIntPtr p, int cubeCount) : this()
        {
            this.pXline = (uint*)p;
            this.CubeCount = cubeCount;
        }

        public DotGrid16x16x16 Alloc(GridFillMode fillmode) => new DotGrid16x16x16(fillmode);

        public void Dispose()
        {
            if( this.pXline == null ) return;// struct なので、複製された場合はこのチェックも意味がない

            Allocater.Dispose((UIntPtr)this.pXline);
            this.pXline = null;
        }


        //public uint this[ int ix, int iy, int iz ]
        //{
        //    get => (uint)( this.pXline[ (iy << shiftNum) + iz ] >> ix & 1 );
            
        //    set
        //    {
        //        //if (value != 0 && value != 1) new ArgumentException();

        //        var i = (iy << shiftNum) + iz;
        //        var prev = this.pXline[i];
        //        if (value == 1)
        //        {
        //            var x = 1 << ix;
        //            this.pXline[i] |= (uint)x;
        //            if (prev != this.pXline[i]) this.CubeCount++;
        //        }
        //        if(value == 0)
        //        {
        //            var x = ~1 << ix;
        //            this.pXline[i] &= (uint)x;
        //            if (prev != this.pXline[i]) this.CubeCount--;
        //        }
        //    }
        //}
        //public uint this[int3 i]
        //{
        //    get => this[i.x, i.y, i.z];
        //    set => this[i.x, i.y, i.z] = value;
        //}


        static public DotGrid16x16x16 CreateDefaultGrid(GridFillMode fillmode)
        {
            return Allocater.Alloc(fillmode);//, size);
        }


        static class Allocater
        {

            static public unsafe DotGrid16x16x16 Alloc(GridFillMode fillMode)
            {
                //var align = UnsafeUtility.AlignOf<uint4>();
                const int align = 32;// 16;

                var p = (UIntPtr)UnsafeUtility.Malloc(16 * 16 * sizeof(uint)/2, align, Allocator.Persistent);

                return Fill(p, fillMode);
            }

            static public unsafe DotGrid16x16x16 Fill(UIntPtr p, GridFillMode fillMode)
            {
                switch (fillMode)
                {
                    case GridFillMode.Solid:
                        {
                            UnsafeUtility.MemSet((void*)p, 0xff, 16 * 16 * sizeof(uint)/2);

                            var cubeCount = 16 * 16 * 16;
                            return new DotGrid16x16x16(p, cubeCount);
                        }
                    case GridFillMode.Blank:
                        {
                            UnsafeUtility.MemClear((void*)p, 16 * 16 * sizeof(uint)/2);

                            var cubeCount = 0;
                            return new DotGrid16x16x16(p, cubeCount);
                        }
                    default:
                        return default;
                }
            }


            static public unsafe void Dispose(UIntPtr p)
            {
                UnsafeUtility.Free((uint*)p, Allocator.Persistent);
            }
        }
    }


}
