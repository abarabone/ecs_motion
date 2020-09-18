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

namespace Abarabone.MarchingCubes
{

    static public unsafe class CubeGridExtension
    {

        static public ref CubeGrid32x32x32Unsafe AsRef(this CubeGrid32x32x32UnsafePtr cubePtr) => ref *cubePtr.p;

        static public ref CubeGrid32x32x32Unsafe AsRef(this UnsafeList<CubeGrid32x32x32Unsafe> list, int index) => ref *(list.Ptr + index);

        static public ref T AsRef<T>(this UnsafeList list, int index) where T : unmanaged => ref *((T*)list.Ptr + index);



        //static public Cubee With(ref this CubeGridArrayUnsafe grids, ref CubeGridGlobal global, )
        //{

        //}

        //static public void a(ref this CubeGridArrayUnsafe arr, ref CubeGridGlobalData globalData)
        //{

        //    var _0or1 = math.sign(grid.CubeCount);
        //    var defaultGrid = globalData.GetDefaultGrid((GridFillMode)_0or1);


        //}



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool isDefault
            (ref this DynamicBuffer<CubeGridGlobal.DefualtGridData> defaultGrids, CubeGrid32x32x32Unsafe grid)
        {
            var p = new uint2((uint)grid.pUnits).xx;
            var def = new uint2((uint)defaultGrids.Blank().pUnits, (uint)defaultGrids.Solid().pUnits);
            return math.any(p == def);
        }

        static ref CubeGrid32x32x32Unsafe getGridFromArea
            (
                ref this (CubeGridArea.BufferData, CubeGridArea.InfoTempData) x,
                int ix, int iy, int iz
            )
        {
            ref var areas = ref x.Item1;
            ref var areaInfo = ref x.Item2;

            var i3 = new int3(ix, iy, iz) + 1;
            var i = math.dot(i3, areaInfo.GridSpan);

            return ref areas.Grids.AsRef(i);
        }

        static public void rentGridFromGlobal
            (ref this CubeGridGlobal.BufferData buf, ref CubeGrid32x32x32Unsafe grid)
        {
            grid.pUnits = (uint*)buf.FreeGridStocks[buf.FreeGridStocks.length--];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public CubeGrid32x32x32Unsafe GetGrid
            (
                ref this (DynamicBuffer<CubeGridGlobal.DefualtGridData>, CubeGridArea.BufferData, CubeGridArea.InfoTempData) x,
                int ix, int iy, int iz
            )
        {
            ref var defaultGrids = ref x.Item1;
            ref var areas = ref x.Item2;
            ref var areaInfo = ref x.Item3;


            var i3 = new int3(ix, iy, iz) + 1;
            var i = math.dot(i3, areaInfo.GridSpan);

            var gridptr = new CubeGrid32x32x32UnsafePtr { p = areas.Grids.Ptr + i };
            ref var grid = ref gridptr.AsRef();

            if (!defaultGrids.isDefault(grid)) return grid;


            return grid;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void BackGridIfFilled
            (ref this (DynamicBuffer<CubeGridGlobal.DefualtGridData>, CubeGridGlobal.BufferData) x, ref CubeGrid32x32x32Unsafe grid)
        {
            ref var defaultGrids = ref x.Item1;
            ref var buf = ref x.Item2;


            if (grid.isDefault(defaultGrids)) return;

            if (!grid.IsFullOrEmpty) return;


            buf.FreeGridStocks.Add((UIntPtr)grid.pUnits);
            grid = defaultGrids.Get((GridFillMode)(grid.CubeCount >> 5));
        }
    }

}
