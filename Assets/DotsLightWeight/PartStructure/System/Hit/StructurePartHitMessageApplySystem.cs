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


    public struct PartHitMessage : IHitMessage
    {
        public int PartId;
        public Entity PartEntity;
        public float3 Position;
        public float3 Normal;
    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class StructurePartHitMessageAllocSystem : SystemBase
    {
        StructurePartHitMessageApplySystem sys;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.sys = this.World.GetOrCreateSystem<StructurePartHitMessageApplySystem>();
        }
        protected override void OnUpdate()
        {
            this.sys.Reciever.Alloc(10000, Allocator.TempJob);
        }
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(StructureEnvelopeHitMessageApplySystem))]
    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
    public class StructurePartHitMessageApplySystem : DependencyAccessableSystemBase, HitMessage<PartHitMessage>.IRecievable
    {


        public HitMessage<PartHitMessage>.Reciever Reciever { get; private set; }

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<PartHitMessage>.Reciever();// 10000);
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

            this.Dependency = new JobExecution
            {
                cmd = cmd,

                infos = this.GetComponentDataFromEntity<Main.PartInfoData>(isReadOnly: true),
                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),
                Prefabs = this.GetComponentDataFromEntity<Part.DebrisPrefabData>(isReadOnly: true),
                Rotations = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true),
                Positions = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),

                //binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),
                //parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
                //linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),

                updateCollider = new UpdateCollider
                {
                    cmd = cmd,

                    cols = this.GetComponentDataFromEntity<PhysicsCollider>(),
                    infos = this.GetComponentDataFromEntity<Main.PartInfoData>(isReadOnly: true),
                    ress = this.GetBufferFromEntity<Main.PartDestructionResourceData>(isReadOnly: true),
                },
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency);

            this.Dependency = this.Reciever.Holder.ScheduleDispose(this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter cmd;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Main.PartDestructionData> destructions;
            [ReadOnly] public ComponentDataFromEntity<Main.PartInfoData> infos;

            // パーツ用
            [ReadOnly] public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
            [ReadOnly] public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly] public ComponentDataFromEntity<Translation> Positions;

            //// メイン用
            //[ReadOnly] public ComponentDataFromEntity<Structure.Main.BinderLinkData> binderLinks;
            //[ReadOnly] public ComponentDataFromEntity<Structure.Part.PartData> parts;
            //[ReadOnly] public BufferFromEntity<LinkedEntityGroup> linkedGroups;


            public UpdateCollider updateCollider;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                var targetParts = new UnsafeHashSet<Entity>(this.infos[mainEntity].PartLength, Allocator.Temp);

                //wakeupMain_(index, mainEntity);
                //applyDamgeToMain_();

                applyDestructions_(mainEntity, hitMessages, ref targetParts);
                destroyPartAndCreateDebris_(index, in targetParts);

                this.updateCollider.Execute(mainEntity, hitMessages);

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
                    if (!this.Positions.HasComponent(msg.PartEntity)) continue;

                    destruction.SetDestroyed(msg.PartId);

                    targetParts.Add(msg.PartEntity);
                }

                this.destructions[mainEntity] = destruction;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void destroyPartAndCreateDebris_(int uniqueIndex, in UnsafeHashSet<Entity> targetParts)
            {
                foreach (var part in targetParts)
                {
                    //_._log($"{part}");
                    var prefab = this.Prefabs[part].DebrisPrefab;
                    var rot = this.Rotations[part];
                    var pos = this.Positions[part];

                    createDebris_(this.cmd, uniqueIndex, prefab, rot, pos);
                    destroyPart_(this.cmd, uniqueIndex, part);
                }
                return;


                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void createDebris_(
                    EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
                    Rotation rot_, Translation pos_)
                {
                    var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
                    cmd_.SetComponent(uniqueIndex_, ent, rot_);
                    cmd_.SetComponent(uniqueIndex_, ent, pos_);
                }

                //[MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void destroyPart_(
                    EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
                {
                    cmd_.DestroyEntity(uniqueIndex_, part_);
                }
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

        [BurstCompile]
        public struct UpdateCollider
        {

            public EntityCommandBuffer.ParallelWriter cmd;


            public ComponentDataFromEntity<PhysicsCollider> cols;

            [ReadOnly]
            public ComponentDataFromEntity<Main.PartInfoData> infos;

            [ReadOnly]
            public BufferFromEntity<Main.PartDestructionResourceData> ress;


            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe void Execute(
                Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                var info = this.infos[mainEntity];

                var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
                    info.LivePartLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                var buffer = this.ress[mainEntity];
                for (var i = 0; i < info.LivePartLength; i++)
                {
                    dst[i] = buffer[i].ColliderInstance;
                }

                this.cols[mainEntity] = new PhysicsCollider
                {
                    Value = CompoundCollider.Create(dst),
                };
                dst.Dispose();
            }
        }

    }
}
