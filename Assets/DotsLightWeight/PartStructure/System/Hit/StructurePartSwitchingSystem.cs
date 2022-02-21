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

            this.Dependency = new JobExecution
            {
                cmd = cmdScope.CommandBuffer.AsParallelWriter(),

                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),
                compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
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
            [ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            [ReadOnly] public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
            [ReadOnly] public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                if (this.compoundTags.HasComponent(mainEntity)) return;
                //if (hitMessages.Current.PartId == -1) return;


                var destruction = this.destructions[mainEntity];


                var partLength = this.lengths[mainEntity].TotalPartLength;
                using var targetParts = makeTargetPartList_(ref destruction, hitMessages, partLength);

                destructAndCreateDebris_(index, in targetParts);


                this.destructions[mainEntity] = destruction;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static NativeHashSet<Entity> makeTargetPartList_(
                ref Main.PartDestructionData destruction,
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int partLength)
            {
                var targetParts = new NativeHashSet<Entity>(partLength, Allocator.Temp);

                foreach (var msg in hitMessages)
                {
                    if (destruction.IsDestroyed(msg.PartId)) continue;

                    targetParts.Add(msg.ColliderEntity);

                    destruction.SetDestroyed(msg.PartId);
                }

                return targetParts;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void destructAndCreateDebris_(int uniqueIndex, in NativeHashSet<Entity> targetParts)
            {
                foreach (var part in targetParts)
                {
                    //hidePart_(uniqueIndex, part);
                    destroyPart_(uniqueIndex, part);
                    createDebris_(uniqueIndex, part);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void hidePart_(int uniqueIndex, Entity part)
            {
                this.cmd.AddComponent<Disabled>(uniqueIndex, part);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void destroyPart_(int uniqueIndex, Entity part)
            {
                this.cmd.DestroyEntity(uniqueIndex, part);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void createDebris_(int uniqueIndex, Entity part)
            {
                var debrisPrefab = this.Prefabs[part].DebrisPrefab;
                var rot = this.Rotations[part];
                var pos = this.Positions[part];

                var ent = this.cmd.Instantiate(uniqueIndex, debrisPrefab);
                this.cmd.SetComponent(uniqueIndex, ent, rot);
                this.cmd.SetComponent(uniqueIndex, ent, pos);
            }

        }


    }
}
