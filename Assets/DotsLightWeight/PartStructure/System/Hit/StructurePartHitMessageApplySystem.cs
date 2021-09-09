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
using UnityEngine;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;

    public struct PartHitMessage : IHitMessage
    {
        public int PartId;
        public Entity PartEntity;
        public float3 Position;
        public float3 Normal;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class StructurePartHitMessageApplySystem : DependencyAccessableSystemBase, HitMessage<PartHitMessage>.IRecievable
    {


        public HitMessage<PartHitMessage>.Reciever Reciever { get; private set; }

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<PartHitMessage>.Reciever(10000);
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

            var destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>();
            var prefabs = this.GetComponentDataFromEntity<Part.DebrisPrefabData>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Dependency = new JobExecution
            {
                Cmd = cmd,
                Destructions = destructions,
                Prefabs = prefabs,
                Rotations = rots,
                Positions = poss,
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter Cmd;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Main.PartDestructionData> Destructions;

            [ReadOnly]
            public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {

                var destruction = this.Destructions[mainEntity];
                
                using var targetParts = new UnsafeHashSet<Entity>(destruction.partLength, Allocator.Temp);
                foreach (var msg in hitMessages)
                {
                    Debug.Log($"{msg.PartId} {(uint)(destruction.Destructions[msg.PartId >> 5] & (uint)(1 << (msg.PartId & 0b11111)))} {destruction.IsDestroyed(msg.PartId)} {this.Prefabs.HasComponent(msg.PartEntity)}");
                    if (destruction.IsDestroyed(msg.PartId)) continue;
                    if (!this.Positions.HasComponent(msg.PartEntity)) continue;

                    destruction.SetDestroyed(msg.PartId);

                    targetParts.Add(msg.PartEntity);
                }

                this.Destructions[mainEntity] = destruction;


                foreach (var part in targetParts)
                {
                    Debug.Log($"{part}");
                    var prefab = this.Prefabs[part].DebrisPrefab;
                    var rot = this.Rotations[part];
                    var pos = this.Positions[part];

                    createDebris_(this.Cmd, index, prefab, rot, pos);
                    destroyPart_(this.Cmd, index, part);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void createDebris_(
                EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
                Rotation rot_, Translation pos_)
            {
                var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
                cmd_.SetComponent(uniqueIndex_, ent, rot_);
                cmd_.SetComponent(uniqueIndex_, ent, pos_);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void destroyPart_(
                EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
            {
                cmd_.DestroyEntity(uniqueIndex_, part_);
            }
        }
        //[BurstCompile]
        //public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForEach
        //{

        //    public EntityCommandBuffer.ParallelWriter Cmd;

        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<Main.PartDestructionData> Destructions;

        //    [ReadOnly]
        //    public ComponentDataFromEntity<Part.DebrisPrefabData> Prefabs;
        //    [ReadOnly]
        //    public ComponentDataFromEntity<Rotation> Rotations;
        //    [ReadOnly]
        //    public ComponentDataFromEntity<Translation> Positions;


        //    [BurstCompile]
        //    public void Execute(int index, Entity targetEntity, PartHitMessage hitMessage)
        //    {
        //        var destruction = this.Destructions[targetEntity];

        //        // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
        //        // 今回は、同じ key は同じスレッドで処理されるので成立する。
        //        destruction.SetDestroyed(hitMessage.PartId);

        //        this.Destructions[targetEntity] = destruction;


        //        var part = hitMessage.PartEntity;
        //        var prefab = this.Prefabs[part].DebrisPrefab;
        //        var rot = this.Rotations[part];
        //        var pos = this.Positions[part];

        //        createDebris_(this.Cmd, index, prefab, rot, pos);
        //        destroyPart_(this.Cmd, index, part);
        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    static void createDebris_
        //        (
        //            EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity debrisPrefab_,
        //            Rotation rot_, Translation pos_
        //        )
        //    {

        //        var ent = cmd_.Instantiate(uniqueIndex_, debrisPrefab_);
        //        cmd_.SetComponent(uniqueIndex_, ent, rot_);
        //        cmd_.SetComponent(uniqueIndex_, ent, pos_);

        //    }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    static void destroyPart_
        //        (EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_, Entity part_)
        //    {
        //        cmd_.DestroyEntity(uniqueIndex_, part_);
        //    }
        //}
    }

}
