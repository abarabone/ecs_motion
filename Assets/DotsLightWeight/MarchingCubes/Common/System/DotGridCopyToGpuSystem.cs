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
    using DotsLite.Utilities;


    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    //[UpdateAfter(typeof(DotGridUpdateSystem))]
    [UpdateBefore(typeof(Gpu.DrawMarchingCubeCsSystem))]
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


            var em = this.EntityManager;

            //var grids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true);
            //var dirties = this.GetComponentDataFromEntity<DotGrid.UpdateDirtyRangeData>(isReadOnly: true);
            //var parents = this.GetComponentDataFromEntity<DotGrid.ParentAreaData>(isReadOnly: true);

            foreach (var ent in this.MessageHolderSystem.Reciever.Holder.TargetEntities)
            {
                //var grid = grids[ent];
                //var dirty = dirties[ent];
                //var parent = parents[ent];
                var grid = em.GetComponentData<DotGrid.UnitData>(ent);
                var dirty = em.GetComponentData<DotGrid.UpdateDirtyRangeData>(ent);
                var parent = em.GetComponentData<DotGrid.ParentAreaData>(ent);

                var p = grid.Unit.pXline;
                var res = em.GetComponentData<DotGridArea.ResourceGpuModeData>(parent.ParentArea);

                var arr = NativeUtility.PtrToNativeArray(p, 32 * 32);
                var srcstart = (int)dirty.begin;
                var dststart = grid.GridIndexInArea.serial * 32 * 32 + (int)dirty.begin;
                var count = (int)dirty.end - (int)dirty.begin;
                res.ShaderResources.GridContentDataBuffer.Buffer.SetData(arr, srcstart, dststart, count);
            }

            this.Dependency = this.MessageHolderSystem.Reciever.Holder.ScheduleDispose(this.Dependency);
        }

    }

}
