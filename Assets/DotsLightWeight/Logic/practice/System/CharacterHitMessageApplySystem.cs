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
    using DotsLite.Utilities;

    public struct HitMessage : IHitMessage
    {
        public float3 Position;
        public float3 Normale;
        public float3 Force;
        public float Damage;
        public Targeting.Corps TargetCorps;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
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

            
            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            //var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            //var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            //var corpss = this.GetComponentDataFromEntity<CorpsGroup.TargetData>(isReadOnly: true);

            this.Dependency = new JobExecution
            {
                Cmd = cmd,
                CurrentTime = (float)this.Time.ElapsedTime,
                //Corpss = corpss,
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
            //public ComponentDataFromEntity<CorpsGroup.TargetData> Corpss;

            //[ReadOnly]
            //public ComponentDataFromEntity<Rotation> Rotations;
            //[ReadOnly]
            //public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, HitMessage>.Enumerator msgs)
            {

                var damage = 0.0f;
                var force = float3.zero;

                //var corps = this.Corpss[targetEntity].Corps;


                foreach (var msg in msgs)
                {
                    damage += msg.Damage;
                    force += msg.Force;
                }


                this.Cmd.AddComponent(index, targetEntity,
                    new CharacterAction.DamageState
                    {
                        EndTime = this.CurrentTime + 0.3f,
                        Damage = damage,
                        DamageForce = force.As_float4(),
                    }
                );

            }
        }
    }

}
