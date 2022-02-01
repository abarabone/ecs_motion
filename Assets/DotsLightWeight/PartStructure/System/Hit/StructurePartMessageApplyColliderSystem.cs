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

using Collider = Unity.Physics.Collider;

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
    public class StructurePartMessageApplyColliderSystem : DependencyAccessableSystemBase
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

                //destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),
                //lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),
                //Prefabs = this.GetComponentDataFromEntity<Part.DebrisPrefabData>(isReadOnly: true),
                //Rotations = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true),
                //Positions = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),

                //binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),
                //parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
                //linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),

                //updateCollider = new UpdateCollider
                //{
                //    cols = this.GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true),
                //    infos = this.GetComponentDataFromEntity<PartBone.PartInfoData>(isReadOnly: true),
                //    ress = this.GetBufferFromEntity<PartBone.PartDestructionResourceData>(isReadOnly: true),
                //},
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter cmd;


            [ReadOnly] public ComponentDataFromEntity<Main.PartDestructionData> destructions;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            //// パーツ用
            //[ReadOnly] public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
            //[ReadOnly] public ComponentDataFromEntity<Rotation> Rotations;
            //[ReadOnly] public ComponentDataFromEntity<Translation> Positions;


            public ComponentDataFromEntity<PhysicsCollider> colliders;

            //[ReadOnly]
            //public ComponentDataFromEntity<PartBone.LengthData> boneLengths;

            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
            public BufferFromEntity<PartBone.PartColliderResourceData> boneResourceBuffers;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                var targetBones = new UnsafeHashSet<Entity>(this.lengths[mainEntity].TotalPartLength, Allocator.Temp);

                foreach (var boneEntity in targetBones)
                {
                    this.colliders[boneEntity] = new PhysicsCollider
                    {
                        //Value = buildBoneCollider(in this.destructions[mainEntity], ref this.buildBoneCollider.)
                    };
                }

                targetBones.Dispose();
            }

            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe BlobAssetReference<Collider> buildBoneCollider(
                in Main.PartDestructionData destructions,
                ref DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer,
                ref DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer)
            {
                for (var i = 0; i < boneInfoBuffer.Length; i++)
                {
                    var partid = boneInfoBuffer[i].PartId;
                    if (!destructions.IsDestroyed(partid))
                    {
                        boneColliderBuffer.RemoveAtSwapBack(i);
                        boneInfoBuffer.RemoveAtSwapBack(i);
                    }
                }

                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
                return CompoundCollider.Create(na);
            }

        }
    }
}
