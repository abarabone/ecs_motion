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

namespace DotsLite.MarchingCubes.old
{

    static public unsafe class DotGridExtension
    {

        static public ref DotGrid32x32x32Unsafe AsRef(this DotGrid32x32x32UnsafePtr cubePtr) => ref *cubePtr.p;

        static public ref DotGrid32x32x32Unsafe AsRef(this UnsafeList<DotGrid32x32x32Unsafe> list, int index) => ref *(list.Ptr + index);

        static public ref T AsRef<T>(this UnsafeList list, int index) where T : unmanaged => ref *((T*)list.Ptr + index);



        //static public Cubee With(ref this DotGridArrayUnsafe grids, ref DotGridGlobal global, )
        //{

        //}

        //static public void a(ref this DotGridArrayUnsafe arr, ref DotGridGlobalData globalData)
        //{

        //    var _0or1 = math.sign(grid.CubeCount);
        //    var defaultGrid = globalData.GetDefaultGrid((GridFillMode)_0or1);


        //}



        static public bool IsDefault
            (ref this NativeArray<DotGrid32x32x32Unsafe> defaultGrids, DotGrid32x32x32Unsafe grid)
        {
            return grid.pUnits == defaultGrids[(int)grid.FillModeBlankOrSolid].pUnits;
        }

        /// <summary>
        /// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        /// 取得したグリッドポインタからは、グリッドエリア上のグリッドそのものを書き換えることができる。
        /// また、取得すべきグリッドがデフォルトだった場合は、書き換え可能にするためにフリーストックから取得し、
        /// グリッドエリア上のグリッドを取得したグリッドで置き換える。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public DotGrid32x32x32UnsafePtr GetGrid
            (
                ref NativeArray<DotGrid32x32x32Unsafe> defaultGrids,
                ref FreeStockList freeStocks,
                ref DotGridArea.BufferData grids,
                ref DotGridArea.InfoWorkData areaInfo,
                int ix, int iy, int iz
            )
        {
            var gridptr = DotGridArea.GetGridFromArea(ref grids, ref areaInfo, ix, iy, iz);

            if (defaultGrids.IsDefault(*gridptr.p))
            {
                var fillMode = gridptr.p->FillModeBlankOrSolid;
                gridptr.p->pUnits = freeStocks.Rent(fillMode).pUnits;
            }

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
                ref NativeArray<DotGrid32x32x32Unsafe> defaultGrids,
                ref FreeStockList stocks,
                ref DotGrid32x32x32UnsafePtr gridptr
            )
        {
            if (defaultGrids.IsDefault(*gridptr.p)) return;

            if (!gridptr.p->IsFullOrEmpty) return;


            var fillMode = gridptr.p->FillModeBlankOrSolid;
            stocks.Back(*gridptr.p, fillMode);
            gridptr.p->pUnits = defaultGrids[(int)fillMode].pUnits;
        }



    }

}
