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

    public enum GridFillMode
    {
        NotFilled = -1,
        Blank = 0,
        Solid = 1,
        Null = 2,
    };

    

    public unsafe struct DotGrid32x32x32Unsafe : IDotGrid
    {
        public const int dotNum = 32 * 32 * 32;
        public const int xlineInGrid = 1 * 32 * 32;
        public const int shiftNum = 5;
        //public const int maxbitNum = 16;


        public uint* pXline;
        public int CubeCount;// DotCount に変更　あとで


        public bool IsFullOrEmpty => (this.CubeCount & (dotNum - 1) ) == 0;
        public bool IsFull => this.CubeCount == dotNum;
        public bool IsEmpty => this.CubeCount == 0;

        public GridFillMode FillModeBlankOrSolid => (GridFillMode)(this.CubeCount >> (16 - 1));
        public GridFillMode FillMode
        {
            get
            {
                var notfilled = math.select(-1, 0, (this.CubeCount & (dotNum - 1)) != 0);
                var solid = this.CubeCount >> (16 - 1);
                return (GridFillMode)( notfilled | solid );
            }
        }


        public DotGrid32x32x32Unsafe(GridFillMode fillmode) : this()
        {
            //const int size = sizeof( uint ) * xlineInGrid;

            var x = Allocater.Alloc(fillmode);//, size);
            this.pXline = x.pXline;
            this.CubeCount = x.CubeCount;
        }
        public DotGrid32x32x32Unsafe(UIntPtr p, int cubeCount) : this()
        {
            this.pXline = (uint*)p;
            this.CubeCount = cubeCount;
        }

        public void Dispose()
        {
            if( this.pXline == null ) return;// struct なので、複製された場合はこのチェックも意味がない

            Allocater.Dispose((UIntPtr)this.pXline);
            this.pXline = null;
        }


        public uint this[ int ix, int iy, int iz ]
        {
            get => (uint)( this.pXline[ (iy << shiftNum) + iz ] >> ix & 1 );
            
            set
            {
                //if (value != 0 && value != 1) new ArgumentException();

                var i = (iy << shiftNum) + iz;
                var prev = this.pXline[i];
                if (value == 1)
                {
                    var x = 1 << ix;
                    this.pXline[i] |= (uint)x;
                    if (prev != this.pXline[i]) this.CubeCount++;
                }
                if(value == 0)
                {
                    var x = ~1 << ix;
                    this.pXline[i] &= (uint)x;
                    if (prev != this.pXline[i]) this.CubeCount--;
                }

                //var i = (iy << shiftNum) + iz;
                //var oldbit = (this.pUnits[i] >> ix) & 1;
                //var newbit = value;// & 1;

                //var bitIfChange = oldbit ^ newbit;

                //this.pUnits[ i ] = (uint)((oldbit ^ bitIfChange) << ix);

                //var d = (int)(newbit << 1) - 1;
                //this.CubeCount += d * (int)bitIfChange;
            }
        }
        public uint this[int3 i]
        {
            get => this[i.x, i.y, i.z];
            set => this[i.x, i.y, i.z] = value;
        }


        static public DotGrid32x32x32Unsafe CreateDefaultCube(GridFillMode fillmode)
        {
            //const int size = sizeof(uint) * 4;// 工夫すると 16 bytes ですむ、でもなんか遅い？？不思議だけど
            //const int size = sizeof(uint) * xlineInGrid;
            return Allocater.Alloc(fillmode);//, size);
        }


        static class Allocater
        {

            static public unsafe DotGrid32x32x32Unsafe Alloc(GridFillMode fillMode)
            {
                //var align = UnsafeUtility.AlignOf<uint4>();
                const int align = 16;

                var p = (UIntPtr)UnsafeUtility.Malloc(32 * 32 * sizeof(uint), align, Allocator.Persistent);

                return Fill(p, fillMode);
            }

            static public unsafe DotGrid32x32x32Unsafe Fill(UIntPtr p, GridFillMode fillMode)
            {
                switch (fillMode)
                {
                    case GridFillMode.Solid:
                        {
                            UnsafeUtility.MemSet((void*)p, 0xff, 32 * 32 * sizeof(uint));

                            var cubeCount = 32 * 32 * 32;
                            return new DotGrid32x32x32Unsafe(p, cubeCount);
                        }
                    case GridFillMode.Blank:
                        {
                            UnsafeUtility.MemClear((void*)p, 32 * 32 * sizeof(uint));

                            var cubeCount = 0;
                            return new DotGrid32x32x32Unsafe(p, cubeCount);
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



    //static class DotGridAllocater
    //{

    //    static public unsafe DotGrid32x32x32Unsafe Alloc(GridFillMode fillMode, int dotnum = 32 * 32 * 32)
    //    {
    //        //var align = UnsafeUtility.AlignOf<uint4>();
    //        const int align = 32;// 16;

    //        var p = (UIntPtr)UnsafeUtility.Malloc(dotnum >> 3, align, Allocator.Persistent);

    //        return Fill(p, fillMode, dotnum);
    //    }

    //    static public unsafe DotGrid32x32x32Unsafe Fill(UIntPtr p, GridFillMode fillMode, int size = 32 * 32 * 32)
    //    {
    //        if (fillMode == GridFillMode.Solid)
    //        {
    //            UnsafeUtility.MemSet((void*)p, 0xff, size >> 3);
    //            var cubeCount = 32 * 32 * 32;

    //            return new DotGrid32x32x32Unsafe(p, cubeCount);
    //        }
    //        else
    //        {
    //            UnsafeUtility.MemClear((void*)p, size >> 3);
    //            var cubeCount = 0;

    //            return new DotGrid32x32x32Unsafe(p, cubeCount);
    //        }
    //    }


    //    static public unsafe void Dispose(UIntPtr p)
    //    {
    //        UnsafeUtility.Free((uint*)p, Allocator.Persistent);
    //    }
    //}


    //public unsafe struct CubeOperator
    //{
    //    DotGrid32x32x32UnsafePtr gridInArea;
    //    DotGrid32x32x32UnsafePtr defaultGridTop;

    //    UIntPtr pGridSwapOld;


    //    public uint this[int3 i]
    //    {
    //        get => this[i.x, i.y, i.z];
    //        set => this[i.x, i.y, i.z] = value;
    //    }
    //    public uint this[int ix, int iy, int iz]
    //    {
    //        get => this.gridInArea[ix, iy, iz];

    //        set
    //        {
    //            ref var cube = ref *this.gridInArea.p;


    //            var i = (iy << 5) + iz;
    //            var oldbit = (cube.pUnits[i] >> ix) & 1;
    //            var newbit = value;// & 1;

    //            var bitIfChange = oldbit ^ newbit;

    //            // 変化がなければこのまま抜ける
    //            if (bitIfChange != 0) return;


    //            var xline = (uint)((oldbit ^ bitIfChange) << ix);
    //            var d = (int)(newbit << 1) - 1;
    //            var cubeCount = cube.CubeCount + d * (int)bitIfChange;


    //            if (cube.IsFullOrEmpty)
    //            {
    //                // デフォルト→変化あり：確保
    //                var fillMode = (GridFillMode)(cube.CubeCount >> 5);
    //                var newGrid = DotGridAllocater.Alloc(fillMode);

    //                cube = newGrid;
    //            }
    //            else
    //            {
    //                // 確保済→0or1：デフォルト
    //                if ((cubeCount & (32 * 32 * 32 - 1)) == 0)// isBlankORSolid
    //                {
    //                    var i_ = cube.CubeCount >> 5;
    //                    cube.Value = this.defaultGridTop.p[i_].Value;//this.pDefaultGridValue[i];

    //                    // 返すため
    //                    this.pGridSwapOld = (UIntPtr)cube.pUnits;
    //                    return;
    //                }
    //            }

    //            cube.pUnits[i] = xline;
    //            cube.CubeCount = cubeCount;
    //        }
    //    }

    //    public void BackOldGrid(ref DotGridGlobalData global)
    //    {
    //        global.GridStock.Add(this.pGridSwapOld);
    //        this.pGridSwapOld = default;
    //    }

    //}

}
