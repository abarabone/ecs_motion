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
using System.Runtime.InteropServices;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;

    [StructLayout(LayoutKind.Explicit)]
    public struct UpdateMessage : IHitMessage
    {
        [FieldOffset(0)] public AABB aabb;
        [FieldOffset(0)] public UpdateSphere sphere;

        [FieldOffset(32)]
        public DotGridUpdateType type;
    }

    public enum DotGridUpdateType
    {
        none,

        cube_force32,
        aabb_add32,
        aabb_remove32,
        sphere_add32,
        sphere_remove32,
        capsule_add32,
        capsule_remove32,

        cube_force16,
        aabb_add16,
        aabb_remove16,
        sphere_add16,
        sphere_remove16,
        capsule_add16,
        capsule_remove16,
    }

    public struct UpdateSphere
    {
        public float3 center;
        public float radius;
    }


    // テストのためとりあえずグリッドを追加する
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class DotGridAddSystem : DependencyAccessableSystemBase
    {

        HitMessage<UpdateMessage>.Sender mcSender;


        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            this.mcSender = HitMessage<UpdateMessage>.Sender.Create<DotGridMessageAllocSystem>(this);
            using var mcScope = this.mcSender.WithDependencyScope();
            var w = mcScope.MessagerAsParallelWriter;

            this.Entities
                .WithAll<DotGrid.Unit32Data>()
                .ForEach((
                    Entity entity) =>
                {
                    w.Add(entity, new UpdateMessage
                    {
                        type = DotGridUpdateType.cube_force32,
                    });
                })
                .ScheduleParallel();

            this.Entities
                .WithAll<DotGrid.Unit16Data>()
                .ForEach((
                    Entity entity) =>
                {
                    w.Add(entity, new UpdateMessage
                    {
                        type = DotGridUpdateType.cube_force16,
                    });
                })
                .ScheduleParallel();
        }
        protected override void OnUpdate()
        { }
    }


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridMessageAllocSystem : SystemBase, HitMessage<UpdateMessage>.IRecievable
    {


        public HitMessage<UpdateMessage>.Reciever Reciever { get; private set; }



        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<UpdateMessage>.Reciever();// 10000, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override void OnUpdate()
        {
            this.Reciever.Alloc(10000, Allocator.TempJob);
        }
    }


}
