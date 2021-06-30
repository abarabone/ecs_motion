using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.HeightGrid
{

    public static class Wave
    {
        public class GridMasterData : IComponentData, IDisposable
        {
            public NativeArray<float> Nexts;
            public NativeArray<float> Currs;
            public NativeArray<float> Prevs;

            public int2 UnitLengthInGrid;
            public int2 NumGrids;
            public float UnitScale;

            public void Dispose()
            {
                this.Nexts.Dispose();
                this.Currs.Dispose();
                this.Prevs.Dispose();
                Debug.Log("disposed");
            }
        }
    }

    public static class Height
    {
        public struct GridLevel0Tag : IComponentData
        { }

        public struct GridData : IComponentData
        {
            public int LodLevel;
            public int2 GridId;
            public float UnitScaleOnLod;
        }

        public struct BoundingBox : IComponentData
        {
            public AABB WorldBbox;
        }
    }


    public static partial class HeightGridUtility
    {
        public static int ToLinear(this Height.GridData grid, Wave.GridMasterData master)
        {
            var wspan = master.UnitLengthInGrid.x * master.NumGrids.x;
            var i = grid.GridId;// >> grid.LodLevel;
            return i.x + i.y * wspan;
        }
    }
}