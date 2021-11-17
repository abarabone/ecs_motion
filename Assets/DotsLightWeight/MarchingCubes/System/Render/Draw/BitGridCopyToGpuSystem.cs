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
    //[UpdateAfter(typeof(BitGridUpdateSystem))]
    [UpdateBefore(typeof(Gpu.DrawMarchingCubeCsSystem))]
    public class BitGridCopyToGpuSystem : DependencyAccessableSystemBase, BarrierDependency.IRecievable
    {


        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        BarrierDependency.Sender bardep;

        public BitGridMessageAllocSystem MessageSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            
            this.MessageSystem = this.World.GetOrCreateSystem<BitGridMessageAllocSystem>();

            this.bardep = BarrierDependency.Sender.Create<BitGridMessageFreeSystem>(this);
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


            //var areas = this.GetComponentDataFromEntity<BitGridArea.LinkToGridData>(isReadOnly: true);
            var em = this.EntityManager;

            this.Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var ent in this.MessageSystem.Reciever.Holder.TargetEntities)
                    {
                        ////var grid = grids[ent];
                        ////var dirty = dirties[ent];
                        ////var parent = parents[ent];
                        //var index = em.GetComponentData<BitGrid.IndexData>(ent);
                        //var dirty = em.GetComponentData<BitGrid.UpdateDirtyRangeData>(ent);
                        //var parent = em.GetComponentData<BitGrid.ParentAreaData>(ent);
                        //var gridtype = em.GetComponentData<BitGrid.GridTypeData>(ent);

                        ////var p = grid.Unit.pXline;
                        //var res = em.GetComponentData<BitGridArea.ResourceGpuModeData>(parent.ParentArea);

                        //var area = areas[parent.ParentArea];
                        
                        //switch (gridtype.UnitOnEdge)
                        //{
                        //    case 32:
                        //    {
                        //        var grid = em.GetComponentData<BitGrid.Unit32Data>(ent);
                        //        grid.Unit.Copy(in index, in dirty, in area, in res);
                        //    }
                        //    break;

                        //    case 16:
                        //    {
                        //        var grid = em.GetComponentData<BitGrid.Unit16Data>(ent);
                        //        grid.Unit.Copy(in index, in dirty, in area, in res);
                        //    }
                        //    break;
                        //}
                    }
                })
                .Run();
        }

    }

    //static class Extension
    //{

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public unsafe static void Copy<TGrid>(
    //        this TGrid grid,
    //        in BitGrid.IndexData index, in BitGrid.UpdateDirtyRangeData dirty, in BitGridArea.LinkToGridData area,
    //        in BitGridArea.ResourceGpuModeData res)
    //        where TGrid : struct, IBitGrid<TGrid>
    //    {
    //        var p = grid.pXline;

    //        var igrid = index.GridIndexInArea.serial;
    //        var igarr = area.pGridPoolIds[igrid] * grid.XLineBufferLength;

    //        var garr = NativeUtility.PtrToNativeArray(p, grid.XLineBufferLength);
    //        var srcstart = (int)dirty.begin;
    //        var dststart = igarr + (int)dirty.begin;
    //        var count = (int)dirty.end - (int)dirty.begin + 1;
    //        res.ShaderResources.GridDotContentDataBuffer.Buffer.SetData(garr, srcstart, dststart, count);
    //        //Debug.Log($"{index.GridIndexInArea.index}:{index.GridIndexInArea.serial} {srcstart}:{dststart}:{count}");
    //    }
    //}

}
