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

        //public HitMessage<DotGridUpdateMessage>.Reciever 
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


            var areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true);
            var em = this.EntityManager;

            this.Job
                .WithoutBurst()
                .WithCode(() =>
                {
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

                        var area = areas[parent.ParentArea];
                        var igrid = grid.GridIndexInArea.serial;
                        var igarr = area.pGridPoolIds[igrid] * 32 * 32;

                        var garr = NativeUtility.PtrToNativeArray(p, 32 * 32);
                        var srcstart = (int)dirty.begin;
                        var dststart = igarr + (int)dirty.begin;
                        var count = (int)dirty.end - (int)dirty.begin + 1;
                        res.ShaderResources.GridContentDataBuffer.Buffer.SetData(garr, srcstart, dststart, count);
                        Debug.Log($"{grid.GridIndexInArea.index}:{grid.GridIndexInArea.serial} {srcstart}:{dststart}:{count}");
                    }
                })
                .Run();

            //var grids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true);
            //var dirties = this.GetComponentDataFromEntity<DotGrid.UpdateDirtyRangeData>(isReadOnly: true);
            //var parents = this.GetComponentDataFromEntity<DotGrid.ParentAreaData>(isReadOnly: true);
            //new JobExecution
            //{

            //}



            this.Dependency = this.MessageHolderSystem.Reciever.Holder.ScheduleDispose(this.Dependency);
        }


        //public struct JobExecution : HitMessage<DotGridUpdateMessage>.IApplyJobExecutionForKey
        //{
        //    [ReadOnly]
        //    public ComponentDataFromEntity<DotGrid.UnitData> dotgrids;

        //    [WriteOnly]
        //    [NativeDisableParallelForRestriction]
        //    public ComponentDataFromEntity<DotGrid.UpdateDirtyRangeData> dirties;

        //    //var grid = em.GetComponentData<DotGrid.UnitData>(ent);
        //    //var dirty = em.GetComponentData<DotGrid.UpdateDirtyRangeData>(ent);
        //    //var parent = em.GetComponentData<DotGrid.ParentAreaData>(ent);


        //    [BurstDiscard]
        //    public unsafe void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, DotGridUpdateMessage>.Enumerator msgs)
        //    {
        //        var p = this.dotgrids[targetEntity].Unit.pXline;

        //        foreach (var msg in msgs)
        //        {
        //            switch (msg.type)
        //            {
        //                case DotGridUpdateType.aabb:

        //                    p[1] = 0x00ffff00;
        //                    p[2] = 0x00ffff00;
        //                    p[32] = 0x00ffff00;
        //                    p[33] = 0x00ffff00;

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
        //            begin = 0,
        //            end = 32 * 32 - 1,
        //        };
        //    }
        //}
    }

}
