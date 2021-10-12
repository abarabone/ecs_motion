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

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class DotGridToCollisionSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        public DotGridUpdateSystem MessageHolderSystem;



        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DotGridCopyToGpuSystem>(this);

            this.MessageHolderSystem = this.World.GetOrCreateSystem<DotGridUpdateSystem>();
        }

        protected override void OnUpdate()
        {
            using var barScope = this.bardep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                dotgrids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
                KeyEntities = this.MessageHolderSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
            }
            .Schedule(this.MessageHolderSystem.Reciever.Holder.keyEntities, 32, this.Dependency);
        }



        [BurstCompile]
        unsafe struct JobExecution : IJobParallelForDefer
        {
            [ReadOnly]
            public NativeArray<Entity> KeyEntities;

            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;

            [ReadOnly]
            public ComponentDataFromEntity<Translation> poss;


            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(int index)
            {
                var targetEntity = this.KeyEntities[index];
                var p = this.dotgrids[targetEntity].Unit.pXline;




            }
        }

    }

}
