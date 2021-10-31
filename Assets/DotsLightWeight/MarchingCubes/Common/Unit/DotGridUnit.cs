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

    public unsafe interface IDotGrid<TGrid> : IDisposable
        where TGrid : struct, IDotGrid<TGrid>
    {
        uint* pXline { get; }
        //int CubeCount { get; }

        int UnitOnEdge { get; }

        TGrid Alloc(GridFillMode fillmode);

        //TGrid CreateDefault(GridFillMode fillmode);

        void Fill();

        void Copy(in TGrid grid, in DotGrid.IndexData index, in DotGrid.UpdateDirtyRangeData dirty,
            in DotGridArea.LinkToGridData area, in DotGridArea.ResourceGpuModeData res);
    }



    public enum GridFillMode
    {
        NotFilled = -1,
        Blank = 0,
        Solid = 1,
        Null = 2,
    };


}
