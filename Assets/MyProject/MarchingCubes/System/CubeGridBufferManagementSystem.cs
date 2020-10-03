using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    using MarchingCubes;
    using Abarabone.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridBufferManagementSystem : SystemBase
    {


        //EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            //this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            this.RequireSingletonForUpdate<Resource.Initialize>();
            //this.Enabled = false;
        }

        protected unsafe override void OnUpdate()
        {
            //var cmd = this.cmdSystem.CreateCommandBuffer();


            //var globalInfo = this.GetSingleton<DotGridGlobal.InfoData>();


            //this.Entities
            //    .ForEach(
            //            (
            //                Entity entity,
            //                ref DotGridArea.BufferData buffer,
            //                in DotGridArea.InfoData info,
            //                in DotGridArea.InitializeData init
            //            ) =>
            //        {

            //            var wholeLength = info.GridLength + 2;
            //            var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

            //            var gridarea = allocGridArea_(totalSize, init.FillMode);
            //            gbuffer.Add((UIntPtr)gridarea.Ptr);

            //            buffer.Grids = gridarea;

            //            cmd.RemoveComponent<DotGridArea.InitializeData>(entity);
            //        }
            //    )
            //    .Run();

            //this.SetSingleton(
            //    new DotGridGlobal.BufferData
            //    {
            //        CubeBuffers = gbuffer,
            //    }
            //);


            //UnsafeList<DotGrid32x32x32Unsafe> allocGridArea_(int totalSize, GridFillMode fillMode)
            //{
            //    var buffer = new UnsafeList<DotGrid32x32x32Unsafe>(totalSize, Allocator.Persistent);
            //    buffer.length = totalSize;

            //    var defaultGrid = fillMode == GridFillMode.Solid ? solid : blank;

            //    for (var i = 0; i < totalSize; i++)
            //    {
            //        buffer[i] = defaultGrid;
            //    }

            //    return buffer;
            //}
        }



        protected override unsafe void OnDestroy()
        {
            if (!this.HasSingleton<DotGridGlobal.InfoData>()) return;


            var globalent = this.GetSingletonEntity<DotGridGlobal.InfoData>();
            var globalInfo = this.EntityManager.GetComponentData<DotGridGlobal.InfoData>(globalent);
            var globalDefaults = this.EntityManager.GetBuffer<DotGridGlobal.DefualtGridData>(globalent);
            var globalStocks = this.EntityManager.GetBuffer<DotGridGlobal.FreeGridStockData>(globalent);
            var globalInstances = this.EntityManager.GetComponentData<DotGridGlobal.InstanceWorkData>(globalent);

            this.Entities
                .ForEach(
                    (
                        ref DotGridArea.BufferData buffer
                    ) =>
                    {
                        disposeCubeInGridArea_(ref buffer);
                    }
                )
                .Run();


            globalInstances.CubeInstances.Dispose();
            globalInstances.GridInstances.Dispose();

            var blankFreeStocks = globalStocks.ElementAt((int)GridFillMode.Blank);
            var solidFreeStocks = globalStocks.ElementAt((int)GridFillMode.Solid);

            disposeCubeInFreeStocks_(ref blankFreeStocks);
            disposeCubeInFreeStocks_(ref solidFreeStocks);


            var blankDefault = globalDefaults.ElementAt((int)GridFillMode.Blank);
            var solidDefault = globalDefaults.ElementAt((int)GridFillMode.Solid);

            disposeCubeInDefault_(ref blankDefault);
            disposeCubeInDefault_(ref solidDefault);


            base.OnDestroy();

            return;


            void disposeCubeInGridArea_(ref DotGridArea.BufferData buffer_)
            {
                for (var i = 0; i < buffer_.Grids.length; i++)
                {
                    ref var grid = ref buffer_.Grids.ElementAt(i);

                    if (grid.pUnits == default) continue;
                    if (globalDefaults.IsDefault(grid)) continue;

                    grid.Dispose();
                }

                buffer_.Grids.Dispose();
            }

            void disposeCubeInFreeStocks_(ref DotGridGlobal.FreeGridStockData stocks_)
            {
                for (var i = 0; i < stocks_.FreeGridStocks.length; i++)
                {
                    DotGridAllocater.Dispose(stocks_.FreeGridStocks[i]);
                }
                stocks_.FreeGridStocks.Dispose();
            }

            void disposeCubeInDefault_(ref DotGridGlobal.DefualtGridData default_)
            {
                default_.DefaultGrid.Dispose();
            }
        }

    }
}
