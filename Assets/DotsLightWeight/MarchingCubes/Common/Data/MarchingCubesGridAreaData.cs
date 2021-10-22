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


        public unsafe struct LinkToGridData : IComponentData, IDisposable
        {
            public int* pGridIds;
            public uint** ppGridXLines;
            public int3 GridLength;
            public int3 GridSpan;
            public int nextSeed;

            public void Dispose()
            {
                Debug.Log("Link to grid data dispos");
                if (this.pGridIds != null) UnsafeUtility.Free(this.pGridIds, Allocator.Persistent);
                if (this.ppGridXLines != null) UnsafeUtility.Free(this.ppGridXLines, Allocator.Persistent);
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



        //public static unsafe NearGridIndex PickupNearGridIds(int* pGridIds, int3 gridSpan, DotGrid.GridIndex index)
        //{
        //    var ids = new NearGridIndex();


        //    var lhome = index;
        //    ids.left.home = pGridIds[lhome.serial];

        //    var lrear = index.CloneNear(new int3(0, 0, 1), gridSpan);
        //    ids.left.rear = pGridIds[lrear.serial];

        //    var ldown = index.CloneNear(new int3(0, 1, 0), gridSpan);
        //    ids.left.down = pGridIds[ldown.serial];

        //    var lslant = index.CloneNear(new int3(0, 1, 1), gridSpan);
        //    ids.left.slant = pGridIds[lslant.serial];


        //    var rhome = index.CloneNear(new int3(1, 0, 0), gridSpan);
        //    ids.right.home = pGridIds[rhome.serial];

        //    var rrear = index.CloneNear(new int3(1, 0, 1), gridSpan);
        //    ids.right.rear = pGridIds[rrear.serial];

        //    var rdown = index.CloneNear(new int3(1, 1, 0), gridSpan);
        //    ids.right.down = pGridIds[rdown.serial];

        //    var rslant = index.CloneNear(new int3(1, 1, 1), gridSpan);
        //    ids.right.slant = pGridIds[rslant.serial];


        //    return ids;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MakeCube.NearDotGrids PickupNearGridIds(
            in this LinkToGridData grids, DotGrid.UnitData grid, uint* pBlankGrid)
        {
            
            var ppXLines = grids.ppGridXLines;
            var gridSpan = grids.GridSpan;
            var index = grid.GridIndexInArea;


            var lhome = index;
            var plx = ppXLines[lhome.serial];

            var lrear = index.CloneNear(new int3(0, 0, 1), gridSpan);
            var plz = ppXLines[lrear.serial];

            var ldown = index.CloneNear(new int3(0, 1, 0), gridSpan);
            var ply = ppXLines[ldown.serial];

            var lslant = index.CloneNear(new int3(0, 1, 1), gridSpan);
            var plw = ppXLines[lslant.serial];


            var rhome = index.CloneNear(new int3(1, 0, 0), gridSpan);
            var prx = ppXLines[rhome.serial];

            var rrear = index.CloneNear(new int3(1, 0, 1), gridSpan);
            var prz = ppXLines[rrear.serial];

            var rdown = index.CloneNear(new int3(1, 1, 0), gridSpan);
            var pry = ppXLines[rdown.serial];

            var rslant = index.CloneNear(new int3(1, 1, 1), gridSpan);
            var prw = ppXLines[rslant.serial];


            return new MakeCube.NearDotGrids
            {
                L = new MakeCube.NearDotGrids.HalfGridUnit
                {
                    isContained = (uint4)new bool4(plx != null, ply != null, plz != null, plw != null),
                    x = or_(plx, pBlankGrid),
                    y = or_(ply, pBlankGrid),
                    z = or_(plz, pBlankGrid),
                    w = or_(plw, pBlankGrid),
                },
                R = new MakeCube.NearDotGrids.HalfGridUnit
                {
                    isContained = (uint4)new bool4(prx != null, pry != null, prz != null, prw != null),
                    x = or_(prx, pBlankGrid),
                    y = or_(pry, pBlankGrid),
                    z = or_(prz, pBlankGrid),
                    w = or_(prw, pBlankGrid),
                },
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe static uint* or_(uint* p, uint* pBlank) => p != null ? p : pBlank;
    }





}

