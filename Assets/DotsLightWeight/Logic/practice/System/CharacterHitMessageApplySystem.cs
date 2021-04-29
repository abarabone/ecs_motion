using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;

namespace DotsLite.Character
{
    using DotsLite.Dependency;

    public struct HitMessage : IHitMessage
    {
        public float3 Position;
        public float3 Normale;
        public float3 Force;
        public float Damage;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class CharacterHitMessageApplySystem : DependencyAccessableSystemBase, HitMessage<HitMessage>.IRecievable
    {


        public HitMessage<HitMessage>.Reciever Reciever { get; private set; }

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<HitMessage>.Reciever(10000);
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();

            
            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Dependency = new JobExecution
            {
                Cmd = cmd,
                CurrentTime = (float)this.Time.ElapsedTime,
                //Rotations = rots,
                //Positions = poss,
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<HitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter Cmd;
            public float CurrentTime;

            //[ReadOnly]
            //public ComponentDataFromEntity<Rotation> Rotations;
            //[ReadOnly]
            //public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, HitMessage>.Enumerator msgs)
            {


                this.Cmd.AddComponent(index, targetEntity,
                    new AntAction.DamageState
                    {
                        EntTime = this.CurrentTime + 0.3f,
                    }
                );


                foreach (var msg in msgs)
                {

                }

            }
        }
    }

}
