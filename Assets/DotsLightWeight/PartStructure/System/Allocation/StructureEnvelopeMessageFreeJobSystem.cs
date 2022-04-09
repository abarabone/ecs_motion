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

namespace DotsLite.Structure
{
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateAfter(typeof(StructureEnvelopeMessageAllocationSystem))]
    //public class StructureEnvelopeMessageFreeJobSystem : SystemBase
    //{

    //    StructureEnvelopeMessageAllocationSystem allocationSystem;


    //    protected override void OnCreate()
    //    {
    //        base.OnCreate();

    //        this.allocationSystem = this.World.GetOrCreateSystem<StructureEnvelopeMessageAllocationSystem>();
    //    }

    //    protected override void OnUpdate()
    //    {
    //        this.Dependency = this.allocationSystem.Reciever.Holder.ScheduleDispose(this.Dependency);
    //    }
    //}

    public partial class StructureEnvelopeMessageFreeJobSystem : SystemBase, BarrierDependency.IRecievable
    {

        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        StructureEnvelopeMessageAllocationSystem allocationSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructureEnvelopeMessageAllocationSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }


        protected override unsafe void OnUpdate()
        {
            var dep = this.Reciever.CombineAllDependentJobs(this.Dependency);

            this.Dependency = this.allocationSystem.Reciever.Holder.ScheduleDispose(dep);
        }

    }

}