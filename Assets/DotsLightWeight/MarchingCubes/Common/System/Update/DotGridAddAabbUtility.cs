using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Physics;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotsLite.MarchingCubes
{
    public static partial class DotgridUpdateUtility
    {


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Add<TGrid>(this AABB range,
            in DotGridArea.LinkToGridData grids, in DotGridArea.UnitDimensionData dim)
            where TGrid : struct, IDotGrid<TGrid>
        {
            var ppXLines = grids.ppGridXLines;
            var pPoolIds = grids.pGridPoolIds;
            var span = grids.GridSpan;
            var begin = int3.zero;
            var end = grids.GridLength - 1;

            var fst = range.Min - dim.LeftFrontTop.xyz;
            var lst = range.Max - dim.LeftFrontTop.xyz;
            
            var _igfst = (int3)(fst * dim.GridScaleR.xyz);
            var _iglst = (int3)(lst * dim.GridScaleR.xyz);
            var igfst = math.min(math.max(_igfst, begin), end);
            var iglst = math.min(math.max(_iglst, begin), end);

            var _ifst = (int3)(fst * dim.UnitScaleR.xyz);
            var _ilst = (int3)(lst * dim.UnitScaleR.xyz);

            for (var iy = igfst.y; iy <= iglst.y; iy++)
                for (var iz = igfst.z; iz <= iglst.z; iz++)
                    for (var ix = igfst.x; ix <= iglst.x; ix++)
                    {
                        var i = new int3(ix, iy, iz);
                        var gidx = new DotGrid<TGrid>.GridIndex().Set(i, span);
                        var pXlines = ppXLines[gidx.serial];
                        add_grid_inner_(range, _ifst, _ilst, pXlines, in dim, gidx);
                    }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static void add_grid_inner_<TGrid>(this AABB range, int3 _ifst, int3 _ilst,
            uint* pXlines, in DotGridArea.UnitDimensionData dim, DotGrid<TGrid>.GridIndex gidx)
            where TGrid : struct, IDotGrid<TGrid>
        {
            var begin = int3.zero;
            var end = new int3(32 - 1, 32 -1, 32 -1);
            var igrid = gidx.index * 32;

            var ifst = math.min(math.max(_ifst - igrid, begin), end);
            var ilst = math.min(math.max(_ilst - igrid, begin), end);

            var x = (uint)math.ceilpow2((ilst.x - ifst.x + 1)) << ifst.x;

            for (var iy = ifst.y; iy <= ilst.y; iy++)
                for (var iz = ifst.z; iz <= ilst.z; iz++)
                {
                    var i = iz + iy * 32;
                    pXlines[i] |= x;
                }
        }
    }
}