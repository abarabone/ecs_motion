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
        cube_force,
        aabb_add,
        aabb_remove,
        sphere_add,
        sphere_remove,
        capsule_add,
        capsule_remove,
    }
    public struct UpdateSphere
    {
        public float3 center;
        public float radius;
    }



    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridUpdateAllocSystem : SystemBase
    {
        DotGridUpdateSystem sys;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.sys = this.World.GetOrCreateSystem<DotGridUpdateSystem>();
        }
        protected override void OnUpdate()
        {
            this.sys.Reciever.Alloc(10000, Allocator.TempJob);
        }
    }

    // �e�X�g�̂��߂Ƃ肠�����O���b�h��ǉ�����
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class DotGridAddSystem : DependencyAccessableSystemBase
    {
        HitMessage<UpdateMessage>.Sender mcSender;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            this.mcSender = HitMessage<UpdateMessage>.Sender.Create<DotGridUpdateSystem>(this);
            using var mcScope = this.mcSender.WithDependencyScope();
            var w = mcScope.MessagerAsParallelWriter;

            this.Entities
                .WithAll<DotGrid.ParentAreaData>()
                .ForEach((
                    Entity entity,
                    in DotGrid.UnitData grid) =>
                {
                    w.Add(entity, new UpdateMessage
                    {
                        type = DotGridUpdateType.cube_force,
                    });
                })
                .ScheduleParallel();
        }
        protected override void OnUpdate()
        { }
    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    public class DotGridUpdateSystem
        : DependencyAccessableSystemBase, HitMessage<UpdateMessage>.IRecievable
    {


        public HitMessage<UpdateMessage>.Reciever Reciever { get; private set; }

        BarrierDependency.Sender bardep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<UpdateMessage>.Reciever();// 10000, Allocator.TempJob);

            this.bardep = BarrierDependency.Sender.Create<DotGridCopyToGpuSystem>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override void OnUpdate()
        {
            using var barScope = this.bardep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                dotgrids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
                dirties = this.GetComponentDataFromEntity<DotGrid.UpdateDirtyRangeData>(),
                parents = this.GetComponentDataFromEntity<DotGrid.ParentAreaData>(isReadOnly: true),
                areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true),
                dims = this.GetComponentDataFromEntity<DotGridArea.UnitDimensionData>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.Reciever, 1, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<UpdateMessage>.IApplyJobExecutionForKey
        {
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.ParentAreaData> parents;

            [ReadOnly]
            public ComponentDataFromEntity<DotGridArea.LinkToGridData> areas;
            [ReadOnly]
            public ComponentDataFromEntity<DotGridArea.UnitDimensionData> dims;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<DotGrid.UpdateDirtyRangeData> dirties;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity targetEntity,
                NativeMultiHashMap<Entity, UpdateMessage>.Enumerator msgs)
            {
                var p = this.dotgrids[targetEntity].Unit.pXline;
                var parent = this.parents[targetEntity].ParentArea;
                var area = this.areas[parent];
                var dim = this.dims[parent];

                foreach (var msg in msgs)
                {
                    switch (msg.type)
                    {
                        case DotGridUpdateType.cube_force:

                            var v0 = 0u;
                            for (var i = 0; i < 32; i++)
                            {
                                p[i] = v0;
                            }

                            var v = 0xf444_4b44u;
                            for (var i = 32; i < 32 * 3; i++)
                            {
                                p[i] = v;
                                v = (v << 3) | (v >> 28);
                            }

                            for (var i = 32 * 31; i < 32 * 32; i++)
                            {
                                p[i] = v0;
                            }

                            break;
                        case DotGridUpdateType.aabb_add:

                            //msg.aabb.Add(in area, in dim);
                            p[0] = 0xffff_ffff;

                            break;
                        case DotGridUpdateType.sphere_add:

                            break;
                        case DotGridUpdateType.capsule_add:

                            break;
                        default: break;
                    }
                }

                this.dirties[targetEntity] = new DotGrid.UpdateDirtyRangeData
                {
                    begin = 0,//32 * 32 / 2,
                    end = 32 * 32 - 1,
                };
            }
        }

    }

}