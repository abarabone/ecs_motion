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



    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    public class DotGridUpdateSystem : DependencyAccessableSystemBase, HitMessage<DotGridUpdateMessage>.IRecievable
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


            //this.Reciever.Holder.Dispose();
            this.Reciever.Alloc(10000, Allocator.TempJob);

            this.Dependency = new JobExecution
            {
                dotgrids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.Reciever, 32, this.Dependency, needClear: false);
        }



        [BurstCompile]
        public struct JobExecution : HitMessage<DotGridUpdateMessage>.IApplyJobExecutionForKey
        {
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;

            [BurstCompile]
            public unsafe void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, DotGridUpdateMessage>.Enumerator msgs)
            {
                var p = this.dotgrids[targetEntity].Unit.pXline;

                foreach (var msg in msgs)
                {



                }

            }
        }
    }

}
