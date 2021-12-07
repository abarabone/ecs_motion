using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.HeightGrid
{
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Common;
    using DotsLite.Utilities;


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

    public static class GridMaster
    {
        public unsafe struct HeightFieldData : IComponentData//, IDisposable
        {
            public float *pNexts;
            public float *pCurrs;
            public float *pPrevs;

            public void SwapShift()
            {
                this.pPrevs = this.pCurrs;
                this.pCurrs = this.pNexts;
                this.pNexts = this.pPrevs;
            }

            public void Alloc(int ww, int wh, int lw, int lh)
            {
                var totalLength = ww * lw * wh * lh + wh * lh;// 最後に１ライン余分に加え、ループ用にコピーエリアとする
                this.pNexts = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 4, Allocator.Persistent);
                this.pCurrs = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 4, Allocator.Persistent);
                this.pPrevs = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 4, Allocator.Persistent);
            }
            public void Dispose()
            {
                UnsafeUtility.Free(this.pNexts, Allocator.Persistent);
                UnsafeUtility.Free(this.pCurrs, Allocator.Persistent);
                UnsafeUtility.Free(this.pPrevs, Allocator.Persistent);
                Debug.Log("disposed");
            }
        }
        public class Data : IComponentData, IDisposable
        {
            public NativeArray<float> Nexts;
            public NativeArray<float> Currs;
            public NativeArray<float> Prevs;

            public DimensionData Info;

            public void Dispose()
            {
                this.Nexts.Dispose();
                this.Currs.Dispose();
                this.Prevs.Dispose();
                Debug.Log("disposed");
            }
        }

        public struct DimensionData : IComponentData
        {
            public float3 LeftTopLocation;
            public int2 UnitLengthInGrid;
            public int2 NumGrids;
            public float UnitScale;
            public float Dumping;
            public float Constraint2;

            public int2 TotalLength;
            public float UnitScaleRcp;
        }

        public struct Emitting : IComponentData
        {
            public Entity SplashPrefab;
        }
    }



}