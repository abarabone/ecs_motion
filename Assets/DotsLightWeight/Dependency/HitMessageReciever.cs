using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.LowLevel;

namespace DotsLite.Dependency
{


    public static partial class HitMessage<THitMessage>
    {
        /// <summary>
        /// 
        /// </summary>
        public interface IRecievable
        {
            Reciever Reciever { get; }
        }

        /// <summary>
        /// hit message を処理するジョブを構築する。
        /// </summary>
        public interface IApplyJobExecutionForEach
        {
            void Execute(int index, Entity targetEntity, THitMessage hitMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IApplyJobExecutionForKey
        {
            void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, THitMessage>.Enumerator hitMessages);
        }
    }



    public static class HitMessageApplyJobExtension
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallelEach<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution job,
                HitMessage<THitMessage>.Reciever reciever,
                int innerLoopBatchCount,
                JobHandle dependency,
                bool needClear = true
            )
            where THitMessage : struct, IHitMessage
            where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForEach
        =>
            reciever.ScheduleEachParallel(dependency, innerLoopBatchCount, job, needClear);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallelKey<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution job,
                HitMessage<THitMessage>.Reciever reciever,
                int innerLoopBatchCount,
                JobHandle dependency,
                bool needClear = true
            )
            where THitMessage : struct, IHitMessage
            where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        =>
            reciever.ScheduleKeyParallel(dependency, innerLoopBatchCount, job, needClear);

    }



    public static partial class HitMessage<THitMessage>
    {


        /// <summary>
        /// ヒットした相手からのメッセージを受け取り、ためておく。
        /// メッセージは、IHitMessageApplyJobExecution<THitMessage> ジョブで処理する。
        /// ヒット検出側システムには ParallelWriter を渡して、書き込んでもらう。
        /// </summary>
        public partial class Reciever : IDisposable
        //public partial struct Reciever : IDisposable
        {

            public HitMessageHolder Holder { get; }
            public BarrierDependency.Reciever Barrier { get; }


            public Reciever(int capacity, int maxDependsSystem = 16)
            {
                this.Holder = new HitMessageHolder(capacity);
                this.Barrier = BarrierDependency.Reciever.Create(maxDependsSystem);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ScheduleEachParallel<TJobInnerExecution>
                (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution, bool needClear = true)
                where TJobInnerExecution : struct, IApplyJobExecutionForEach
            {
                var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
                var dep1 = this.Holder.ExecuteEachAndSchedule(dep0, innerLoopBatchCount, execution);
                var dep2 = needClear ? this.Holder.ClearAndSchedule(dep1) : dep1;

                return dep2;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ScheduleKeyParallel<TJobInnerExecution>
                (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution, bool needClear = true)
                where TJobInnerExecution : struct, IApplyJobExecutionForKey
            {
                var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
                var dep1 = this.Holder.ExecuteKeyAndSchedule(dep0, innerLoopBatchCount, execution);
                var dep2 = needClear ? this.Holder.ClearAndSchedule(dep1) : dep1;

                return dep2;
            }


            public void Dispose()
            {
                this.Holder.Dispose();
                this.Barrier.Dispose();
            }
        }

        public partial class Reciever
        //public partial struct Reciever
        {

            /// <summary>
            /// ハッシュマップでまともな巡回ができるようになるまでのつなぎ。
            /// native list にユニークな entity を登録し、キーとして巡回する。
            /// </summary>
            public struct HitMessageHolder : IDisposable
            {

                NativeMultiHashMap<Entity, THitMessage> messageHolder;

                NativeList<Entity> keyEntities;
                NativeHashSet<Entity> uniqueKeys;

                //ParallelWriter writer;//


                public HitMessageHolder(int capacity)
                {
                    this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);

                    this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
                    this.uniqueKeys = new NativeHashSet<Entity>(capacity, Allocator.Persistent);

                    //this.writer = new ParallelWriter(ref this.keyEntities, ref this.messageHolder, ref this.uniqueKeys);//
                }


                public ParallelWriter AsParallelWriter() => //this.writer;
                    new ParallelWriter(ref this.keyEntities, ref this.messageHolder, ref this.uniqueKeys);


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandle ExecuteEachAndSchedule<TJobInnerExecution>
                    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecutionForEach
                =>
                    new HitMessageApplyJobForEach<TJobInnerExecution>
                    {
                        MessageHolder = this.messageHolder,
                        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                        InnerJob = execution,
                    }
                    .Schedule(this.keyEntities, innerLoopBatchCount, dependency);


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandle ExecuteKeyAndSchedule<TJobInnerExecution>
                    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecutionForKey
                =>
                    new HitMessageApplyJobForKey<TJobInnerExecution>
                    {
                        MessageHolder = this.messageHolder,
                        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                        InnerJob = execution,
                    }
                    .Schedule(this.keyEntities, innerLoopBatchCount, dependency);


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandle ClearAndSchedule(JobHandle dependency) =>
                    new clearJob
                    {
                        keyEntities = this.keyEntities,
                        messageHolder = this.messageHolder,
                        uniqueKeys = this.uniqueKeys,
                    }
                    .Schedule(dependency);


                public void Dispose()
                {
                    this.keyEntities.Dispose();
                    this.messageHolder.Dispose();
                    this.uniqueKeys.Dispose();
                }

            }


            /// <summary>
            /// 
            /// </summary>
            [BurstCompile]
            struct clearJob : IJob
            {
                public NativeList<Entity> keyEntities;
                public NativeMultiHashMap<Entity, THitMessage> messageHolder;
                public NativeHashSet<Entity> uniqueKeys;

                public void Execute()
                {
                    this.keyEntities.Clear();
                    this.messageHolder.Clear();
                    this.uniqueKeys.Clear();
                }
            }


            /// <summary>
            /// 
            /// </summary>
            [BurstCompile]
            struct HitMessageApplyJobForEach<TJobInnerExecution> : IJobParallelForDefer
                where TJobInnerExecution : struct, IApplyJobExecutionForEach
            {
                [ReadOnly]
                public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

                [ReadOnly]
                public NativeArray<Entity> KeyEntities;


                [NativeDisableParallelForRestriction]
                [NativeDisableContainerSafetyRestriction]
                public TJobInnerExecution InnerJob;


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Execute(int index)
                {
                    var key = this.KeyEntities[index];
                    var msgs = this.MessageHolder.GetValuesForKey(key);

                    foreach (var msg in msgs)
                    {
                        this.InnerJob.Execute(index, key, msg);
                    }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            [BurstCompile]
            struct HitMessageApplyJobForKey<TJobInnerExecution> : IJobParallelForDefer
                where TJobInnerExecution : struct, IApplyJobExecutionForKey
            {
                [ReadOnly]
                public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

                [ReadOnly]
                public NativeArray<Entity> KeyEntities;


                [NativeDisableParallelForRestriction]
                [NativeDisableContainerSafetyRestriction]
                public TJobInnerExecution InnerJob;


                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Execute(int index)
                {
                    var key = this.KeyEntities[index];
                    var msgs = this.MessageHolder.GetValuesForKey(key);

                    this.InnerJob.Execute(index, key, msgs);
                }
            }
        }



    }


}
