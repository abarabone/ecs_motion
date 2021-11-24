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
    using DotsLite.MarchingCubes.Data;


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


            //var areas = this.GetComponentDataFromEntity<BitGridArea.GridLinkData>(isReadOnly: true);
            //var drawmodels = this.GetComponentDataFromEntity<CubeDrawModel.MakeCubesShaderResourceData>(isReadOnly: true);
            var em = this.EntityManager;

            this.Job
                .WithoutBurst()
                .WithCode(() =>
                {
                    foreach (var ent in this.MessageSystem.Reciever.Holder.TargetEntities)
                    {
                        var locate = em.GetComponentData<BitGrid.LocationInAreaData>(ent);
                        var dirty = em.GetComponentData<BitGrid.UpdateDirtyRangeData>(ent);
                        var parent = em.GetComponentData<BitGrid.ParentAreaData>(ent);
                        var gridtype = em.GetComponentData<BitGrid.GridTypeData>(ent);
                        var drawlink = em.GetComponentData<Draw.DrawInstance.ModelLinkData>(ent);

                        var drawres = em.GetComponentData<CubeDrawModel.MakeCubesShaderResourceData>(drawlink.DrawModelEntityCurrent);//
                        var gids = em.GetComponentData<BitGridArea.GridInstructionIdData>(parent.ParentAreaEntity);

                        var grid = em.GetComponentData<BitGrid.BitLinesData>(ent);
                        Copy(in grid, in locate, in dirty, in gids, in drawres);
                    }
                })
                .Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void Copy(
            in BitGrid.BitLinesData grid,
            in BitGrid.LocationInAreaData locate,
            in BitGrid.UpdateDirtyRangeData dirty,
            in BitGridArea.GridInstructionIdData gids,
            in CubeDrawModel.MakeCubesShaderResourceData res)

        {
            var igrid = locate.IndexInArea.serial;
            var dstoffset = gids.pId3dArray[igrid] * res.XLineLengthPerGrid;
            var p = grid.p;

            var garr = NativeUtility.PtrToNativeArray(p, res.XLineLengthPerGrid);
            var srcstart = (int)dirty.begin;
            var dststart = dstoffset + (int)dirty.begin;
            var count = (int)dirty.end - (int)dirty.begin + 1;
            res.GridBitLines.Buffer.SetData(garr, srcstart, dststart, count);
            //Debug.Log($"{locate.IndexInArea.index}:{locate.IndexInArea.serial} {srcstart}:{dststart}:{count}");
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