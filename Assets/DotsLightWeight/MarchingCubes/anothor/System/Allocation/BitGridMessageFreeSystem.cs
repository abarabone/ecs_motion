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

namespace DotsLite.MarchingCubes.another
{
    using DotsLite.Dependency;




    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.DrawAfter))]
    //[UpdateAfter(typeof(DotGridUpdateSystem))]
    //[UpdateAfter(typeof(Gpu.DrawMarchingCubeCsSystem))]
    public class BitGridMessageFreeSystem : SystemBase, BarrierDependency.IRecievable
    {

        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        BitGridMessageAllocSystem messageSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.messageSystem = this.World.GetOrCreateSystem<BitGridMessageAllocSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }


        protected override unsafe void OnUpdate()
        {
            var dep = this.Reciever.CombineAllDependentJobs(this.Dependency);

            this.Dependency = this.messageSystem.Reciever.Holder.ScheduleDispose(dep);
        }

    }

}
