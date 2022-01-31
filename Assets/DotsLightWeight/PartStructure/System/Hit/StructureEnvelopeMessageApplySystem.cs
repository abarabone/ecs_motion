﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;


    // parts の destroy とここでの wakeup components 着脱が競合する可能性もはらんでいるかも？？ em 専用 system をつくるべきなのかなぁ
    // ↑システムの順序をちゃんとしたら大丈夫だった！
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateAfter(typeof(StructureEnvelopeMessageAllocationSystem))]
    [UpdateBefore(typeof(StructureEnvelopeMessageFreeJobSystem))]
    public class StructureEnvelopeMessageApplySystem : DependencyAccessableSystemBase
    {


        StructureEnvelopeMessageAllocationSystem allocationSystem;

        BarrierDependency.Sender barcopydep;
        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructureEnvelopeMessageAllocationSystem>();
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.barcopydep = BarrierDependency.Sender.Create<StructureEnvelopeMessageFreeJobSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var barcopyScope = this.barcopydep.WithDependencyScope();
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();


            this.Dependency = new JobExecution
            {
                Cmd = cmd,
                binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),
                parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
                linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }



        [BurstCompile]
        public struct JobExecution : HitMessage<EnvelopeHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter Cmd;
            //public float CurrentTime;
            [ReadOnly] public ComponentDataFromEntity<Structure.Main.BinderLinkData> binderLinks;
            [ReadOnly] public ComponentDataFromEntity<Structure.Part.PartData> parts;
            [ReadOnly] public BufferFromEntity<LinkedEntityGroup> linkedGroups;

            [BurstCompile]
            public void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, EnvelopeHitMessage>.Enumerator msgs)
            {

                var damage = 0.0f;
                var force = float3.zero;

                //var corps = this.Corpss[targetEntity].Corps;


                //foreach (var msg in msgs)
                //{

                //}

                //this.Cmd.AddComponent(index, targetEntity, new Unity.Physics.PhysicsVelocity { });
                var binder = this.binderLinks[targetEntity];
                this.Cmd.ChangeComponentsToWakeUp(targetEntity, index, binder, this.parts, this.linkedGroups);
            }
        }
    }

}