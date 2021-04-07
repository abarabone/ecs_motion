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



    public static partial class HitMessage<THitMessage>
        where THitMessage : struct
    {
        /// <summary>
        /// hit message を処理するジョブを構築する。
        /// </summary>
        public interface IApplyJobExecution
        {
            void Execute(int index, Entity targetEntity, THitMessage hitMessage);
        }
    }


    public interface IRecievable<THitMessage>
        where THitMessage : struct
    {
        HitMessage<THitMessage>.Reciever Reciever { get; }
    }

    public static class HitMessageApplyJobExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallel<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution job,
                HitMessage<THitMessage>.Reciever reciever,
                int innerLoopBatchCount,
                JobHandle dependency
            )
            where THitMessage : struct
            where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecution
        =>
            reciever.ScheduleParallel(dependency, innerLoopBatchCount, job);
    }



    public static partial class HitMessage<THitMessage>
    {


        /// <summary>
        /// ヒットした相手からのメッセージを受け取り、ためておく。
        /// メッセージは、IHitMessageApplyJobExecution<THitMessage> ジョブで処理する。
        /// ヒット検出側システムには ParallelWriter を渡して、書き込んでもらう。
        /// </summary>
        public partial struct Reciever : IDisposable
        {

            public HitMessageHolder Holder { get; }
            public DependencyBarrier Barrier { get; }


            public Reciever(int capacity, int maxDependsSystem = 16)
            {
                this.Holder = new HitMessageHolder(capacity);
                this.Barrier = new DependencyBarrier(maxDependsSystem);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ScheduleParallel<TJobInnerExecution>
                (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                where TJobInnerExecution : struct, IApplyJobExecution
            {
                //this.waiter.CompleteAllDependentJobs(dependency);
                //var dep0 = dependency;
                var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
                var dep1 = this.Holder.ExecutionAndSchedule(dep0, innerLoopBatchCount, execution);
                var dep2 = this.Holder.ClearAndSchedule(dep1);

                return dep2;
            }


            public void Dispose()
            {
                this.Holder.Dispose();
                this.Barrier.Dispose();
            }
        }

        public partial struct Reciever
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
                public JobHandle ExecutionAndSchedule<TJobInnerExecution>
                    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecution
                =>
                    new HitMessageApplyJob<TJobInnerExecution>
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
            struct HitMessageApplyJob<TJobInnerExecution> : IJobParallelForDefer
                where TJobInnerExecution : struct, IApplyJobExecution
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


        }




        [BurstCompile]
        public struct ParallelWriter
        {
            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeList<Entity>.ParallelWriter nl;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeMultiHashMap<Entity, THitMessage>.ParallelWriter hm;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeHashSet<Entity>.ParallelWriter uk;

            public ParallelWriter
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


    }


}
