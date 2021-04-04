using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace Abarabone.Character.Action
{

    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public abstract class HitMessageSystemBase<THitMessage, TInnerJob> : SystemBase
        where THitMessage : struct
        where TInnerJob : IHitMessageApplyJobExecution<THitMessage>
    {



        struct HitMessageHolder
        {
            NativeList<Entity> keyEntities;
            NativeMultiHashMap<Entity, THitMessage> messageHolder;
            //ParallelWriter writer;


            public ParallelWriter GetParallelWriter() =>
                new ParallelWriter(ref this.keyEntities, ref this.messageHolder);

            public void Clear()
            {
                this.keyEntities.Clear();
                this.messageHolder.Clear();
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


        public ParallelWriter GetParallelWriter() => new ParallelWriter(ref this.keyEntities, ref this.messageHolder);//this.writer;





        protected override void OnCreate()
        {
            base.OnCreate();

            var capacity = 10000;
            this.keyEntities = new NativeList<Entity>(capacity, Allocator.Persistent);
            this.messageHolder = new NativeMultiHashMap<Entity, THitMessage>(capacity, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.keyEntities.Dispose();
            this.messageHolder.Dispose();
        }

        protected override void OnUpdate()
        {

            this.Dependency = new HitMessageApplyJob<THitMessage, IHitMessageApplyJobExecution<THitMessage>>
            {
                MessageHolder = this.messageHolder,
                KeyEntities = this.keyEntities.AsDeferredJobArray(),
                InnerJob = buildJob(),
            }
            .Schedule(this.keyEntities, 8, this.Dependency);


            var ke = this.keyEntities;
            var mh = this.messageHolder;
            this.Job
                .WithCode(
                    () =>
                    {
                        ke.Clear();
                        mh.Clear();
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

    struct HitMessageApplyJob<THitMessage, TApplier> : IJobParallelForDefer
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
