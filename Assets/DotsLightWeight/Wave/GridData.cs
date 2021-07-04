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

    [Serializable]
    public struct GridBinaryLength2
    {
        public binary_length_grid u;
        public binary_length_grid v;
        public int x { get => (int)this.u; set => this.u = (binary_length_grid)value; }
        public int y { get => (int)this.v; set => this.v = (binary_length_grid)value; }
        public static implicit operator int2(GridBinaryLength2 src) => new int2((int)src.u, (int)src.v);
    }
    public enum binary_length_grid
    {
        //length_1 = 1,
        //length_2 = 2,
        //length_4 = 4,
        length_8 = 8,
        length_16 = 16,
        length_32 = 32,
        length_64 = 64,
        length_128 = 128,
        length_256 = 256,
    }

    public static class Wave
    {
        public class GridMasterData : IComponentData, IDisposable
        {
            public NativeArray<float> Nexts;
            public NativeArray<float> Currs;
            public NativeArray<float> Prevs;

            public GridMasterInfo Info;

            public void Dispose()
            {
                this.Nexts.Dispose();
                this.Currs.Dispose();
                this.Prevs.Dispose();
                Debug.Log("disposed");
            }
        }
        public struct GridMasterInfo
        {
            public float3 LeftTopPosition;
            public int2 UnitLengthInGrid;
            public int2 NumGrids;
            public float UnitScale;
            public float Dumping;
            public float Constraint2;

            public int2 TotalLength;
            public float UnitScaleRcp;
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
        //public static int ToLinear(this Height.GridData grid, Wave.GridMasterData master)
        //{
        //    var wspan = master.UnitLengthInGrid.x * master.NumGrids.x;
        //    var i = grid.GridId;// >> grid.LodLevel;
        //    return i.x + i.y * wspan;
        //}

        //public static int2 CalcHitIndex(this Wave.GridMasterData master, float2 point)
        //{

        //    master.LeftTopPosition.

        //    return int2.zero;
        //}

        //public static int2 CalcHitIndex(this Wave.GridMasterData master, float3 point, float3 dir, float length)
        //{



        //    return int2.zero;
        //}

        public static unsafe float CalcWaveHeight(this Wave.GridMasterInfo info, float* pHeight, float2 point)
        {
            var xy = point - info.LeftTopPosition.xz;
            var i = xy * info.UnitScaleRcp;

            var index2 = (int2)(xy * info.UnitScaleRcp);

            var serialIndex = index2.x + index2.y * info.TotalLength.x;

            var i0 = serialIndex + 0;
            var i1 = serialIndex + 1;
            var i2 = serialIndex + index2.x + 0;
            var i3 = serialIndex + index2.x + 1;

            var h00 = pHeight[i0];
            var h01 = pHeight[i1];
            var h02 = pHeight[i2];

            var h10 = pHeight[i2];
            var h11 = pHeight[i1];
            var h12 = pHeight[i3];


        }
        
    }

    public struct RightTriangle
    {
        public float3 BasePoint;
        public float2 UvLengthRcp;

        //public float3 GravityHit(float3 p, float length)
        //{

        //    var uv = p.xz - this.BasePoint.xz;

        //    var nmuv = uv * this.UvLengthRcp;

        //    var isHit = 1.0f >= nmuv.x + nmuv.y;
        //}
        //public float3 RaycastHit(float3 p, float3 dir, float length)
        //{

            

        //}
    }
}