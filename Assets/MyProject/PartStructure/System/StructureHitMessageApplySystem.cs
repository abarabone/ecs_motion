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
    using UnityEngine.Video;
    using System.Runtime.CompilerServices;

    public struct StructureHitMessage
    {
        public int PartId;
        public Entity PartEntity;
        public float3 Position;
        public float3 Normale;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
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
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

            //var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

            var destructions = this.GetComponentDataFromEntity<Structure.PartDestructionData>();
            var prefabs = this.GetComponentDataFromEntity<StructurePart.DebrisPrefabData>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            

            var msgs = this.messageSystem.MsgHolder;
            
            this.Dependency = new StructureHitApplyJob
            {
                Cmd = cmd,
                Destructions = destructions,
                Prefabs = prefabs,
                Rotations = rots,
                Positions = poss,
            }
            .Schedule(msgs, 0, this.Dependency);

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }


        [BurstCompile]
        struct StructureHitApplyJob : IJobNativeMultiHashMapVisitKeyMutableValue<Entity, StructureHitMessage>
        {

            public EntityCommandBuffer.ParallelWriter Cmd;


            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Structure.PartDestructionData> Destructions;

            [ReadOnly]
            public ComponentDataFromEntity<StructurePart.DebrisPrefabData> Prefabs;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> Positions;


            //[BurstCompile]
            public void ExecuteNext(int uniqueIndex, Entity key, ref StructureHitMessage value)
            {

                var destruction = this.Destructions[key];

                // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
                destruction.SetDestroyed(value.PartId);

                this.Destructions[key] = destruction;


                var prefab = this.Prefabs[value.PartEntity].DebrisPrefab;
                var rot = this.Rotations[value.PartEntity];
                var pos = this.Positions[value.PartEntity];
                createDebris_(this.Cmd, uniqueIndex, prefab, rot, pos);

                destroyPart_(this.Cmd, uniqueIndex, value.PartEntity);

            }

        }

        [BurstCompile]
        struct StructureHitApplyJob2 : IJobNativeMultiHashMapMergedSharedKeyIndices<Entity, StructureHitMessage>
        {

            public EntityCommandBuffer.ParallelWriter Cmd;


            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Structure.PartDestructionData> Destructions;

            [ReadOnly]
            public ComponentDataFromEntity<StructurePart.DebrisPrefabData> Prefabs;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> Positions;


            //[BurstCompile]
            public void ExecuteNext(int uniqueIndex, Entity key, ref StructureHitMessage value)
            {

                var destruction = this.Destructions[key];

                // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
                destruction.SetDestroyed(value.PartId);

                this.Destructions[key] = destruction;


                var prefab = this.Prefabs[value.PartEntity].DebrisPrefab;
                var rot = this.Rotations[value.PartEntity];
                var pos = this.Positions[value.PartEntity];
                createDebris_(this.Cmd, uniqueIndex, prefab, rot, pos);

                destroyPart_(this.Cmd, uniqueIndex, value.PartEntity);

            }

        }


        //[BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void createDebris_
            (
                EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
                Rotation rot_, Translation pos_
            )
        {

            var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
            cmd_.SetComponent(uniqueIndex_, ent, rot_);
            cmd_.SetComponent(uniqueIndex_, ent, pos_);

        }

        //[BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void destroyPart_
            (EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
        {
            cmd_.DestroyEntity(uniqueIndex_, part_);
        }
    }







    [JobProducerType(typeof(JobNativeMultiHashMapVisitKeyMutableValue.JobNativeMultiHashMapVisitKeyMutableValueProducer<,,>))]
    public interface IJobNativeMultiHashMapVisitKeyMutableValue<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
        where TValue : struct
    {
        void ExecuteNext(int uniqueIndex, TKey key, ref TValue value);
    }

    [BurstCompile]
    public static class JobNativeMultiHashMapVisitKeyMutableValue
    {
        //[BurstCompile]
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
                        typeof(TJob),
                        (ExecuteJobFunction)Execute
                    );
                }

                return s_JobReflectionData;
            }


            internal delegate void ExecuteJobFunction
                (ref JobNativeMultiHashMapVisitKeyMutableValueProducer<TJob, TKey, TValue> producer,
                IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex
            );

            //[BurstCompile]
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

                            producer.JobData.ExecuteNext(uniqueIndex++, key, ref UnsafeUtility.ArrayElementAsRef<TValue>(values, entryIndex));

                            entryIndex = nextPtrs[entryIndex];
                        }
                    }
                }
            }
        }


        //[BurstCompile]
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
                , ScheduleMode.Parallel
            );

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, hashMap.GetUnsafeBucketData().bucketCapacityMask + 1, minIndicesPerJobCount);
        }
    }


    // -----

    public interface IJobNativeMultiHashMapMergedSharedKeyIndices
    {
        void ExecuteFirst(int index);
        void ExecuteNext(int firstIndex, int index);
    }

    public static class JobNativeMultiHashMapUniqueHashExtensions
    {
        struct NativeMultiHashMapUniqueHashJobStruct<TJob, TKey>
            where TJob : struct, IJobNativeMultiHashMapMergedSharedKeyIndices
            where TKey : struct, IEquatable<TKey>
        {
            internal struct JobMultiHashMap
            {
                [ReadOnly] public NativeMultiHashMap<TKey, int> HashMap;
                public TJob JobData;
            }

            private static IntPtr jobReflectionData;

            public static IntPtr Initialize()
            {
                //if (jobReflectionData == IntPtr.Zero)
                //    jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobMultiHashMap), typeof(TJob),
                //        JobType.ParallelFor, (ExecuteJobFunction)Execute);
                //return jobReflectionData;
                if (jobReflectionData == IntPtr.Zero)
                {
                    jobReflectionData = JobsUtility.CreateJobReflectionData
                    (
                        typeof(NativeMultiHashMapUniqueHashJobStruct<TJob, TKey>),
                        typeof(TJob),
                        (ExecuteJobFunction)Execute
                    );
                }
                return jobReflectionData;
            }

            private delegate void ExecuteJobFunction(ref JobMultiHashMap fullData, IntPtr additionalPtr,
                IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            private static unsafe void Execute(ref JobMultiHashMap fullData, IntPtr additionalPtr,
                IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (true)
                {
                    int begin;
                    int end;

                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out begin, out end))
                        return;

                    var bucketData = fullData.HashMap.GetUnsafeBucketData();
                    var buckets = (int*)bucketData.buckets;
                    var nextPtrs = (int*)bucketData.next;
                    var keys = bucketData.keys;
                    var values = bucketData.values;
                    //var buckets = (int*)fullData.HashMap.m_Buffer->buckets;
                    //var nextPtrs = (int*)fullData.HashMap.m_Buffer->next;
                    //var keys = fullData.HashMap.m_Buffer->keys;
                    //var values = fullData.HashMap.m_Buffer->values;

                    for (int i = begin; i < end; i++)
                    {
                        int entryIndex = buckets[i];

                        while (entryIndex != -1)
                        {
                            var key = UnsafeUtility.ReadArrayElement<TKey>(keys, entryIndex);
                            var value = UnsafeUtility.ReadArrayElement<int>(values, entryIndex);
                            int firstValue;

                            NativeMultiHashMapIterator<TKey> it;
                            fullData.HashMap.TryGetFirstValue(key, out firstValue, out it);

                            // [macton] Didn't expect a usecase for this with multiple same values
                            // (since it's intended use was for unique indices.)
                            // https://forum.unity.com/threads/ijobnativemultihashmapmergedsharedkeyindices-unexpected-behavior.569107/#post-3788170
                            if (entryIndex == it.GetEntryIndex())//.EntryIndex)
                            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS

                                JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData,
                                    UnsafeUtility.AddressOf(ref fullData), value, 1);
#endif
                                fullData.JobData.ExecuteFirst(value);
                            }
                            else
                            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                                //var startIndex = Math.Min(firstValue, value);
                                //var lastIndex = Math.Max(firstValue, value);
                                var startIndex = math.min(firstValue, value);
                                var lastIndex = math.max(firstValue, value);
                                var rangeLength = (lastIndex - startIndex) + 1;

                                JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData,
                                    UnsafeUtility.AddressOf(ref fullData), startIndex, rangeLength);
#endif
                                fullData.JobData.ExecuteNext(firstValue, value);
                            }

                            entryIndex = nextPtrs[entryIndex];
                        }
                    }
                }
            }
        }

        public static unsafe JobHandle Schedule<TJob, TKey>(this TJob jobData, NativeMultiHashMap<TKey, int> hashMap,
            int minIndicesPerJobCount, JobHandle dependsOn = new JobHandle())
            where TJob : struct, IJobNativeMultiHashMapMergedSharedKeyIndices
            where TKey : struct, IEquatable<TKey>
        {
            var fullData = new NativeMultiHashMapUniqueHashJobStruct<TJob, TKey>.JobMultiHashMap
            {
                HashMap = hashMap,
                JobData = jobData
            };

            //var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref fullData),
            //    NativeMultiHashMapUniqueHashJobStruct<TJob, TKey>.Initialize(), dependsOn, ScheduleMode.Batched);
            //return JobsUtility.ScheduleParallelFor(ref scheduleParams, hashMap.m_Buffer->bucketCapacityMask + 1,
            //    minIndicesPerJobCount);
            
            var scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref fullData)
                , NativeMultiHashMapUniqueHashJobStruct<TJob, TKey>.Initialize()
                , dependsOn
                , ScheduleMode.Parallel
            );

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, hashMap.GetUnsafeBucketData().bucketCapacityMask + 1, minIndicesPerJobCount);
        }
}
}
