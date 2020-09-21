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



        static bool isDefault
            (ref this DynamicBuffer<CubeGridGlobal.DefualtGridData> defaultGrids, CubeGrid32x32x32Unsafe grid)
        {
            //var p = new uint2((uint)grid.pUnits).xx;
            //var def = new uint2((uint)defaultGrids.Blank().pUnits, (uint)defaultGrids.Solid().pUnits);
            //return math.any(p == def);
            return grid.pUnits == defaultGrids.Blank().pUnits | grid.pUnits == defaultGrids.Solid().pUnits;
        }

        static GridFillMode getFillMode(ref this CubeGrid32x32x32Unsafe grid)
        {
            return (GridFillMode)(grid.CubeCount >> 5);
        }


        /// <summary>
        /// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static CubeGrid32x32x32UnsafePtr getGridFromArea
            (
                ref this (CubeGridArea.BufferData, CubeGridArea.InfoTempData) x,
                int ix, int iy, int iz
            )
        {
            ref var areas = ref x.Item1;
            ref var areaInfo = ref x.Item2;

            var i3 = new int3(ix, iy, iz) + 1;
            var i = math.dot(i3, areaInfo.GridSpan);

            return new CubeGrid32x32x32UnsafePtr { p = areas.Grids.Ptr + i };
        }


        /// <summary>
        /// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        /// 取得したグリッドポインタからは、グリッドエリア上のグリッドそのものを書き換えることができる。
        /// また、取得すべきグリッドがデフォルトだった場合は、書き換え可能にするためにフリーストックから取得する。
        /// グリッドエリア上のグリッドをそのグリッドに置き換え、そのポインタを返す。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public CubeGrid32x32x32UnsafePtr GetGrid
            (
                ref this (DynamicBuffer<CubeGridGlobal.DefualtGridData>, CubeGridArea.BufferData, CubeGridArea.InfoTempData) x,
                int ix, int iy, int iz
            )
        {
            ref var defaultGrids = ref x.Item1;
            ref var grids = ref x.Item2;
            ref var areaInfo = ref x.Item3;


            var area = (grids, areaInfo);
            var gridptr = area.getGridFromArea(ix, iy, iz);

            if (!defaultGrids.isDefault(*gridptr.p)) return gridptr;




            return gridptr;
        }

        /// <summary>
        /// 指定したグリッドをチェックする。ソリッドかブランクだった場合、フリーストックエリアに戻す。
        /// 入れ替えで、デフォルトグリッドを取得し、グリッドに格納する。
        /// また、参照であるため、グリッドエリア上のグリッドも同時に書き換えられる。
        /// もともとデフォルトだったり、ソリッドでもブランクでもない場合は何もしない。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void BackGridIfFilled
            (
                ref this (DynamicBuffer<CubeGridGlobal.DefualtGridData>, DynamicBuffer<CubeGridGlobal.FreeGridStockData>) x,
                ref CubeGrid32x32x32UnsafePtr gridptr
            )
        {
            ref var defaultGrids = ref x.Item1;
            ref var stocks = ref x.Item2;


            if (defaultGrids.isDefault(*gridptr.p)) return;

            if (!gridptr.p->IsFullOrEmpty) return;


            var fillMode = gridptr.p->getFillMode();

            stocks.BackToFreeGridStocks(fillMode, *gridptr.p);

            *gridptr.p = defaultGrids.GetDefaultGrid(fillMode);
        }
    }

}
