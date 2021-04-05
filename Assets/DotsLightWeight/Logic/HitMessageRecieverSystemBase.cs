using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Abarabone.Common
{



    public struct HitMessageReciever<THitMessage, TJobInnerExecution> : IDisposable
        where THitMessage : struct
        where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
    {

        HitMessageHolder<THitMessage, TJobInnerExecution> holder;

        DependencyWaiter waiter;


        public HitMessageReciever(int capacity, int maxDependsSystem = 32)
        {
            this.holder = new HitMessageHolder<THitMessage, TJobInnerExecution>(capacity);
            this.waiter = new DependencyWaiter(maxDependsSystem);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HitMessageHolder<THitMessage, TJobInnerExecution>.ParallelWriter AsParallelWriter() =>
            this.holder.AsParallelWriter();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddDependencyBeforeHitApply(JobHandle jobHandle) => this.waiter.AddDependencyBeforeHitApply(jobHandle);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Schedule(JobHandle dependency, TJobInnerExecution execution)
        {
            this.waiter.WaitAllDependencyJobs();

            var dep0 = dependency;
            var dep1 = this.holder.ExecutionAndSchedule(dep0, execution);
            var dep2 = this.holder.ClearAndSchedule(dep1);

            return dep2;
        }


        public void Dispose()
        {
            this.holder.Dispose();
            this.waiter.Dispose();
        }
    }


    public struct HitMessageHolder<THitMessage, TJobInnerExecution> : IDisposable
        where THitMessage : struct
        where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
    {

        NativeList<Entity> keyEntities;
        NativeMultiHashMap<Entity, THitMessage> messageHolder;


        public HitMessageHolder(int capacity)
        {
            this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
            this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ParallelWriter AsParallelWriter() => new ParallelWriter(ref this.keyEntities, ref this.messageHolder);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle ExecutionAndSchedule(JobHandle dependency, TJobInnerExecution execution) =>
            new HitMessageApplyJob<THitMessage, TJobInnerExecution>
            {
                MessageHolder = this.messageHolder,
                KeyEntities = this.keyEntities.AsDeferredJobArray(),
                InnerJob = execution,
            }
            .Schedule(this.keyEntities, 8, dependency);
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle ClearAndSchedule(JobHandle dependency) =>
            new clearJob2<THitMessage, TJobInnerExecution>
            {
                keyEntities = this.keyEntities,
                messageHolder = this.messageHolder,
            }
            .Schedule(dependency);


        public void Dispose()
        {
            this.keyEntities.Dispose();
            this.messageHolder.Dispose();
        }


        public struct ParallelWriter
        {
            NativeList<Entity>.ParallelWriter nl;
            NativeMultiHashMap<Entity, THitMessage>.ParallelWriter hm;

            public ParallelWriter(ref NativeList<Entity> nl, ref NativeMultiHashMap<Entity, THitMessage> hm)
            {
                this.nl = nl.AsParallelWriter();
                this.hm = hm.AsParallelWriter();
            }

            public void Add(Entity entity, THitMessage hitMessage)
            {
                this.nl.AddNoResize(entity);
                this.hm.Add(entity, hitMessage);
            }
        }
    }


    struct DependencyWaiter : IDisposable
    {

        NativeList<JobHandle> dependencyJobHandles;


        public DependencyWaiter(int capacity)
        {
            this.dependencyJobHandles = new NativeList<JobHandle>(capacity, Allocator.Persistent);
        }


        public void AddDependencyBeforeHitApply(JobHandle jobHandle) => this.dependencyJobHandles.Add(jobHandle);


        public void WaitAllDependencyJobs()
        {
            JobHandle.CombineDependencies(this.dependencyJobHandles).Complete();

            this.dependencyJobHandles.Clear();
        }


        public void Dispose() => this.dependencyJobHandles.Dispose();
    }


    [BurstCompile]
    struct clearJob2<THitMessage, TJobInnerExecution> : IJob
        where THitMessage : struct
        where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
    {
        public NativeList<Entity> keyEntities;
        public NativeMultiHashMap<Entity, THitMessage> messageHolder;

        public void Execute()
        {
            this.keyEntities.Clear();
            this.messageHolder.Clear();
        }
    }








    ////[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    //public abstract class HitMessageSystemBase<THitMessage, TJobInnerExecution> : SystemBase
    //    where THitMessage : struct
    //    where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
    //{


    //    HitMessageHolder hitMessageHolder;


    //    public ParallelWriter GetParallelWriter() => hitMessageHolder.AsParallelWriter();






    //    public struct HitMessageHolder : IDisposable
    //    {
    //        NativeList<Entity> keyEntities;
    //        NativeMultiHashMap<Entity, THitMessage> messageHolder;
    //        //ParallelWriter writer;


    //        public HitMessageHolder(int capacity)
    //        {
    //            this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
    //            this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);
    //        }


    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        public ParallelWriter AsParallelWriter() =>
    //            new ParallelWriter(ref this.keyEntities, ref this.messageHolder);


    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        public JobHandle ExecuteJobSchedule(JobHandle dependency, TJobInnerExecution execution) =>
    //            new HitMessageApplyJob<THitMessage, TJobInnerExecution>
    //            {
    //                MessageHolder = this.messageHolder,
    //                KeyEntities = this.keyEntities.AsDeferredJobArray(),
    //                InnerJob = execution,
    //            }
    //            .Schedule(this.keyEntities, 8, dependency);


    //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //        public void Clear()
    //        {
    //            this.keyEntities.Clear();
    //            this.messageHolder.Clear();
    //        }


    //        public void Dispose()
    //        {
    //            this.keyEntities.Dispose();
    //            this.messageHolder.Dispose();
    //        }
    //    }

    //    public struct ParallelWriter
    //    {
    //        NativeList<Entity>.ParallelWriter nl;
    //        NativeMultiHashMap<Entity, THitMessage>.ParallelWriter hm;

    //        public ParallelWriter(ref NativeList<Entity> nl, ref NativeMultiHashMap<Entity, THitMessage> hm)
    //        {
    //            this.nl = nl.AsParallelWriter();
    //            this.hm = hm.AsParallelWriter();
    //        }

    //        public void Add(Entity entity, THitMessage hitMessage)
    //        {
    //            this.nl.AddNoResize(entity);
    //            this.hm.Add(entity, hitMessage);
    //        }
    //    }







    //    protected override void OnCreate()
    //    {
    //        base.OnCreate();

    //        var capacity = 10000;//
    //        this.hitMessageHolder = new HitMessageHolder(capacity);
    //    }

    //    protected override void OnDestroy()
    //    {
    //        base.OnDestroy();

    //        this.hitMessageHolder.Dispose();
    //    }

    //    protected override void OnUpdate()
    //    {

    //        this.Dependency = this.hitMessageHolder
    //            .ExecuteJobSchedule(this.Dependency, this.buildJobInnerExecution());

    //        this.Dependency = new clearJob<THitMessage, TJobInnerExecution>
    //        {
    //            holder = this.hitMessageHolder,
    //        }
    //        .Schedule(this.Dependency);
    //        // 自動生成はできないみたい、ジェネリクスのせいか abstract のせいかはわからん
    //        //var holder = this.hitMessageHolder;
    //        //this.Job
    //        //    .WithCode(
    //        //        () =>
    //        //        {
    //        //            holder.Clear();
    //        //        }
    //        //    )
    //        //    .Schedule();
    //    }

    //    protected abstract TJobInnerExecution buildJobInnerExecution();

    //}



    //struct clearJob<THitMessage, TInnerJob> : IJob
    //    where THitMessage : struct
    //    where TInnerJob : struct, IHitMessageApplyJobExecution<THitMessage>
    //{
    //    public HitMessageSystemBase<THitMessage, TInnerJob>.HitMessageHolder holder;

    //    public void Execute()
    //    {
    //        this.holder.Clear();
    //    }
    //}



    public interface IHitMessageApplyJobExecution<THitMessage>
        where THitMessage : struct
    {
        void Execute(int index, Entity targetEntity, THitMessage hitMessage);
    }


    [BurstCompile]
    public struct HitMessageApplyJob<THitMessage, TApplier> : IJobParallelForDefer
        where THitMessage : struct
        where TApplier : struct, IHitMessageApplyJobExecution<THitMessage>
    {
        [ReadOnly]
        public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

        [ReadOnly]
        public NativeArray<Entity> KeyEntities;


        public TApplier InnerJob;


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
