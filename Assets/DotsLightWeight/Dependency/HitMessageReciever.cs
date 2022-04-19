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

        ///// <summary>
        ///// hit message を処理するジョブを構築する。
        ///// </summary>
        //public interface IApplyJobExecutionForEach
        //{
        //    void Execute(int index, Entity targetEntity, THitMessage hitMessage);
        //}

        /// <summary>
        /// 
        /// </summary>
        public interface IApplyJobExecutionForKey
        {
            void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, THitMessage>.Enumerator hitMessages);
        }
        //public interface IApplyJobExecutionForKey3 : IJobParallelForDefer
        //{
        //    NativeArray<Entity> KeyEntities { set; }
        //    NativeMultiHashMap<Entity, THitMessage> MessageHolder { set; }
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        //public interface IApplyExecutionForKey : IJobParallelForDefer
        //{
        //    void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, THitMessage>.Enumerator hitMessages);
        //}

    }

    //public struct Job<TJobInnerExecution, THitMessage>
    //    where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
    //        where THitMessage : struct, IHitMessage
    //{
    //    //public TJobInnerExecution innerjob;
    //    public HitMessage<THitMessage>.Reciever.HitMessageApplyJobForKey<TJobInnerExecution> outerjob;

    //    public JobHandle Schedule(
    //        HitMessage<THitMessage>.Reciever reciever,
    //        int innerLoopBatchCount,
    //        JobHandle dependency)
    //    {
    //        var dep0 = reciever.Barrier.CombineAllDependentJobs(dependency);
    //        var dep1 = this.outerjob.Schedule(reciever.Holder.keyEntities, innerLoopBatchCount, dep0);

    //        return dep1;
    //    }
    //}


    public static class HitMessageApplyJobExtension
    {

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static JobHandle ScheduleParallelEach<THitMessage, TJobInnerExecution>
        //    (
        //        this TJobInnerExecution job,
        //        HitMessage<THitMessage>.Reciever reciever,
        //        int innerLoopBatchCount,
        //        JobHandle dependency
        //    )
        //    where THitMessage : struct, IHitMessage
        //    where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForEach
        //=>
        //    reciever.ScheduleEachParallel(dependency, innerLoopBatchCount, job);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle ScheduleParallelKey<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution job,
                HitMessage<THitMessage>.Reciever reciever,
                int innerLoopBatchCount,
                JobHandle dependency
            )
            where THitMessage : struct, IHitMessage
            where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        =>
            reciever.ScheduleKeyParallel(dependency, innerLoopBatchCount, job);


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static JobHandle ScheduleParallelKey2<THitMessage, TJobInnerExecution>
        //    (
        //        this TJobInnerExecution job,
        //        HitMessage<THitMessage>.Reciever reciever,
        //        int innerLoopBatchCount,
        //        JobHandle dependency
        //    )
        //    where THitMessage : struct, IHitMessage
        //    where TJobInnerExecution : struct, IApplyJobExecutionForKey<THitMessage>
        //=>
        //    reciever.ScheduleKeyParallel2(dependency, innerLoopBatchCount, job);


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static JobHandle ScheduleParallelKey3<THitMessage, TJobInnerExecution>
        //    (
        //        this TJobInnerExecution job,
        //        HitMessage<THitMessage>.Reciever reciever,
        //        int innerLoopBatchCount,
        //        JobHandle dependency
        //    )
        //    where THitMessage : struct, IHitMessage
        //    where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        //{
        //    var dep0 = reciever.Barrier.CombineAllDependentJobs(dependency);
        //    return job.ScheduleParallelKey3_(reciever, innerLoopBatchCount, dep0);
        //}
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static JobHandle ScheduleParallelKey3_<THitMessage, TJobInnerExecution>
        //    (
        //        this TJobInnerExecution job,
        //        HitMessage<THitMessage>.Reciever reciever,
        //        int innerLoopBatchCount,
        //        JobHandle dependency
        //    )
        //    where THitMessage : struct, IHitMessage
        //    where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        //{
        //    return new HitMessage<THitMessage>.Reciever.HitMessageApplyJobForKey<TJobInnerExecution>
        //    {
        //        MessageHolder = reciever.Holder.messageHolder,
        //        KeyEntities = reciever.Holder.keyEntities.AsDeferredJobArray(),
        //        InnerJob = job,
        //    }
        //    .Schedule(reciever.Holder.keyEntities, innerLoopBatchCount, dependency);
        //}




        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HitMessage<THitMessage>.Reciever.HitMessageApplyJobForKey<TJobInnerExecution> With<THitMessage, TJobInnerExecution>
            (
                this TJobInnerExecution innerjob,
                HitMessage<THitMessage>.Reciever reciever
            )
            where THitMessage : struct, IHitMessage
            where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        {
            return new HitMessage<THitMessage>.Reciever.HitMessageApplyJobForKey<TJobInnerExecution>
            {
                MessageHolder = reciever.Holder.messageHolder,
                KeyEntities = reciever.Holder.keyEntities.AsDeferredJobArray(),
                InnerJob = innerjob,
            };
        }
        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static JobHandle Schedule<THitMessage, TJobInnerExecution>
        //    (
        //        this HitMessage<THitMessage>.Reciever.HitMessageApplyJobForKey<TJobInnerExecution> job,
        //        HitMessage<THitMessage>.Reciever reciever,
        //        int innerLoopBatchCount,
        //        JobHandle dependency
        //    )
        //    where THitMessage : struct, IHitMessage
        //    where TJobInnerExecution : struct, HitMessage<THitMessage>.IApplyJobExecutionForKey
        //{
        //    var dep0 = reciever.Barrier.CombineAllDependentJobs(dependency);
        //    var dep1 = job.Schedule(reciever.Holder.keyEntities, innerLoopBatchCount, dep0);

        //    return dep1;

        //}
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

            public HitMessageHolder Holder { get; private set; }
            public BarrierDependency.Reciever Barrier { get; }

            Allocator holderAllocator;// 暫定、dispose 用


            public Reciever(int capacity, Allocator allocator = Allocator.Persistent)//, int maxDependsSystem = 16)
            {
                this.Holder = new HitMessageHolder(capacity, allocator);
                this.Barrier = BarrierDependency.Reciever.Create(32);// maxDependsSystem);
                this.holderAllocator =allocator;
            }
            public Reciever()//int maxDependsSystem = 16)
            {
                //this.Holder = default;
                this.Barrier = BarrierDependency.Reciever.Create(32);// maxDependsSystem);
                this.holderAllocator = Allocator.None;
            }

            public void Alloc(int capacity, Allocator allocator = Allocator.Persistent)
            {
                this.Holder = new HitMessageHolder(capacity, allocator);
                this.holderAllocator = allocator;
            }


            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public JobHandle ScheduleEachParallel<TJobInnerExecution>
            //    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
            //    where TJobInnerExecution : struct, IApplyJobExecutionForEach
            //{
            //    var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
            //    var dep1 = this.Holder.ScheduleExecuteEach(dep0, innerLoopBatchCount, execution);
            //    return dep1;
            //}

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ScheduleKeyParallel<TJobInnerExecution>
                (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                where TJobInnerExecution : struct, IApplyJobExecutionForKey
            {
                var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
                var dep1 = this.Holder.ScheduleExecuteKey(dep0, innerLoopBatchCount, execution)
                    .Schedule(this.Holder.keyEntities, innerLoopBatchCount, dep0);
                return dep1;
                //return dependency;
            }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ScheduleKeyParallel2<TJobInnerExecution>
                (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                where TJobInnerExecution : struct, IApplyJobExecutionForKey<THitMessage>
            {
                var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
                var dep1 = this.Holder.ScheduleExecuteKey2(dep0, innerLoopBatchCount, execution)
                    .Schedule(this.Holder.keyEntities, innerLoopBatchCount, dep0);
                return dep1;
                //return dependency;
            }
            ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public JobHandle ToJob<TJobInnerExecution>(
            //    JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
            //    where TJobInnerExecution : struct, IApplyJobExecutionForKey
            //{
            //    var dep0 = this.Barrier.CombineAllDependentJobs(dependency);
            //    var dep1 = this.Holder.ToJob(execution)
            //        .Schedule(this.Holder.keyEntities, innerLoopBatchCount, dep0);
            //    return dep1;
            //    //return dependency;
            //}
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //public JobHandle combinealldependents(JobHandle dependency)
            //{
            //    return this.Barrier.CombineAllDependentJobs(dependency);
            //}


            public void Dispose()
            {
                if (this.holderAllocator == Allocator.Persistent) this.Holder.Dispose();
                this.Barrier.Dispose();
            }
        }

        public partial class Reciever
        //public partial struct Reciever
        {

            public struct DispatchJobs
            {

            }



            /// <summary>
            /// ハッシュマップでまともな巡回ができるようになるまでのつなぎ。
            /// native list にユニークな entity を登録し、キーとして巡回する。
            /// </summary>
            public struct HitMessageHolder : IDisposable
            {

                public//
                NativeMultiHashMap<Entity, THitMessage> messageHolder;

                public//
                NativeList<Entity> keyEntities;
                public//
                NativeHashSet<Entity> uniqueKeys;

                //ParallelWriter writer;//


                public NativeList<Entity> TargetEntities => this.keyEntities;


                public HitMessageHolder(int capacity, Allocator allocator)
                {
                    this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, allocator);

                    this.keyEntities = new NativeList<Entity>(capacity, allocator);
                    this.uniqueKeys = new NativeHashSet<Entity>(capacity, allocator);
                    //this.writer = new ParallelWriter(ref this.keyEntities, ref this.messageHolder, ref this.uniqueKeys);//
                }


                public ParallelWriter AsParallelWriter() => //this.writer;
                    new ParallelWriter(ref this.keyEntities, ref this.messageHolder, ref this.uniqueKeys);


                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                //public JobHandle ScheduleExecuteEach<TJobInnerExecution>
                //    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                //    where TJobInnerExecution : struct, IApplyJobExecutionForEach
                //=>
                //    new HitMessageApplyJobForEach<TJobInnerExecution>
                //    {
                //        MessageHolder = this.messageHolder,
                //        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                //        InnerJob = execution,
                //    }
                //    .Schedule(this.keyEntities, innerLoopBatchCount, dependency);
                ////dependency;


                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                public HitMessageApplyJobForKey<TJobInnerExecution> ScheduleExecuteKey<TJobInnerExecution>
                    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecutionForKey
                {
                    return new HitMessageApplyJobForKey<TJobInnerExecution>
                    {
                        MessageHolder = this.messageHolder,
                        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                        InnerJob = execution,
                    };
                    //return job.Schedule(this.keyEntities, innerLoopBatchCount, dependency);
                }
                //=> dependency;
                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                public HitMessageApplyJobForKey<TJobInnerExecution, THitMessage> ScheduleExecuteKey2<TJobInnerExecution>
                    (JobHandle dependency, int innerLoopBatchCount, TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecutionForKey<THitMessage>
                {
                    return new HitMessageApplyJobForKey<TJobInnerExecution, THitMessage>
                    {
                        MessageHolder = this.messageHolder,
                        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                        InnerJob = execution,
                    };
                    //return job.Schedule(this.keyEntities, innerLoopBatchCount, dependency);
                }
                //=> dependency;

                public HitMessageApplyJobForKey<TJobInnerExecution> ToJob<TJobInnerExecution>(TJobInnerExecution execution)
                    where TJobInnerExecution : struct, IApplyJobExecutionForKey
                {
                    return new HitMessageApplyJobForKey<TJobInnerExecution>
                    {
                        MessageHolder = this.messageHolder,
                        KeyEntities = this.keyEntities.AsDeferredJobArray(),
                        InnerJob = execution,
                    };
                }

                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                //public JobHandle ScheduleClear(JobHandle dependency) =>
                //    new clearJob
                //    {
                //        keyEntities = this.keyEntities,
                //        messageHolder = this.messageHolder,
                //        uniqueKeys = this.uniqueKeys,
                //    }
                //    .Schedule(dependency);

                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandle ScheduleDispose(JobHandle dependency)
                {
                    var dep1 = this.keyEntities.Dispose(dependency);
                    var dep2 = this.messageHolder.Dispose(dependency);
                    var dep3 = this.uniqueKeys.Dispose(dependency);
                    return JobHandle.CombineDependencies(dep1, dep2, dep3);
                    //var dep0 = dependency;
                    //var dep1 = this.keyEntities.Dispose(dep0);
                    //var dep2 = this.messageHolder.Dispose(dep1);
                    //var dep3 = this.uniqueKeys.Dispose(dep2);
                    //return dep3;
                }


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


            ///// <summary>
            ///// 
            ///// </summary>
            //[BurstCompile]
            //struct disposeJob : IJob
            //{
            //    //[DeallocateOnJobCompletion]
            //    public NativeList<Entity> keyEntities;
            //    //[DeallocateOnJobCompletion]
            //    public NativeMultiHashMap<Entity, THitMessage> messageHolder;
            //    //[DeallocateOnJobCompletion]
            //    public NativeHashSet<Entity> uniqueKeys;

            //    public void Execute()
            //    {
            //        //this.keyEntities.Dispose();
            //        //this.messageHolder.Dispose();
            //        //this.uniqueKeys.Dispose();
            //    }
            //}


            ///// <summary>
            ///// 
            ///// </summary>
            //[BurstCompile]
            //struct HitMessageApplyJobForEach<TJobInnerExecution> : IJobParallelForDefer
            //    where TJobInnerExecution : struct, IApplyJobExecutionForEach
            //{
            //    [ReadOnly]
            //    public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

            //    [ReadOnly]
            //    public NativeArray<Entity> KeyEntities;


            //    //[NativeDisableParallelForRestriction]
            //    //[NativeDisableContainerSafetyRestriction]
            //    public TJobInnerExecution InnerJob;


            //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
            //    public void Execute(int index)
            //    {
            //        var key = this.KeyEntities[index];
            //        var msgs = this.MessageHolder.GetValuesForKey(key);

            //        foreach (var msg in msgs)
            //        {
            //            this.InnerJob.Execute(index, key, msg);
            //        }
            //    }
            //}


            /// <summary>
            /// 
            /// </summary>
            [BurstCompile]
            public struct HitMessageApplyJobForKey<TJobInnerExecution> : IJobParallelForDefer
                where TJobInnerExecution : struct, IApplyJobExecutionForKey
            {
                [ReadOnly]
                public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

                [ReadOnly]
                public NativeArray<Entity> KeyEntities;


                //[NativeDisableParallelForRestriction]
                //[NativeDisableContainerSafetyRestriction]
                public TJobInnerExecution InnerJob;


                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Execute(int index)
                {
                    var key = this.KeyEntities[index];
                    var msgs = this.MessageHolder.GetValuesForKey(key);

                    //this.InnerJob.Execute(index, key, msgs);
                }
            }
            ///// <summary>
            ///// 
            ///// </summary>
            //[BurstCompile]
            //public struct HitMessageApplyDataForKey<TJobInnerExecution>
            //    where TJobInnerExecution : struct, IApplyExecutionForKey
            //{
            //    [ReadOnly]
            //    public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

            //    [ReadOnly]
            //    public NativeArray<Entity> KeyEntities;


            //    //[NativeDisableParallelForRestriction]
            //    //[NativeDisableContainerSafetyRestriction]
            //    public TJobInnerExecution InnerJob;


            //    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //    public void Execute(int index)
            //    {
            //        var key = this.KeyEntities[index];
            //        var msgs = this.MessageHolder.GetValuesForKey(key);

            //        this.InnerJob.Execute(index, key, msgs);
            //    }
            //}
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public interface IApplyJobExecutionForKey<THitMessage>
        where THitMessage : struct, IHitMessage
    {
        void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, THitMessage>.Enumerator hitMessages);
    }
    /// <summary>
    /// 
    /// </summary>
    [BurstCompile]
    public struct HitMessageApplyJobForKey<TJobInnerExecution, THitMessage> : IJobParallelForDefer
        where TJobInnerExecution : struct, IApplyJobExecutionForKey<THitMessage>
        where THitMessage : struct, IHitMessage
    {
        [ReadOnly]
        public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

        [ReadOnly]
        public NativeArray<Entity> KeyEntities;


        //[NativeDisableParallelForRestriction]
        //[NativeDisableContainerSafetyRestriction]
        public TJobInnerExecution InnerJob;


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int index)
        {
            var key = this.KeyEntities[index];
            var msgs = this.MessageHolder.GetValuesForKey(key);

            this.InnerJob.Execute(index, key, msgs);
        }
    }
}


