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

        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }

        public unsafe struct LinkToGridData : IComponentData//, IDisposable
        {
            public int* pGridPoolIds;
            public uint** ppGridXLines;
            public int3 GridLength;
            public int3 GridSpan;
            public int nextSeed;

            public void Dispose()
            {
                Debug.Log("Link to grid data dispos");
                if (this.pGridPoolIds != null) UnsafeUtility.Free(this.pGridPoolIds, Allocator.Persistent);
                if (this.ppGridXLines != null) UnsafeUtility.Free(this.ppGridXLines, Allocator.Persistent);
                this.pGridPoolIds = null;
                this.ppGridXLines = null;
            }
        }

        public struct UnitDimensionData : IComponentData
        {
            public float4 LeftFrontTop;
            public float4 GridScaleR;
            public float4 UnitScaleR;
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
        public static unsafe MakeCube.NearDotGrids PickupNearGridIds<TGrid>
            (in this LinkToGridData gridlinks, in TGrid grid, in DotGrid.IndexData gridindex)
            where TGrid : struct, IDotGrid<TGrid>
        {
            
            var ppXLines = gridlinks.ppGridXLines;
            var pPoolIds = gridlinks.pGridPoolIds;
            var span = gridlinks.GridSpan;
            var index = gridindex.GridIndexInArea;


            var lhome = index;
            var plx = ppXLines[lhome.serial];
            var ilx = pPoolIds[lhome.serial];

            var lrear = index.CloneNear(new int3(0, 0, 1), span);
            var plz = ppXLines[lrear.serial];
            var ilz = pPoolIds[lrear.serial];

            var ldown = index.CloneNear(new int3(0, 1, 0), span);
            var ply = ppXLines[ldown.serial];
            var ily = pPoolIds[ldown.serial];

            var lslant = index.CloneNear(new int3(0, 1, 1), span);
            var plw = ppXLines[lslant.serial];
            var ilw = pPoolIds[lslant.serial];


            var rhome = index.CloneNear(new int3(1, 0, 0), span);
            var prx = ppXLines[rhome.serial];
            var irx = pPoolIds[rhome.serial];

            var rrear = index.CloneNear(new int3(1, 0, 1), span);
            var prz = ppXLines[rrear.serial];
            var irz = pPoolIds[rrear.serial];

            var rdown = index.CloneNear(new int3(1, 1, 0), span);
            var pry = ppXLines[rdown.serial];
            var iry = pPoolIds[rdown.serial];

            var rslant = index.CloneNear(new int3(1, 1, 1), span);
            var prw = ppXLines[rslant.serial];
            var irw = pPoolIds[rslant.serial];


            return new MakeCube.NearDotGrids
            {
                L = new MakeCube.NearDotGrids.HalfGridUnit
                {
                    isContained = (uint4)math.sign(new int4(ilx, ily, ilz, ilw) + 1),
                    x = plx, y = ply, z = plz, w = plw,
                },
                R = new MakeCube.NearDotGrids.HalfGridUnit
                {
                    isContained = (uint4)math.sign(new int4(irx, iry, irz, irw) + 1),
                    x = prx, y = pry, z = prz, w = prw,
                },
            };
        }
    }





}

