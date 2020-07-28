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
    using System.Security.Cryptography;


    //public class StructurePartHitHolder
    //{

    //}



    public struct StructureHitMessage
    {
        public int PartId;
        public Entity PartEntity;
        public float3 Position;
        public float3 Normale;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
    public class StructureHitMessageApplySystem : SystemBase
    {

        StructureHitMessageHolderAllocationSystem messageSystem;

        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            this.messageSystem = this.World.GetExistingSystem<StructureHitMessageHolderAllocationSystem>();
        }

        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().ToConcurrent();

            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

            var destractions = this.GetComponentDataFromEntity<Structure.PartDestractionData>();


            var msgs = this.messageSystem.MsgHolder;

            this.Dependency = new StructureHitApplyJob
            {
                Destractions = destractions,
                Cmd = cmd,
            }
            .Schedule(msgs, 0, this.Dependency);


            //this.Entities
            //    .WithBurst()
            //    .WithReadOnly(msgs)
            //    .WithReadOnly(parts)
            //    //.WithDeallocateOnJobCompletion(msgs)
            //    .ForEach(
            //        (
            //            Entity stent, int entityInQueryIndex,
            //            ref Structure.PartDestractionData alive
            //        ) =>
            //        {
            //            var isSuccess = msgs.TryGetFirstValue(stent, out var hitMsg, out var iterator);
                        
            //            while(isSuccess)
            //            {

            //                var partData = parts[hitMsg.PartEntity];


            //                alive.SetDestroyed(partData.PartId);

            //                cmd.DestroyEntity(entityInQueryIndex, hitMsg.PartEntity);


            //                isSuccess = msgs.TryGetNextValue(out hitMsg, ref iterator);
            //            }
            //        }
            //    )
            //    .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }


        struct StructureHitApplyJob : IJobNativeMultiHashMapVisitKeyMutableValue<Entity, StructureHitMessage>
        {

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Structure.PartDestractionData> Destractions;

            //[ReadOnly]
            public EntityCommandBuffer.Concurrent Cmd;


            public void ExecuteNext(int uniqueIndex, Entity key, ref StructureHitMessage value)
            {

                var destraction = this.Destractions[key];

                destraction.SetDestroyed(value.PartId);

                this.Destractions[key] = destraction;


                this.Cmd.DestroyEntity(uniqueIndex, value.PartEntity);

            }
        }

    }







    [JobProducerType(typeof(JobNativeMultiHashMapVisitKeyMutableValue.JobNativeMultiHashMapVisitKeyMutableValueProducer<,,>))]
    public interface IJobNativeMultiHashMapVisitKeyMutableValue<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        void ExecuteNext(int uniqueIndex, TKey key, ref TValue value);
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
                var uniqueIndex = 0;

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

                            producer.JobData.ExecuteNext(uniqueIndex++, key, ref UnsafeUtilityEx.ArrayElementAsRef<TValue>(values, entryIndex));

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
