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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Add(
            in this DotGridArea.LinkToGridData grids, AABB range)
        {
            var ppXLines = grids.ppGridXLines;
            var pPoolIds = grids.pGridPoolIds;
            var gridSpan = grids.GridSpan;


            var len = grids.GridLength;
            //grids.
        }
    }
}