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

namespace Abarabone.Structure
{
    using Abarabone.Dependency;

    public struct StructureHitMessage
    {
        public int PartId;
        public Entity PartEntity;
        public float3 Position;
        public float3 Normale;
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class StructureHitMessageApplySystem : DependentableSystemBase
    {


        public HitMessageReciever<StructureHitMessage, JobExecution> Reciever { get; private set; }

        CommandBufferDependency<BeginInitializationEntityCommandBufferSystem> cmdbuf;


        protected override void OnCreate()
        {
            base.OnCreate();
            this.Reciever = new HitMessageReciever<StructureHitMessage, JobExecution>(10000);
            this.cmdbuf = new CommandBufferDependency<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.Reciever.Dispose();
        }

        protected override void OnUpdate()
        {
            using var dc = this.cmdbuf.AsDependencyDisposable();
            var cmd = this.cmdbuf.CreateCommandBuffer().AsParallelWriter();

            var destructions = this.GetComponentDataFromEntity<Structure.PartDestructionData>();
            var prefabs = this.GetComponentDataFromEntity<StructurePart.DebrisPrefabData>(isReadOnly: true);
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
            .ScheduleParallel(this.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : IHitMessageApplyJobExecution<StructureHitMessage>
        {

            public EntityCommandBuffer.ParallelWriter Cmd;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Structure.PartDestructionData> Destructions;

            [ReadOnly]
            public ComponentDataFromEntity<StructurePart.DebrisPrefabData> Prefabs;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> Positions;


            [BurstCompile]
            public void Execute(int index, Entity targetEntity, StructureHitMessage hitMessage)
            {
                var destruction = this.Destructions[targetEntity];

                // 複数の子パーツから１つの親構造物のフラグを立てることがあるので、並列化の際に注意が必要
                // 今回は、同じ key は同じスレッドで処理されるので成立する。
                destruction.SetDestroyed(hitMessage.PartId);

                this.Destructions[targetEntity] = destruction;


                var part = hitMessage.PartEntity;
                var prefab = this.Prefabs[part].DebrisPrefab;
                var rot = this.Rotations[part];
                var pos = this.Positions[part];
                createDebris_(this.Cmd, index, prefab, rot, pos);

                //destroyPart_(this.Cmd, index, part);
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
