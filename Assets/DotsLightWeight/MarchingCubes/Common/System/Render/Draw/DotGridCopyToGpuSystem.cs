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
using UnityEngine;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;
    using DotsLite.Utilities;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    //[UpdateAfter(typeof(DotGridUpdateSystem))]
    [UpdateBefore(typeof(Gpu.DrawMarchingCubeCsSystem))]
    public class DotGridCopyToGpuSystem : DependencyAccessableSystemBase, BarrierDependency.IRecievable
    {


        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        BarrierDependency.Sender bardep;

        public DotGridMessageAllocSystem MessageSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            
            this.MessageSystem = this.World.GetOrCreateSystem<DotGridMessageAllocSystem>();

            this.bardep = BarrierDependency.Sender.Create<DotGridMessageFreeSystem>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override unsafe void OnUpdate()
        {
            using var barscope = this.bardep.WithDependencyScope();

            this.Reciever.CompleteAllDependentJobs(this.Dependency);


            var areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true);
            var em = this.EntityManager;

            this.Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var ent in this.MessageSystem.Reciever.Holder.TargetEntities)
                    {
                        //var grid = grids[ent];
                        //var dirty = dirties[ent];
                        //var parent = parents[ent];
                        var index = em.GetComponentData<DotGrid.IndexData>(ent);
                        var dirty = em.GetComponentData<DotGrid.UpdateDirtyRangeData>(ent);
                        var parent = em.GetComponentData<DotGrid.ParentAreaData>(ent);
                        var gridtype = em.GetComponentData<DotGrid.GridTypeData>(ent);

                        //var p = grid.Unit.pXline;
                        var res = em.GetComponentData<DotGridArea.ResourceGpuModeData>(parent.ParentArea);

                        var area = areas[parent.ParentArea];
                        
                        switch (gridtype.UnitOnEdge)
                        {
                            case 32:
                            {
                                var grid = em.GetComponentData<DotGrid.Unit32Data>(ent);
                                grid.Unit.Copy(in index, in dirty, in area, in res);
                            }
                            break;

                            case 16:
                            {
                                var grid = em.GetComponentData<DotGrid.Unit16Data>(ent);
                                grid.Unit.Copy(in index, in dirty, in area, in res);
                            }
                            break;
                        }
                    }
                })
                .Run();
        }

    }

    static class Extension
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Copy<TGrid>(
            this TGrid grid,
            in DotGrid.IndexData index, in DotGrid.UpdateDirtyRangeData dirty, in DotGridArea.LinkToGridData area,
            in DotGridArea.ResourceGpuModeData res)
            where TGrid : struct, IDotGrid<TGrid>
        {
            var p = grid.pXline;

            var igrid = index.GridIndexInArea.serial;
            var igarr = area.pGridPoolIds[igrid] * grid.XLineBufferLength;

            var garr = NativeUtility.PtrToNativeArray(p, grid.XLineBufferLength);
            var srcstart = (int)dirty.begin;
            var dststart = igarr + (int)dirty.begin;
            var count = (int)dirty.end - (int)dirty.begin + 1;
            res.ShaderResources.GridDotContentDataBuffer.Buffer.SetData(garr, srcstart, dststart, count);
            //Debug.Log($"{index.GridIndexInArea.index}:{index.GridIndexInArea.serial} {srcstart}:{dststart}:{count}");
        }
    }

}
