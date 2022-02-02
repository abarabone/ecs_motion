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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateAfter(typeof(StructurePartMessageAllocationSystem))]
    [UpdateAfter(typeof(StructureEnvelopeMessageApplySystem))]
    [UpdateBefore(typeof(StructurePartMessageFreeJobSystem))]
    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
    public class StructurePartSwitchingSystem : DependencyAccessableSystemBase
    {


        StructurePartMessageAllocationSystem allocationSystem;

        BarrierDependency.Sender freedep;
        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructurePartMessageAllocationSystem>();
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.freedep = BarrierDependency.Sender.Create<StructurePartMessageFreeJobSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var freeScope = this.freedep.WithDependencyScope();
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            this.Dependency = new JobExecution
            {
                cmd = cmd,

                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),
                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),
                Prefabs = this.GetComponentDataFromEntity<Part.DebrisPrefabData>(isReadOnly: true),
                Rotations = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true),
                Positions = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter cmd;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Main.PartDestructionData> destructions;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            // パーツ用
            [ReadOnly] public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
            [ReadOnly] public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                var targetParts = new UnsafeHashSet<Entity>(this.lengths[mainEntity].TotalPartLength, Allocator.Temp);

                //wakeupMain_(index, mainEntity);
                //applyDamgeToMain_();
                Debug.Log($"child id {hitMessages.Current.ColliderChildId}");

                applyDestructions_(mainEntity, hitMessages, ref targetParts);
                destroyPartAndCreateDebris_(index, in targetParts);

                //this.updateCollider.Execute(mainEntity, hitMessages);

                targetParts.Dispose();
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe void applyDestructions_(
                Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages,
                ref UnsafeHashSet<Entity> targetParts)
            {
                var destruction = this.destructions[mainEntity];

                foreach (var msg in hitMessages)
                {
                    //_._log($"{msg.PartId} {(uint)(destruction.Destructions[msg.PartId >> 5] & (uint)(1 << (msg.PartId & 0b11111)))} {destruction.IsDestroyed(msg.PartId)} {this.Prefabs.HasComponent(msg.PartEntity)}");
                    if (destruction.IsDestroyed(msg.PartId)) continue;
                    if (!this.Positions.HasComponent(msg.ColliderEntity)) continue;

                    targetParts.Add(msg.ColliderEntity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void destroyPartAndCreateDebris_(int uniqueIndex, in UnsafeHashSet<Entity> targetParts)
            {
                foreach (var part in targetParts)
                {
                    hideOrShowPart_(this.cmd, uniqueIndex, part);
                    //destroyPart_(this.cmd, uniqueIndex, part);
                }
                return;


                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void hideOrShowPart_(
                    EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
                {
                    cmd_.AddComponent<Disabled>(uniqueIndex_, part_);
                }

                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void destroyPart_(
                    EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
                {
                    cmd_.DestroyEntity(uniqueIndex_, part_);
                }
            }

        }


    }
}
