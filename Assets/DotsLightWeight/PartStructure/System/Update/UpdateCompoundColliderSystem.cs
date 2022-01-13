using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Draw
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Structure;
    using DotsLite.Dependency;


    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class UpdateCompoundColliderSystem : DependencyAccessableSystemBase
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

            this.Dependency = new JobExecution
            {
                cmd = cmd,

                cols = this.GetComponentDataFromEntity<PhysicsCollider>(),
                infos = this.GetComponentDataFromEntity<Main.PartInfoData>(isReadOnly: true),
                ress = this.GetBufferFromEntity<Main.PartDestructionResourceData>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency);

        }


        [BurstCompile]
        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter cmd;

            public ComponentDataFromEntity<PhysicsCollider> cols;

            [ReadOnly]
            public ComponentDataFromEntity<Main.PartInfoData> infos;

            [ReadOnly]
            public BufferFromEntity<Main.PartDestructionResourceData> ress;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
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
