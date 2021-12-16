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
using Unity.Burst.Intrinsics;

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
        public unsafe class HeightFieldShaderResourceData : IComponentData
        {
            public HeightFieldBuffer Heights;

            public void Alloc(int2 numGrids, int2 unitLengthInGrid)
            {
                var length = numGrids * (unitLengthInGrid + 1);
                this.Heights = HeightFieldBuffer.Create(length.x * length.y);
            }
            public void Dispose()
            {
                this.Heights.Dispose();
                Debug.Log("height resouce disposed");
            }
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

            public int ToGridIndex(int2 gridLocation) =>
                gridLocation.y * this.UnitLengthInGrid.y * this.TotalLength.x + gridLocation.x * this.UnitLengthInGrid.x;
        }

        public struct Emitting : IComponentData
        {
            public Entity SplashPrefab;
        }

        public struct InitializeTag : IComponentData
        { }
    }


    //public struct HeightFieldBufferTexture : IDisposable
    //{
    //    public Texture2D Texture { get; private set; }

    //    public static HeightFieldBufferTexture Create(int2 length)
    //    {
    //        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat;
    //        var flags = UnityEngine.Experimental.Rendering.TextureCreationFlags.None;
    //        var buffer = new Texture2D(length.x, length.y, format, flags);
    //        buffer.enableRandomWrite = true;
    //        buffer.set
    //        buffer.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
    //        buffer.Create();

    //        return new HeightFieldBufferTexture
    //        {
    //            Texture = buffer,
    //        };
    //    }
    //    public void Dispose()
    //    {
    //        this.Texture?.Release();
    //        this.Texture = null;
    //    }
    //}
    public struct HeightFieldBuffer : IDisposable
    {
        public GraphicsBuffer Buffer { get; private set; }

        public static HeightFieldBuffer Create(int2 length) => new HeightFieldBuffer
        {
            //Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, length.x * length.y, sizeof(float))
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (length.x+1) * (length.y+1), sizeof(float))
        };
        public void Dispose()
        {
            this.Buffer?.Release();
            this.Buffer = null;
        }
    }

    static class InitUtility
    {
        // 暫定（直接地形データを渡すよりよい方法ないか？）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitResourceData(this GridMaster.HeightFieldShaderResourceData res, TerrainData data)
        {
            var length = data.heightmapResolution - 1;
            var terrainHeights = data.GetHeights(0, 0, length, length);
            var flatten = new float[terrainHeights.Length];

            var i = 0;
            foreach (var f in terrainHeights) flatten[i++] = f * data.heightmapScale.y;
            
            var heights = HeightFieldBuffer.Create(length);
            heights.Buffer.SetData(flatten, 0, 0, terrainHeights.Length);
            res.Heights = heights;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void InitHeightBuffer(this GridMaster.HeightFieldData heights, TerrainData data)
        {
            var length = data.heightmapResolution - 1;
            var terrainHeights = data.GetHeights(0, 0, length, length);

            var i = 0;
            foreach (var f in terrainHeights) heights.p[i++] = f * data.heightmapScale.y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyResourceFrom(
            this GridMaster.HeightFieldShaderResourceData res, GridMaster.HeightFieldData heights, GridMaster.DimensionData dim,
            Height.GridData grid, int2 begin, int2 end)
        {
            //var length = (end.y - begin.y) * dim.UnitLengthInGrid.x - begin.x + end.x + 1;
            var l2 = end - begin;
            var length = l2.y * dim.UnitLengthInGrid.x + l2.x + 1;

            var buf = new NativeArray<float>((l2.y + 1) * dim.UnitLengthInGrid.x, Allocator.Temp);

            //var srcstride = dim.TotalLength.x * sizeof(float);
            //var dststride = dim.UnitLengthInGrid.x;
            //var pSrc = heights.p + begin.y * srcstride;
            //var pDst = buf.GetUnsafePtr();
            //UnsafeUtility.MemCpyStride(pDst, dststride, pSrc, srcstride, dststride, l2.y + 1);
            var srcstride = dim.TotalLength.x;
            var dststride = dim.UnitLengthInGrid.x;
            var pSrc = heights.p + begin.y * srcstride;
            var pDst = (float*)buf.GetUnsafePtr();
            copy_(pSrc, srcstride, pDst, dststride, l2);

            var startIndex = dim.ToGridIndex(grid.GridId);
            var beginIndex = startIndex + begin.y * dim.TotalLength.x + begin.x;
            res.Heights.Buffer.SetData(buf, begin.x, beginIndex, length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void copy_(float *pSrc, int srcStride, float *pDst, int dstStride, int2 l2)
        {
            if (X86.Avx2.IsAvx2Supported)
            {
                var pDst_ = (v256*)pDst;
                var pSrc_ = (v256*)pSrc;
                var dstStride_ = dstStride >> 4;
                var srcStride_ = srcStride >> 4;
                var i = 0;
                for (var iy = 0; iy < l2.y + 1; iy++)
                {
                    for (var ix = 0; ix < dstStride_; ix++)
                    {
                        pDst_[ix] = pSrc_[ix];
                    }
                    pDst_ += dstStride_;
                    pSrc_ += srcStride_;
                }
            }
            else
            {
                var pDst_ = (float4*)pDst;
                var pSrc_ = (float4*)pSrc;
                var dstStride_ = dstStride >> 2;
                var srcStride_ = srcStride >> 2;
                var i = 0;
                for (var iy = 0; iy < l2.y + 1; iy++)
                {
                    for (var ix = 0; ix < dstStride_; ix++)
                    {
                        pDst_[ix] = pSrc_[ix];
                    }
                    pDst_ += dstStride_;
                    pSrc_ += srcStride_;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetResourcesTo(this GridMaster.HeightFieldShaderResourceData res, Material mat, GridMaster.DimensionData dim)
        {
            mat.SetBuffer("Heights", res.Heights.Buffer);

            var lengthInGrid = dim.UnitLengthInGrid;
            var widthSpan = dim.TotalLength.x;
            var scale = dim.UnitScale;
            var value = new float4(math.asfloat(lengthInGrid), math.asfloat(widthSpan), scale);
            mat.SetVector("DimInfo", value);
        }
    }
}