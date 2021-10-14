using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes
{

    using DotsLite.Draw;
    using DotsLite.Utilities;



    static public partial class DotGridArea
    {

        //public struct InitializeData : IComponentData
        //{
        //    public GridFillMode FillMode;
        //}


        public unsafe struct LinkToGridData : IComponentData//, IDisposable
        {
            public int* pGridIds;
            public int3 GridLength;
            public int3 GridSpan;
            public int nextSeed;

            public void Dispose()
            {
                Debug.Log("Link to grid data disposed");
                UnsafeUtility.Free(this.pGridIds, Allocator.Persistent);
            }
        }

        public struct DotGridPrefabData : IComponentData
        {
            public Entity Prefab;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            //public int3 GridWholeLength;
        }
        public struct InfoWorkData : IComponentData
        {
            public int3 GridSpan;
        }



        static unsafe NearGridIndex PickupNearGridIds(int* pGridIds, int3 gridSpan, DotGrid.GridIndex index)
        {
            var ids = new NearGridIndex();


            var lhome = index;
            ids.left.home = pGridIds[lhome.serial];

            var lrear = new DotGrid.GridIndex().Set(index.index + new int3(0, 0, 1), gridSpan);
            ids.left.rear = pGridIds[lrear.serial];

            var ldown = new DotGrid.GridIndex().Set(index.index + new int3(0, 1, 0), gridSpan);
            ids.left.down = pGridIds[ldown.serial];

            var lslant = new DotGrid.GridIndex().Set(index.index + new int3(0, 1, 1), gridSpan);
            ids.left.slant = pGridIds[lslant.serial];


            var rhome = new DotGrid.GridIndex().Set(index.index + new int3(1, 0, 0), gridSpan);
            ids.right.home = pGridIds[rhome.serial];

            var rrear = new DotGrid.GridIndex().Set(index.index + new int3(1, 0, 1), gridSpan);
            ids.right.rear = pGridIds[rrear.serial];

            var rdown = new DotGrid.GridIndex().Set(index.index + new int3(1, 1, 0), gridSpan);
            ids.right.down = pGridIds[rdown.serial];

            var rslant = new DotGrid.GridIndex().Set(index.index + new int3(1, 1, 1), gridSpan);
            ids.right.slant = pGridIds[rslant.serial];


            return ids;
        }
    }





}

