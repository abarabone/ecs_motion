using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    static public class GridUtility
    {


        public struct AdjacentGrids
        {
            public int gridId;
            public HalfGridUnit L;
            public HalfGridUnit R;

            public struct HalfGridUnit
            {
                public CubeGrid32x32x32UnsafePtr x;
                public CubeGrid32x32x32UnsafePtr y;
                public CubeGrid32x32x32UnsafePtr z;
                public CubeGrid32x32x32UnsafePtr w;
            }
        }


        static unsafe CubeGrid32x32x32UnsafePtr toPtr(in this CubeGridArea.BufferData gridArea, int i) =>
            new CubeGrid32x32x32UnsafePtr
            {
                p = gridArea.Grids.Ptr + i,
            };



        /// <summary>
        /// 
        /// </summary>
        static public unsafe AdjacentGrids getGridSet_
            (in this CubeGridArea.BufferData gridArea, int ix, int iy, int iz, int3 gridSpan) =>
            gridArea.getGridSet_(new int3(ix, iy, iz), gridSpan);

        static public unsafe AdjacentGrids getGridSet_
            (in this CubeGridArea.BufferData gridArea, int3 index, int3 gridSpan)
        {
            var i = math.dot(index, gridSpan);

            return new AdjacentGrids
            {
                L =
                {
                    x = gridArea.toPtr( i + 0 ),
                    y = gridArea.toPtr( i + gridSpan.y + 0 ),
                    z = gridArea.toPtr( i + gridSpan.z + 0 ),
                    w = gridArea.toPtr( i + gridSpan.y + gridSpan.z + 0 ),
                },
                R =
                {
                    x = gridArea.toPtr( i + 1 ),
                    y = gridArea.toPtr( i + gridSpan.y + 1 ),
                    z = gridArea.toPtr( i + gridSpan.z + 1 ),
                    w = gridArea.toPtr( i + gridSpan.y + gridSpan.z + 1 ),
                },
            };
        }

        public struct GridCounts
        {
            public int4 L, R;
        }
        static public unsafe GridCounts getEachCount(in this AdjacentGrids g)
        {
            var gridCount = new int4
            (
                g.L.x.p->CubeCount,
                g.L.y.p->CubeCount,
                g.L.z.p->CubeCount,
                g.L.w.p->CubeCount
            );
            var gridCount_right = new int4
            (
                g.R.x.p->CubeCount,
                g.R.y.p->CubeCount,
                g.R.z.p->CubeCount,
                g.R.w.p->CubeCount
            );
            return new GridCounts { L = gridCount, R = gridCount_right };
        }


        static public bool isNeedDraw_(int4 gridCount, int4 gridCount_right)
        {
            var addvalue = gridCount + gridCount_right;
            var isZero = !math.any(addvalue);
            var isFull = math.all(addvalue == 0x8000 << 1);
            return !(isZero | isFull);
        }

        /// <summary>
        /// グリッドとその右グリッドが、同じフィルなら描画の必要はない。
        /// </summary>
        static public bool isNeedDraw_(in this GridCounts gridCounts)
        {
            var addvalue = gridCounts.L + gridCounts.R;
            var isBlank = !math.any(addvalue);
            var isSolid = math.all(addvalue == 0x8000 << 1);
            return !(isBlank | isSolid);
        }


    }
}