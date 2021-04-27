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

namespace Abarabone.Character
{
    using Abarabone.Dependency;

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
                Rotations = rots,
                Positions = poss,
            }
            .ScheduleParallelEach(this.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<HitMessage>.IApplyJobExecutionForEach
        {

            public EntityCommandBuffer.ParallelWriter Cmd;

            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public void Execute(int index, Entity targetEntity, HitMessage hitMessage)
            {



            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void createDebris_
                (
                    EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
                    Rotation rot_, Translation pos_
                )
            {

                var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
                cmd_.SetComponent(uniqueIndex_, ent, rot_);
                cmd_.SetComponent(uniqueIndex_, ent, pos_);

            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void destroyPart_
                (EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
            {
                cmd_.DestroyEntity(uniqueIndex_, part_);
            }
        }
    }

}
