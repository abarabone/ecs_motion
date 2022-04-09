using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace DotsLite.Draw
{

    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Dependency;

    /// <summary>
    /// 描画ゼロでもスレッド使ってしまうかも　そのうちなんとかしたい
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Sort))]
    public partial class SortModelBufferSystem : SystemBase
    {


        EntityQuery sortQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<DrawSystem.SortingNativeTransformBufferData>();

            this.sortQuery = this.GetEntityQuery(ComponentType.ReadOnly<DrawModel.SortSettingData>());
            this.RequireForUpdate(this.sortQuery);
        }



        protected override unsafe void OnUpdate()
        {
            var campos = Camera.main.transform.position.As_float3();


            var sortingBuffers = this.GetComponentDataFromEntity<DrawSystem.SortingNativeTransformBufferData>();
            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>();
            var bufferinfos = this.GetComponentDataFromEntity<DrawSystem.TransformBufferInfoData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var useTempJobBuffer = this.HasSingleton<DrawSystem.TransformBufferUseTempJobTag>();


            this.Job
                .WithName("prev")
                .WithBurst()
                .WithReadOnly(bufferinfos)
                .WithCode(() =>
                {
                    if (useTempJobBuffer)
                    {
                        swapBuffer_(drawSysEnt, sortingBuffers, nativeBuffers);
                    }
                    else
                    {
                        var nativebuffer = nativeBuffers[drawSysEnt];
                        var sortingbuffer = sortingBuffers[drawSysEnt];
                        var info = bufferinfos[drawSysEnt];
                        copyBuffer_(sortingbuffer, nativebuffer, info.CurrentVectorLength);
                    }
                })
                .Schedule();


            this.Entities
                .WithName("sort")
                .WithBurst()
                .WithReadOnly(sortingBuffers)
                .WithReadOnly(nativeBuffers)
                .ForEach(
                    (
                        in DrawModel.InstanceCounterData counter,
                        in DrawModel.VectorIndexData offset,
                        in DrawModel.BoneVectorSettingData info,
                        in DrawModel.SortSettingData sort
                    ) =>
                    {
                        if (counter.InstanceCounter.Count == 0) return;


                        var length = counter.InstanceCounter.Count;
                        var nativebuffer = nativeBuffers[drawSysEnt];
                        var sortingbuffer = sortingBuffers[drawSysEnt];

                        switch (sort.Order)
                        {
                            case DrawModel.SortOrder.acs:
                                {
                                    using var distsq_work =
                                        buildSortingArray_(campos, length, sortingbuffer, offset, info);

                                    distsq_work.Sort(new DistanceSortAsc());

                                    writeBack_(distsq_work, sortingbuffer, nativebuffer, offset, info);
                                }
                                break;

                            case DrawModel.SortOrder.desc:
                                {
                                    using var distsq_work =
                                        buildSortingArray_(campos, length, sortingbuffer, offset, info);

                                    distsq_work.Sort(new DistanceSortDesc());

                                    writeBack_(distsq_work, sortingbuffer, nativebuffer, offset, info);
                                }
                                break;

                            default:
                                copyModelArray_(sortingbuffer, nativebuffer, offset, info, length);
                                break;
                        }
                        //using var distsq_work =
                        //    buildSortingArray_(campos, length, sortingbuffer, offset, info);

                        //sort_(distsq_work, sort);

                        //writeBack_(distsq_work, sortingbuffer, nativebuffer, offset, info);
                    }
                )
                .ScheduleParallel();


            this.Job
                .WithName("after")
                .WithBurst()
                .WithCode(() =>
                {
                    if (useTempJobBuffer)
                    {
                        sortingBuffers[drawSysEnt].Transforms.Dispose();
                    }
                    else
                    {
                        // なにもしない
                    }
                })
                .Schedule();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void swapBuffer_(Entity drawSysEnt,
            ComponentDataFromEntity<DrawSystem.SortingNativeTransformBufferData> sortingBuffers,
            ComponentDataFromEntity<DrawSystem.NativeTransformBufferData> nativeBuffers)
        {
            var srcbuffer = nativeBuffers[drawSysEnt].Transforms;

            sortingBuffers[drawSysEnt] = new DrawSystem.SortingNativeTransformBufferData
            {
                Transforms = srcbuffer
            };

            nativeBuffers[drawSysEnt] = new DrawSystem.NativeTransformBufferData
            {
                Transforms = new SimpleNativeBuffer<float4>(srcbuffer.Length, Allocator.TempJob)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void copyBuffer_(
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawSystem.NativeTransformBufferData nativebuffer, int vectorlength)
        {
            var pSrc = nativebuffer.Transforms.pBuffer;
            var pDst = sortingSource.Transforms.pBuffer;

            UnsafeUtility.MemCpy(pDst, pSrc, sizeof(float4) * vectorlength);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe NativeArray<DistanceUnit> buildSortingArray_(
            float3 campos, int length,
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawModel.VectorIndexData offset, DrawModel.BoneVectorSettingData info)
        {
            var distsq_work = new NativeArray<DistanceUnit>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);


            var pSrc = sortingSource.Transforms.pBuffer + offset.ModelStartIndex;
            var dst = distsq_work;

            var src_span = offset.OptionalVectorLengthPerInstance + info.VectorLengthInBone * info.BoneLength;
            var src_ofs = offset.OptionalVectorLengthPerInstance;
            //Debug.Log($"info {src_span} {src_ofs}");

            for (var i = 0; i < distsq_work.Length; i++)
            {
                var pos = pSrc[i * src_span + src_ofs];

                dst[i] = new DistanceUnit
                {
                    index = i,
                    distsq = math.distancesq(pos.xyz, campos),
                };
            }

            return distsq_work;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void sort_(NativeArray<DistanceUnit> distsq_work, DrawModel.SortSettingData sort)
        //{
        //    //for (var i = 0; i < distsq_work.Length; i++) Debug.Log($"pre {distsq_work[i].index} {distsq_work[i].distsq}");
        //    switch (sort.Order)
        //    {
        //        case DrawModel.SortOrder.acs:
        //            distsq_work.Sort(new DistanceSortAsc());
        //            break;

        //        case DrawModel.SortOrder.desc:
        //            distsq_work.Sort(new DistanceSortDesc());
        //            break;

        //        default:
        //            break;
        //    }
        //    //for (var i = 0; i < distsq_work.Length; i++) Debug.Log($"aft {distsq_work[i].index} {distsq_work[i].distsq}");
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void writeBack_(
            NativeArray<DistanceUnit> distsq_work,
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawSystem.NativeTransformBufferData nativebuffer,
            DrawModel.VectorIndexData offset, DrawModel.BoneVectorSettingData info)
        {
            var src = distsq_work;
            var pSrc = sortingSource.Transforms.pBuffer + offset.ModelStartIndex;
            var pDst = nativebuffer.Transforms.pBuffer + offset.ModelStartIndex;

            var span = offset.OptionalVectorLengthPerInstance + info.VectorLengthInBone * info.BoneLength;
            var size = span * sizeof(float4);
            //Debug.Log($"{offset.OptionalVectorLengthPerInstance} {info.VectorLengthInBone} {info.BoneLength} / {span} {size}");

            for (var i = 0; i < distsq_work.Length; i++)
            {
                var isrc = src[i].index * span;
                var idst = i * span;
                UnsafeUtility.MemCpy(pDst + idst, pSrc + isrc, size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void copyModelArray_(
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawSystem.NativeTransformBufferData nativebuffer,
            DrawModel.VectorIndexData offset, DrawModel.BoneVectorSettingData info, int length)
        {
            var pSrc = sortingSource.Transforms.pBuffer + offset.ModelStartIndex;
            var pDst = nativebuffer.Transforms.pBuffer + offset.ModelStartIndex;

            var span = offset.OptionalVectorLengthPerInstance + info.VectorLengthInBone * info.BoneLength;
            var size = span * sizeof(float4);

            UnsafeUtility.MemCpy(pDst, pSrc, size * length);
        }
    }

    public struct DistanceUnit
    {
        public int index;
        public float distsq;
    }

    public struct DistanceSortAsc : IComparer<DistanceUnit>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistanceUnit a, DistanceUnit b) => b.distsq.CompareTo(a.distsq);
    }
    public struct DistanceSortDesc : IComparer<DistanceUnit>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(DistanceUnit a, DistanceUnit b) => a.distsq.CompareTo(b.distsq);
    }
}
