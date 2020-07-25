using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Structure
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;


    public class StructurePartHitHolder
    {

    }



    public struct StructurePartHitMessage
    {
        public uint PartId;
        public float3 HitPosition;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
    public class StructurePartHitMessageApplySystem : SystemBase
    {

        StructurePartHitMessageHolderAllocationSystem messageSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.messageSystem = this.World.GetExistingSystem<StructurePartHitMessageHolderAllocationSystem>();
        }

        protected override void OnUpdate()
        {

            var msgs = this.messageSystem.MsgHolder;
            

            this.Entities
                .WithBurst()
                .WithReadOnly(msgs)
                //.WithDeallocateOnJobCompletion(msgs)
                .ForEach(
                    (
                        Entity stent,
                        ref Structure.PartAlivedData alive
                    ) =>
                    {
                        var partAliveFlags = new Structure.PartAlivedData { };


                        var isSuccess = msgs.TryGetFirstValue(stent, out var hitMsg, out var iterator);
                        
                        while(isSuccess)
                        {


                            partAliveFlags.SetTrunOn(hitMsg.PartId);


                            isSuccess = msgs.TryGetNextValue(out hitMsg, ref iterator);
                        }


                        alive = partAliveFlags;
                    }
                )
                .ScheduleParallel();

        }

    }







    [JobProducerType(typeof(JobNativeMultiHashMapVisitKeyMutableValue.JobNativeMultiHashMapVisitKeyMutableValueProducer<,,>))]
    public interface IJobNativeMultiHashMapVisitKeyMutableValue<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        void ExecuteNext(TKey key, ref TValue value);
    }

    public static class JobNativeMultiHashMapVisitKeyMutableValue
    {
        internal struct JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue>
            where TJob : struct, IJobNativeMultiHashMapVisitKeyMutableValue<TKey, TValue>
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {

            [NativeDisableContainerSafetyRestriction]
            internal NativeMultiHashMap<TKey, TValue> HashMap;
            internal TJob JobData;

            static IntPtr s_JobReflectionData;


            internal static IntPtr Initialize()
            {

                if (s_JobReflectionData == IntPtr.Zero)
                {
                    s_JobReflectionData = JobsUtility.CreateJobReflectionData
                    (
                        typeof(JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue>),
                        typeof(TJob), JobType.ParallelFor,
                        (ExecuteJobFunction)Execute
                    );
                }

                return s_JobReflectionData;
            }


            internal delegate void ExecuteJobFunction
                (ref JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue> producer,
                IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex
            );

            public static unsafe void Execute
                (
                    ref JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue> producer,
                    IntPtr additionalPtr,
                    IntPtr bufferRangePatchData,
                    ref JobRanges ranges,
                    int jobIndex
                )
            {
                while (true)
                {
                    int begin;
                    int end;

                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out begin, out end))
                    {
                        return;
                    }

                    var bucketData = producer.HashMap.GetUnsafeBucketData();
                    var buckets = (int*)bucketData.buckets;
                    var nextPtrs = (int*)bucketData.next;
                    var keys = bucketData.keys;
                    var values = bucketData.values;

                    for (int i = begin; i < end; i++)
                    {
                        int entryIndex = buckets[i];

                        while (entryIndex != -1)
                        {
                            var key = UnsafeUtility.ReadArrayElement<TKey>(keys, entryIndex);

                            producer.JobData.ExecuteNext(key, ref UnsafeUtilityEx.ArrayElementAsRef<TValue>(values, entryIndex));

                            entryIndex = nextPtrs[entryIndex];
                        }
                    }
                }
            }
        }


        public static unsafe JobHandle Schedule<TJob, TKey, TValue>
            (this TJob jobData, NativeMultiHashMap<TKey, TValue> hashMap, int minIndicesPerJobCount, JobHandle dependsOn = new JobHandle())
            where TJob : struct, IJobNativeMultiHashMapVisitKeyMutableValue<TKey, TValue>
            where TKey : struct, IEquatable<TKey>
            where TValue : struct
        {
            var jobProducer = new JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue>
            {
                HashMap = hashMap,
                JobData = jobData
            };

            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref jobProducer)
                , JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue>.Initialize()
                , dependsOn
                , ScheduleMode.Batched
            );

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, hashMap.GetUnsafeBucketData().bucketCapacityMask + 1, minIndicesPerJobCount);
        }
    }
}
