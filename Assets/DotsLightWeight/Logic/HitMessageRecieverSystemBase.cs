using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace Abarabone.Common
{

    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public abstract class HitMessageSystemBase<THitMessage, TInnerJob> : SystemBase
        where THitMessage : struct
        where TInnerJob : IHitMessageApplyJobExecution<THitMessage>
    {


        HitMessageHolder hitMessageHolder;


        public ParallelWriter GetParallelWriter() => hitMessageHolder.AsParallelWriter();






        public struct HitMessageHolder : IDisposable
        {
            NativeList<Entity> keyEntities;
            NativeMultiHashMap<Entity, THitMessage> messageHolder;
            //ParallelWriter writer;


            public HitMessageHolder(int capacity)
            {
                this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
                this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ParallelWriter AsParallelWriter() =>
                new ParallelWriter(ref this.keyEntities, ref this.messageHolder);


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JobHandle ExecuteJob(JobHandle dependency, IHitMessageApplyJobExecution<THitMessage> innerJob)
            {

                return new HitMessageApplyJob<THitMessage, IHitMessageApplyJobExecution<THitMessage>>
                {
                    MessageHolder = this.messageHolder,
                    KeyEntities = this.keyEntities.AsDeferredJobArray(),
                    InnerJob = innerJob,
                }
                .Schedule(this.keyEntities, 8, dependency);

            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                this.keyEntities.Clear();
                this.messageHolder.Clear();
            }


            public void Dispose()
            {
                this.keyEntities.Dispose();
                this.messageHolder.Dispose();
            }
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







        protected override void OnCreate()
        {
            base.OnCreate();

            var capacity = 10000;//
            this.hitMessageHolder = new HitMessageHolder(capacity);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.hitMessageHolder.Dispose();
        }

        protected override void OnUpdate()
        {

            this.Dependency = this.hitMessageHolder.ExecuteJob(this.Dependency, this.buildJob());

            var holder = this.hitMessageHolder;
            this.Job
                .WithCode(
                    () =>
                    {
                        holder.Clear();
                    }
                )
                .Schedule();
        }

        protected abstract IHitMessageApplyJobExecution<THitMessage> buildJob();

    }



    public interface IHitMessageApplyJobExecution<THitMessage>
        where THitMessage : struct
    {
        void Execute(int index, Entity targetEntity, THitMessage hitMessage);
    }

    public struct HitMessageApplyJob<THitMessage, TApplier> : IJobParallelForDefer
        where THitMessage : struct
        where TApplier : IHitMessageApplyJobExecution<THitMessage>
    {
        [ReadOnly]
        public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

        [ReadOnly]
        public NativeArray<Entity> KeyEntities;


        public IHitMessageApplyJobExecution<THitMessage> InnerJob;


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
