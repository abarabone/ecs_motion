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
using Unity.Burst;
using Unity.Physics;

using Collider = Unity.Physics.Collider;
using TerrainCollider = Unity.Physics.TerrainCollider;
using Material = UnityEngine.Material;

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
                var totalLength = ww * lw * wh * lh + wh * lh;// �Ō�ɂP���C���]���ɉ����A���[�v�p�ɃR�s�[�G���A�Ƃ���
                Debug.Log($"heights {ww},{wh} {lw},{lh} {totalLength}");

                this.p = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 32, Allocator.Persistent);

                UnsafeUtility.MemClear(this.p, totalLength * sizeof(float));
            }
            public void Dispose()
            {
                UnsafeUtility.Free(this.p, Allocator.Persistent);
                Debug.Log("height disposed");
            }
        }

        /// <summary>
        /// �n�C�g�t�B�[���h�̏ꍇ�A�����f�[�^���ω����Ȃ����Ԃ̂ق��������̂ŁA
        /// �f�o�t�ɍ����f�[�^�𑗂��Ă���
        /// </summary>
        public unsafe class HeightFieldShaderResourceData : IComponentData
        {
            public HeightFieldBuffer Heights;

            public void Alloc(int2 numGrids, int2 unitLengthInGrid)
            {
                var length = numGrids * (unitLengthInGrid + 1);
                Debug.Log(length);
                this.Heights = HeightFieldBuffer.Create(length);
                Debug.Log($"{length} {length.x * length.y}");
            }
            public void Dispose()
            {
                this.Heights.Dispose();
                Debug.Log("height resouce disposed");
            }
        }

        /// <summary>
        /// �g�̏ꍇ�A���t���[���f�[�^���ω�����B
        /// ���̂��߁A���t���[�� transform �f�[�^�ƂƂ��Ƀo�b�t�@�ɑ���B
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
                var totalLength = ww * lw * wh * lh + wh * lh;// �Ō�ɂP���C���]���ɉ����A���[�v�p�ɃR�s�[�G���A�Ƃ���

                this.pNexts = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 32, Allocator.Persistent);
                this.pPrevs = (float*)UnsafeUtility.Malloc(totalLength * sizeof(float), 32, Allocator.Persistent);

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

            //public int ToGridIndex(int2 gridLocation) =>
            //    gridLocation.y * this.UnitLengthInGrid.y * this.TotalLength.x + gridLocation.x * this.UnitLengthInGrid.x;
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
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, length.x * length.y, sizeof(float))
            //Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (length.x+1) * (length.y+1), sizeof(float))
        };
        public void Dispose()
        {
            this.Buffer?.Release();
            this.Buffer = null;
        }
    }

    // �n�`�͂��قǕω����Ȃ��̂ŁA�f�o�t�Ƀo�b�t�@�������ق����悳����
    // �g�͖��t���[���S���������Ȃ̂ŁA�O���b�h���Ƃɑ���΂悢�Ǝv���@���������̊Ԉ���������Ȃ�ʂ����ǁc

    // �f�o�t�o�b�t�@�l��������
    // �E�t���b�g�@�@�@�c �����Ȃ��@���ʂȂf�o�t�]���@�����܂����ƘA���̈悶��Ȃ��Ȃ�
    // �E�O���b�h���Ɓ@�c �����̂����@�ʒu�̌v�Z���ʓ|�@�k�n�c�ŕs�����Ǝv�������ǁA�\�����H�i�ǂ��� shader �ł͒��_���Ƃɕʌv�Z�Ȃ̂Łj
    // �E���[�g�������@�c �x�X�g�����Ǝv�������ǑS�]���łm�f�@�k�n�c�����@�f�o�t�]�����悳�����Ǝv�������A�^�񒆗̈�őS�]���@�ʒu�v�Z��������Ƃ����s��
    static class InitUtility
    {
        // �b��i���ڒn�`�f�[�^��n�����悢���@�Ȃ����H�j
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
        public static unsafe GridHeightsTempBuffer<float> MakeCopyOfGridBuffer(
            this GridMaster.HeightFieldData heights, in GridMaster.DimensionData dim,
            int srcSerialIndex, int2 begin, int2 end)
        {
            BurstUtility.CopyGridToNativeBuffer(heights, dim, srcSerialIndex, begin, end, out var buffer);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void MemCpyStride<T>(T* pDst, int dstStride, T*pSrc, int srcStride, int elementLength, int count)
            where T : unmanaged
        {
            var u = sizeof(T);
            UnsafeUtility.MemCpyStride(pDst, dstStride * u, pSrc, srcStride * u, elementLength * u, count * u);
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

    public unsafe struct GridHeightsTempBuffer<T> : IDisposable
        where T : unmanaged
    {
        public T* p { get; private set; }
        public int2 begin { get; private set; }
        public int length { get; private set; }
        int allocLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GridHeightsTempBuffer<T> Create(int allocLength, int2 begin, int length) => new GridHeightsTempBuffer<T>
        {
            p = (T*)UnsafeUtility.Malloc(allocLength * sizeof(T), 32, Allocator.Temp),
            begin = begin,
            length = length,
            allocLength = allocLength,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> AsNativeArray() => NativeUtility.PtrToNativeArray(this.p, this.allocLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UnsafeUtility.Free(this.p, Allocator.Temp);
            this.p = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyToResource(
            GridMaster.HeightFieldShaderResourceData res,
            GridMaster.DimensionData dim,
            int dstSerialIndex)
        {
            var begin = this.begin;
            var length = this.length;
            var span = dim.UnitLengthInGrid.x + 1;
            var srcBeginIndex = begin.x;
            var dstBeginIndex = dstSerialIndex + begin.y * span + begin.x;
            res.Heights.Buffer.SetData(this.AsNativeArray(), srcBeginIndex, dstBeginIndex, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlobAssetReference<Collider> CreateCollider(
            GridMaster.DimensionData dim,
            CollisionFilter filter)
        {
            var meshtype = TerrainCollider.CollisionMethod.VertexSamples;
            var size = dim.UnitLengthInGrid + 1;
            var scale = new float3(dim.UnitScale, 1, dim.UnitScale);
            var na = NativeUtility.PtrToNativeArray<float>((float*)this.p, this.allocLength);
            return TerrainCollider.Create(na, size, scale, meshtype, filter);
        }
    }
    [BurstCompile]
    static class BurstUtility
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyGridToNativeBuffer(
            in GridMaster.HeightFieldData heights, in GridMaster.DimensionData dim,
            in int srcSerialIndex,
            in int2 begin, in int2 end, out GridHeightsTempBuffer<float> buffer)
        {
            var unitlength = dim.UnitLengthInGrid + 1;// �ƂȂ�̃O���b�h�����������Ă���

            //var length = (end.y - begin.y) * unitlength.x - begin.x + end.x + 1;
            var span = end - begin + 1;
            var length = (span.y - 1) * unitlength.x + span.x;

            var srcstride = dim.TotalLength.x;
            var dststride = unitlength.x;

            buffer = GridHeightsTempBuffer<float>.Create(span.y * dststride, begin, length);

            var pSrc = heights.p + srcSerialIndex + begin.y * srcstride;
            var pDst = buffer.p;
            //Debug.Log($"copy grid : unit{unitlength} span{span} length{length} alloc{span.y * dststride} srcIndex{srcSerialIndex}+{begin.y}*{srcstride}={srcSerialIndex + begin.y * srcstride} dststrid{dststride}");
            copy_plus1_(pSrc, srcstride, pDst, dststride, span.y);
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void copy_plus1_(
            float* pSrc, in int srcStride, float* pDst, in int dstStride, in int spanY)
        {
            if (X86.Avx2.IsAvx2Supported)
            //if (X86.Avx.IsAvxSupported)
            {
                var pDst_ = (v256*)pDst;
                var pSrc_ = (v256*)pSrc;
                var dstStride_ = dstStride >> 3;
                var srcStride_ = srcStride >> 3;
                for (var iy = 0; iy < spanY; iy++)
                {
                    var pSrcSave_ = pSrc_;

                    for (var ix = 0; ix < dstStride_; ix++)
                    {
                        var val = X86.Avx2.mm256_stream_load_si256(pSrc_++);
                        //var val = X86.Avx.mm256_load_ps(pSrc_++);
                        X86.Avx.mm256_storeu_ps(pDst_++, val);
                    }

                    var pSrc1_ = (float*)pSrc_;
                    var pDst1_ = (float*)pDst_;
                    *pDst1_++ = *pSrc1_;

                    pDst_ = (v256*)pDst1_;
                    pSrc_ = pSrcSave_ + srcStride_;
                }
            }
            else if(X86.Sse4_1.IsSse41Supported)
            {
                var pDst_ = (v128*)pDst;
                var pSrc_ = (v128*)pSrc;
                var dstStride_ = dstStride >> 2;
                var srcStride_ = srcStride >> 2;
                for (var iy = 0; iy < spanY; iy++)
                {
                    var pSrcSave_ = pSrc_;

                    for (var ix = 0; ix < dstStride_; ix++)
                    {
                        var val = X86.Sse4_1.stream_load_si128(pSrc_++);
                        X86.Sse2.storeu_si128(pDst_++, val);
                    }

                    var pSrc1_ = (float*)pSrc_;
                    var pDst1_ = (float*)pDst_;
                    *pDst1_++ = *pSrc1_;

                    pDst_ = (v128*)pDst1_;
                    pSrc_ = pSrcSave_ + srcStride_;
                }
            }
            else
            {
                //InitUtility.MemCpyStride(pDst, dstStride, pSrc, srcStride, dstStride, spanY);
                // ���Ȃ񂩃G�f�B�^��������c
                for (var iy = 0; iy < spanY; iy++)
                {
                    var pSrcSave = pSrc;

                    for (var ix = 0; ix < dstStride; ix++)
                    {
                        *pDst++ = *pSrc++;
                    }

                    pSrc = pSrcSave + srcStride;
                }
            }
        }
    }

}