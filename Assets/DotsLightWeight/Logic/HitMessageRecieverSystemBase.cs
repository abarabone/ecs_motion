using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace Abarabone.Character.Action
{

    public struct HitMessageUnit
    {
        public float damage;
    }

    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public abstract class HitMessageSystemBase<THitMessage> : SystemBase
        where THitMessage : struct
    {



        NativeList<Entity> keyEntities;
        NativeMultiHashMap<Entity, THitMessage> messageHolder;
        ParallelWriter writer;

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

        public ParallelWriter GetParallelWriter() => this.writer;

        


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

            this.Dependency = new HitMessageApplyJob<THitMessage>
            {
                MessageHolder = this.messageHolder,
                KeyEntities = this.keyEntities.AsDeferredJobArray(),
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
    }

    struct HitMessageApplyJob<THitMessage> : IJobParallelForDefer
        where THitMessage : struct
    {
        [ReadOnly]
        public NativeMultiHashMap<Entity, THitMessage> MessageHolder;

        [ReadOnly]
        public NativeArray<Entity> KeyEntities;

        public void Execute(int index)
        {

        }
    }
}
