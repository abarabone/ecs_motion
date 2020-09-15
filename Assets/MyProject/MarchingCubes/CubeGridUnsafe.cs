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
        NotFill = -1,
        Blank = 0,
        Solid = 1,
    };

    

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CubeGrid32x32x32Unsafe
    {
        const int maxNum = 32 * 32 * 32;
        const int xlineInGrid = 1 * 32 * 32;
        const int shiftNum = 5;


        [FieldOffset(0)]
        public uint* pUnits;
        [FieldOffset(4)]
        public int CubeCount;

        [FieldOffset(0)]
        public ulong Value;

        public bool IsFullOrEmpty => (this.CubeCount & (maxNum - 1) ) == 0;
        public bool IsFull => this.CubeCount == maxNum;
        public bool IsEmpty => this.CubeCount == 0;


        public CubeGrid32x32x32Unsafe(GridFillMode fillmode) : this()
        {
            const int size = sizeof( uint ) * xlineInGrid;

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
            get => (uint)( this.pUnits[ (iy << shiftNum) + iz ] >> ix & 1 );
            
            set
            {
                //if (value != 0 && value != 1) new ArgumentException();

                var i = (iy << shiftNum) + iz;
                var oldbit = (this.pUnits[i] >> ix) & 1;
                var newbit = value;// & 1;

                var bitIfChange = oldbit ^ newbit;

                this.pUnits[ i ] = (uint)((oldbit ^ bitIfChange) << ix);

                var d = (int)(newbit << 1) - 1;
                this.CubeCount += d * (int)bitIfChange;
            }
        }
        public uint this[int3 i]
        {
            get => this[i.x, i.y, i.z];
            set => this[i.x, i.y, i.z] = value;
        }


        static public CubeGrid32x32x32Unsafe CreateDefaultCube(GridFillMode fillmode)
        {
            //const int size = sizeof(uint) * 4;// 工夫すると 16 bytes ですむ、でもなんか遅い？？不思議だけど
            const int size = sizeof(uint) * xlineInGrid;
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


    public unsafe struct CubeOperator
    {
        CubeGrid32x32x32UnsafePtr gridInArea;
        CubeGrid32x32x32UnsafePtr defaultGridTop;

        UIntPtr pGridSwapOld;


        public uint this[int3 i]
        {
            get => this[i.x, i.y, i.z];
            set => this[i.x, i.y, i.z] = value;
        }
        public uint this[int ix, int iy, int iz]
        {
            get => this.gridInArea[ix, iy, iz];

            set
            {
                ref var cube = ref *this.gridInArea.p;


                var i = (iy << 5) + iz;
                var oldbit = (cube.pUnits[i] >> ix) & 1;
                var newbit = value;// & 1;

                var bitIfChange = oldbit ^ newbit;

                // 変化がなければこのまま抜ける
                if (bitIfChange != 0) return;


                var xline = (uint)((oldbit ^ bitIfChange) << ix);
                var d = (int)(newbit << 1) - 1;
                var cubeCount = cube.CubeCount + d * (int)bitIfChange;


                if (cube.IsFullOrEmpty)
                {
                    // デフォルト→変化あり：確保
                    var fillMode = (GridFillMode)(cube.CubeCount >> 5);
                    var newGrid = CubeGridAllocater.Alloc(fillMode);

                    cube = newGrid;
                }
                else
                {
                    // 確保済→0or1：デフォルト
                    if ((cubeCount & (32 * 32 * 32 - 1)) == 0)// isBlankORSolid
                    {
                        var i_ = cube.CubeCount >> 5;
                        cube.Value = this.defaultGridTop.p[i_].Value;//this.pDefaultGridValue[i];

                        // 返すため
                        this.pGridSwapOld = (UIntPtr)cube.pUnits;
                        return;
                    }
                }

                cube.pUnits[i] = xline;
                cube.CubeCount = cubeCount;
            }
        }

        public void BackOldGrid(ref CubeGridGlobalData global)
        {
            global.GridStock.Add(this.pGridSwapOld);
            this.pGridSwapOld = default;
        }

    }

    static public unsafe class CubeGridExtension
    {

        static public ref CubeGrid32x32x32Unsafe AsRef(this CubeGrid32x32x32UnsafePtr cubePtr) => ref *cubePtr.p;


        static public Cubee With(ref this CubeGridArrayUnsafe grids, ref CubeGridGlobal global, )
        {

        }

        static public void a(ref this CubeGridArrayUnsafe arr,  ref CubeGridGlobalData globalData)
        {

            var _0or1 = math.sign(grid.CubeCount);
            var defaultGrid = globalData.GetDefaultGrid((GridFillMode)_0or1);


        }
    }

}
