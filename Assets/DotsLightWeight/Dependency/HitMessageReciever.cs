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

namespace Abarabone.Dependency
{



    /// <summary>
    /// hit message を処理するジョブを構築する。
    /// </summary>
    public interface IHitMessageApplyJobExecution<THitMessage>
        where THitMessage : struct
    {
        void Execute(int index, Entity targetEntity, THitMessage hitMessage);
    }

    public static class HitMessageApplyJobExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallel<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution job,
                HitMessageReciever<THitMessage, TJobInnerExecution> reciever,
                int innerLoopBatchCount,
                JobHandle dependency
            )
            where THitMessage : struct
            where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
        =>
            reciever.ScheduleParallel(dependency, innerLoopBatchCount, job);
    }


    public struct HitMessageRecieverParallelWriter<THitMessage>
        where THitMessage : struct
    {
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        NativeList<Entity>.ParallelWriter nl;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        NativeMultiHashMap<Entity, THitMessage>.ParallelWriter hm;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        NativeHashSet<Entity>.ParallelWriter uk;

        public HitMessageRecieverParallelWriter
            (
                ref NativeList<Entity> nl,
                ref NativeMultiHashMap<Entity, THitMessage> hm,
                ref NativeHashSet<Entity> uk
            )
        {
            this.nl = nl.AsParallelWriter();
            this.hm = hm.AsParallelWriter();
            this.uk = uk.AsParallelWriter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Entity entity, THitMessage hitMessage)
        {
            if (this.uk.Add(entity)) this.nl.AddNoResize(entity);
            this.hm.Add(entity, hitMessage);
        }
    }



    /// <summary>
    /// ヒットした相手からのメッセージを受け取り、ためておく。
    /// メッセージは、IHitMessageApplyJobExecution<THitMessage> ジョブで処理する。
    /// ヒット検出側システムには ParallelWriter を渡して、書き込んでもらう。
    /// </summary>
    public partial struct HitMessageReciever<THitMessage, TJobInnerExecution> : IDisposable
        where THitMessage : struct
        where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
    {

        HitMessageHolder holder;
        DependencyBarrier barrier;
        //ParallelWriter writer;


        public HitMessageReciever(int capacity, int maxDependsSystem = 16)
        {
            this.holder = new HitMessageHolder(capacity);
            this.barrier = new DependencyBarrier(maxDependsSystem);
            //this.writer = this.holder.AsParallelWriter();
        }


        //public ParallelWriter AsParallelWriter() => //this.writer;
        //    this.holder.AsParallelWriter();


        //public void AddDependencyBeforeHitApply(JobHandle jobHandle) =>
        //    this.barrier.AddDependencyBefore(jobHandle);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle ScheduleParallel(JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
        {
            //this.waiter.CompleteAllDependentJobs(dependency);
            //var dep0 = dependency;
            var dep0 = this.barrier.CombineAllDependentJobs(dependency);
            var dep1 = this.holder.ExecutionAndSchedule(dep0, innerLoopBatchCount, execution);
            var dep2 = this.holder.ClearAndSchedule(dep1);

            return dep2;
        }


        public void Dispose()
        {
            this.holder.Dispose();
            this.barrier.Dispose();
        }
    }

    public partial struct HitMessageReciever<THitMessage, TJobInnerExecution>
    {
        /// <summary>
        /// ハッシュマップでまともな巡回ができるようになるまでのつなぎ
        /// </summary>
        public struct HitMessageHolder : IDisposable
        {

            NativeList<Entity> keyEntities;
            NativeMultiHashMap<Entity, THitMessage> messageHolder;

            NativeHashSet<Entity> uniqueKeys;


            public HitMessageHolder(int capacity)
            {
                this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
                this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);

                this.uniqueKeys = new NativeHashSet<Entity>(capacity, Allocator.Persistent);
            }


            public HitMessageRecieverParallelWriter<THitMessage> AsParallelWriter() =>
                new HitMessageRecieverParallelWriter<THitMessage>(ref this.keyEntities, ref this.messageHolder, ref this.uniqueKeys);


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ExecutionAndSchedule(JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution) =>
                new HitMessageApplyJob
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
        struct HitMessageApplyJob : IJobParallelForDefer
        {
            [ReadOnly]
            public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

            [ReadOnly]
            public NativeArray<Entity> KeyEntities;


            public TJobInnerExecution InnerJob;


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


    }



}
