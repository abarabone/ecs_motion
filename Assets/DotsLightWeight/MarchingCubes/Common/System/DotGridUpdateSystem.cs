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
    public struct DotGridUpdateMessage : IHitMessage
    {
        [FieldOffset(0)] public UpdateAabb aabb;
        [FieldOffset(0)] public UpdateSphere sphere;

        [FieldOffset(32)]
        public DotGridUpdateType type;
    }
    public enum DotGridUpdateType
    {
        aabb,
        sphere,
        capsule,
    }
    public struct UpdateAabb
    {
        public float3 min;
        public float3 max;
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

    // テストのためとりあえずグリッドを追加する
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class DotGridAddSystem : DependencyAccessableSystemBase
    {
        HitMessage<DotGridUpdateMessage>.Sender mcSender;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            this.mcSender = HitMessage<DotGridUpdateMessage>.Sender.Create<DotGridUpdateSystem>(this);
            using var mcScope = this.mcSender.WithDependencyScope();
            var w = mcScope.MessagerAsParallelWriter;

            this.Entities
                .WithAll<DotGrid.ParentAreaData>()
                .ForEach((
                    Entity entity,
                    in DotGrid.UnitData grid) =>
                {
                    w.Add(entity, new DotGridUpdateMessage
                    {
                        type = DotGridUpdateType.aabb,
                        aabb = new UpdateAabb
                        {
                            min = 0,
                            max = 0,
                        },
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
        : DependencyAccessableSystemBase, HitMessage<DotGridUpdateMessage>.IRecievable
    {


        public HitMessage<DotGridUpdateMessage>.Reciever Reciever { get; private set; }

        BarrierDependency.Sender bardep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<DotGridUpdateMessage>.Reciever();// 10000, Allocator.TempJob);

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


            //this.Dependency = new JobExecution_
            //{
            //    MessageHolder = this.Reciever.Holder.messageHolder,
            //    KeyEntities = this.Reciever.Holder.keyEntities.AsDeferredJobArray(),
            //    dotgrids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
            //    dirties = this.GetComponentDataFromEntity<DotGrid.UpdateDirtyRangeData>(),
            //}
            //.Schedule(this.Reciever.Holder.keyEntities, 32, this.Dependency);
            this.Dependency = new JobExecution
            {
                dotgrids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
                dirties = this.GetComponentDataFromEntity<DotGrid.UpdateDirtyRangeData>(),
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency);
        }


        //[BurstCompile]
        //unsafe struct JobExecution_ : IJobParallelForDefer
        //{
        //    [ReadOnly]
        //    public NativeMultiHashMap<Entity, DotGridUpdateMessage> MessageHolder;

        //    [ReadOnly]
        //    public NativeArray<Entity> KeyEntities;


        //    //[NativeDisableParallelForRestriction]
        //    //[NativeDisableContainerSafetyRestriction]

        //    [ReadOnly]
        //    public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;

        //    [WriteOnly]
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<DotGrid.UpdateDirtyRangeData> dirties;


        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public void Execute(int index)
        //    {
        //        var targetEntity = this.KeyEntities[index];
        //        var msgs = this.MessageHolder.GetValuesForKey(targetEntity);

        //        var p = this.dotgrids[targetEntity].Unit.pXline;

        //        foreach (var msg in msgs)
        //        {
        //            switch (msg.type)
        //            {
        //                case DotGridUpdateType.aabb:

        //                    var v = 0x00ffff00u;
        //                    for (var i = 64; i < 32 * 32 - 1 - 64; i++)
        //                    {
        //                        p[i] = v;
        //                    }

        //                    break;
        //                case DotGridUpdateType.sphere:

        //                    break;
        //                case DotGridUpdateType.capsule:

        //                    break;
        //                default: break;
        //            }
        //        }

        //        this.dirties[targetEntity] = new DotGrid.UpdateDirtyRangeData
        //        {
        //            begin = 0,//32 * 32 / 2,
        //            end = 32 * 32 - 1,
        //        };
        //    }
        //}


        [BurstCompile]
        public struct JobExecution : HitMessage<DotGridUpdateMessage>.IApplyJobExecutionForKey
        {
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<DotGrid.UpdateDirtyRangeData> dirties;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity targetEntity,
                NativeMultiHashMap<Entity, DotGridUpdateMessage>.Enumerator msgs)
            {
                var p = this.dotgrids[targetEntity].Unit.pXline;

                foreach (var msg in msgs)
                {
                    switch (msg.type)
                    {
                        case DotGridUpdateType.aabb:

                            var v0 = 0u;
                            for (var i = 0; i < 32; i++)
                            {
                                p[i] = v0;
                            }

                            var v = 0xf444_4b44u;
                            for (var i = 32; i < 32 * 31; i++)
                            {
                                p[i] = v;
                                v = (v << 3) | (v >> 28);
                            }

                            for (var i = 32 * 31; i < 32 * 32; i++)
                            {
                                p[i] = v0;
                            }

                            break;
                        case DotGridUpdateType.sphere:

                            break;
                        case DotGridUpdateType.capsule:

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
