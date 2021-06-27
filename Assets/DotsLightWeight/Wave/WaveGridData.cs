using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.WaveGrid
{
    public class WaveGridMasterData : IComponentData, IDisposable
    {
        public NativeArray<WaveGridPrevPoint> PrevUnits;
        public NativeArray<WaveGridNextPoint> NextUnits;
        public int2 UnitLengthInGrid;
        public int2 NumGrids;
        public float UnitScale;

        public void Dispose()
        {
            this.PrevUnits.Dispose();
            this.NextUnits.Dispose();
            Debug.Log("disp");
        }
    }
    public struct WaveGridPrevPoint
    {
        public float Curr;
        public float Prev;
    }
    public struct WaveGridNextPoint
    {
        public float Next;
    }

    public struct WaveGridData : IComponentData
    {
        public int LodLevel;
        public int2 GridId;
        public float UnitScaleOnLod;
    }

    public static partial class WaveGridUtility
    {
        public static int ToLinear(this WaveGridData grid, WaveGridMasterData master)
        {
            var wspan = master.UnitLengthInGrid.x * master.NumGrids.x;
            var i = grid.GridId;// >> grid.LodLevel;
            return i.x + i.y * wspan;
        }
    }
}