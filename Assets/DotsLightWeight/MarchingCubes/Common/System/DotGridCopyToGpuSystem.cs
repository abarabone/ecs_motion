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

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(DotGridUpdateSystem))]
    public class DotGridCopyToGpuSystem : DependencyAccessableSystemBase, BarrierDependency.IRecievable
    {


        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        public DotGridUpdateSystem MessageHolderSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.MessageHolderSystem = this.World.GetOrCreateSystem<DotGridUpdateSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override unsafe void OnUpdate()
        {
            this.Reciever.CompleteAllDependentJobs(this.Dependency);

            //foreach ()

            this.MessageHolderSystem.Reciever.Holder.ClearAndSchedule(this.Dependency);
        }

    }

}
