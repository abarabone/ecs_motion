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
            public float* p;

            public void Alloc(int2 numGrids, int2 unitLengthInGrid)
            {
                var ww = numGrids.x;
                var wh = numGrids.y;
                var lw = unitLengthInGrid.x;
                var lh = unitLengthInGrid.y;
                var totalLength = ww * lw * wh * lh + wh * lh;// 最後に１ライン余分に加え、ループ用にコピーエリアとする

                this.p = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 16, Allocator.Persistent);

                UnsafeUtility.MemClear(this.p, totalLength * sizeof(float));
            }
            public void Dispose()
            {
                UnsafeUtility.Free(this.p, Allocator.Persistent);
                Debug.Log("height disposed");
            }
        }

        /// <summary>
        /// ハイトフィールドの場合、高さデータが変化しない時間のほうが多いので、
        /// ＧＰＵに高さデータを送っておく
        /// </summary>
        public unsafe struct HeightFieldShaderResourceData : IComponentData
        {

        }

        /// <summary>
        /// 波の場合、毎フレームデータが変化する。
        /// そのため、毎フレーム transform データとともにバッファに送る。
        /// </summary>
        public unsafe struct WaveFieldData : IComponentData//, IDisposable
        {
            public float *pNexts;
            public float *pPrevs;

            public void SwapShiftBuffers(ref HeightFieldData heights)
            {
                var pCurrs = heights.p;
                heights.p = this.pNexts;
                this.pNexts = this.pPrevs;
                this.pPrevs = pCurrs;
            }

            public void Alloc(int2 numGrids, int2 unitLengthInGrid)
            {
                var ww = numGrids.x;
                var wh = numGrids.y;
                var lw = unitLengthInGrid.x;
                var lh = unitLengthInGrid.y;
                var totalLength = ww * lw * wh * lh + wh * lh;// 最後に１ライン余分に加え、ループ用にコピーエリアとする

                this.pNexts = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 16, Allocator.Persistent);
                this.pPrevs = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 16, Allocator.Persistent);

                UnsafeUtility.MemClear(this.pNexts, totalLength * sizeof(float));
                UnsafeUtility.MemClear(this.pPrevs, totalLength * sizeof(float));
            }
            public void Dispose()
            {
                UnsafeUtility.Free(this.pNexts, Allocator.Persistent);
                UnsafeUtility.Free(this.pPrevs, Allocator.Persistent);
                Debug.Log("wave disposed");
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

        public struct InitializeTag : IComponentData
        { }
    }



}