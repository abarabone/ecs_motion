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
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Sort))]
    public class SortModelBufferSystem : SystemBase
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
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var useTempJobBuffer = this.HasSingleton<DrawSystem.TransformBufferUseTempJobTag>();


            this.Job
                .WithBurst()
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
                        copyBuffer_(sortingbuffer, nativebuffer);
                    }
                })
                .Schedule();


            this.Entities
                .WithBurst()
                .WithReadOnly(sortingBuffers)
                .WithReadOnly(nativeBuffers)
                .ForEach(
                    (
                        in DrawModel.InstanceCounterData counter,
                        in DrawModel.InstanceOffsetData offset,
                        in DrawModel.BoneVectorSettingData info,
                        in DrawModel.SortSettingData sort
                    ) =>
                    {
                        if (counter.InstanceCounter.Count == 0) return;


                        var length = counter.InstanceCounter.Count;
                        var nativebuffer = nativeBuffers[drawSysEnt];
                        var sortingbuffer = sortingBuffers[drawSysEnt];

                        using var distsq_work =
                            buildSortingArray_(campos, length, sortingbuffer, offset, info);

                        sort_(distsq_work, sort);

                        writeBack_(distsq_work, sortingbuffer, nativebuffer, offset, info);
                    }
                )
                .ScheduleParallel();


            this.Job
                .WithBurst()
                .WithCode(() =>
                {
                    if (useTempJobBuffer)
                    {
                        sortingBuffers[drawSysEnt].Transforms.Dispose();
                    }
                    else
                    {

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
                Transforms = new SimpleNativeBuffer<float4>(srcbuffer.Length)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void copyBuffer_(
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawSystem.NativeTransformBufferData nativebuffer)
        {

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe NativeArray<DistanceUnit> buildSortingArray_(
            float3 campos, int length,
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawModel.InstanceOffsetData offset, DrawModel.BoneVectorSettingData info)
        {
            var distsq_work = new NativeArray<DistanceUnit>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);


            var pSrc = sortingSource.Transforms.pBuffer + offset.VectorOffsetPerModel;
            var dst = distsq_work;

            var src_span = info.VectorLengthInBone + offset.VectorOffsetPerInstance;
            var src_ofs = offset.VectorOffsetPerInstance;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void sort_(NativeArray<DistanceUnit> distsq_work, DrawModel.SortSettingData sort)
        {
            switch (sort.Order)
            {
                case DrawModel.SortOrder.acs:
                    distsq_work.Sort(new DistanceSortAsc());
                    break;

                case DrawModel.SortOrder.desc:
                    distsq_work.Sort(new DistanceSortDesc());
                    break;

                default:
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void writeBack_(
            NativeArray<DistanceUnit> distsq_work,
            DrawSystem.SortingNativeTransformBufferData sortingSource,
            DrawSystem.NativeTransformBufferData nativebuffer,
            DrawModel.InstanceOffsetData offset, DrawModel.BoneVectorSettingData info)
        {
            var src = distsq_work;
            var pSrc = sortingSource.Transforms.pBuffer + offset.VectorOffsetPerModel;
            var pDst = nativebuffer.Transforms.pBuffer + offset.VectorOffsetPerModel;

            var span = info.VectorLengthInBone + offset.VectorOffsetPerInstance;
            var size = span * sizeof(float4);

            for (var i = 0; i < distsq_work.Length; i++)
            {
                var isrc = src[i].index * span;
                var idst = i * span;
                UnsafeUtility.MemCpy(pDst + idst, pSrc + isrc, size);
            }
        }
    }

    public struct DistanceUnit
    {
        public int index;
        public float distsq;
    }

    public struct DistanceSortAsc : IComparer<DistanceUnit>
    {
        public int Compare(DistanceUnit a, DistanceUnit b) => a.distsq.CompareTo(b.distsq);
    }
    public struct DistanceSortDesc : IComparer<DistanceUnit>
    {
        public int Compare(DistanceUnit a, DistanceUnit b) => b.distsq.CompareTo(a.distsq);
    }
}
