using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace Abarabone.Dependency
{





    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public abstract class HitMessageSystemBase<THitMessage, TJobInnerExecution> : SystemBase
        where THitMessage : struct
        where TJobInnerExecution : struct, IHitMessageApplyJobExecution<THitMessage>
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
            public JobHandle ExecuteJobSchedule(JobHandle dependency, TJobInnerExecution execution) =>
                new HitMessageApplyJob
                {
                    MessageHolder = this.messageHolder,
                    KeyEntities = this.keyEntities.AsDeferredJobArray(),
                    InnerJob = execution,
                }
                .Schedule(this.keyEntities, 8, dependency);


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

        struct clearJob : IJob
        {
            public HitMessageHolder holder;

            public void Execute()
            {
                this.holder.Clear();
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

            this.Dependency = this.hitMessageHolder
                .ExecuteJobSchedule(this.Dependency, this.buildJobInnerExecution());

            this.Dependency = new clearJob
            {
                holder = this.hitMessageHolder,
            }
            .Schedule(this.Dependency);
            // 自動生成はできないみたい、ジェネリクスのせいか abstract のせいかはわからん
            //var holder = this.hitMessageHolder;
            //this.Job
            //    .WithCode(
            //        () =>
            //        {
            //            holder.Clear();
            //        }
            //    )
            //    .Schedule();
        }

        protected abstract TJobInnerExecution buildJobInnerExecution();


    }



}
