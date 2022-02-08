using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;

    using DotsLite.Utility.Log.NoShow;


    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
    [UpdateAfter(typeof(StructureEnvelopeMessageApplySystem))]
    [UpdateBefore(typeof(StructurePartBoneUpdateColliderSystem))]
    [UpdateBefore(typeof(StructurePartSwitchingSystem))]
    [UpdateBefore(typeof(StructurePartMessageFreeJobSystem))]
    public class StructurePartMessageApplySystem : DependencyAccessableSystemBase
    {


        StructurePartMessageAllocationSystem allocationSystem;

        BarrierDependency.Sender freedep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructurePartMessageAllocationSystem>();
            this.freedep = BarrierDependency.Sender.Create<StructurePartMessageFreeJobSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var freeScope = this.freedep.WithDependencyScope();
            

            this.Dependency = new JobExecution
            {

            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {



            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                //wakeupMain_(index, mainEntity);
                //applyDamgeToMain_();
            }



            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //void wakeupMain_(int uniqueIndex, Entity mainEntity)
            //{
            //    //this.Cmd.AddComponent(index, targetEntity, new Unity.Physics.PhysicsVelocity { });
            //    var binder = this.binderLinks[mainEntity];
            //    this.cmd.ChangeComponentsToWakeUp(mainEntity, uniqueIndex, binder, this.parts, this.linkedGroups);
            //}

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //static void applyDamgeToMain_()
            //{
            //    var damage = 0.0f;
            //    var force = float3.zero;
            //}


        }


    }
}
