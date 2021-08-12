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



        protected override unsafe void OnUpdate()
        {
            var campos = Camera.main.transform.position.As_float3();


            this.Entities
                //.WithBurst()
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

                        using var distsq_work = buildSortingArray_(campos, length, offset, info);

                        sort_(distsq_work, sort);

                        writeBack_();



                    }
                )
                ;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe NativeArray<DistanceUnit> buildSortingArray_(
            float3 campos, int length,
            DrawModel.InstanceOffsetData offset, DrawModel.BoneVectorSettingData info)
        {
            var distsq_work = new NativeArray<DistanceUnit>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);


            var pSrc = offset.pVectorOffsetPerModelInBuffer;
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
            if (sort.IsSortAsc)
            {
                distsq_work.Sort(new DistanceSortAsc());
            }
            else
            {
                distsq_work.Sort(new DistanceSortDesc());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void writeBack_(
            NativeArray<DistanceUnit> distsq_work,
            DrawModel.InstanceOffsetData offset, DrawModel.BoneVectorSettingData info)
        {
            var src = distsq_work;
            var pDst = offset.pVectorOffsetPerModelInBuffer;

            var span = info.VectorLengthInBone + offset.VectorOffsetPerInstance;
            var size = span * sizeof(float4);

            for (var i = 0; i < distsq_work.Length; i++)
            {
                var isrc = src[i].index * span;
                var idst = i * span;
                UnsafeUtility.MemCpy(pDst + idst, pDst + isrc, size);
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
