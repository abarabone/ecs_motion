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
    public class CubeGridBufferManagementSystem : SystemBase
    {


        EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected unsafe override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer();


            var globalInfo = this.GetSingleton<CubeGridGlobal.InfoData>();


            //this.Entities
            //    .ForEach(
            //            (
            //                Entity entity,
            //                ref CubeGridArea.BufferData buffer,
            //                in CubeGridArea.InfoData info,
            //                in CubeGridArea.InitializeData init
            //            ) =>
            //        {

            //            var wholeLength = info.GridLength + 2;
            //            var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

            //            var gridarea = allocGridArea_(totalSize, init.FillMode);
            //            gbuffer.Add((UIntPtr)gridarea.Ptr);

            //            buffer.Grids = gridarea;

            //            cmd.RemoveComponent<CubeGridArea.InitializeData>(entity);
            //        }
            //    )
            //    .Run();

            //this.SetSingleton(
            //    new CubeGridGlobal.BufferData
            //    {
            //        CubeBuffers = gbuffer,
            //    }
            //);


            //UnsafeList<CubeGrid32x32x32Unsafe> allocGridArea_(int totalSize, GridFillMode fillMode)
            //{
            //    var buffer = new UnsafeList<CubeGrid32x32x32Unsafe>(totalSize, Allocator.Persistent);
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
            var globalent = this.GetSingletonEntity<CubeGridGlobal.InfoData>();
            var globalInfo = this.EntityManager.GetComponentData<CubeGridGlobal.InfoData>(globalent);
            var globalDefaults = this.EntityManager.GetBuffer<CubeGridGlobal.DefualtGridData>(globalent);
            var globalStocks = this.EntityManager.GetBuffer<CubeGridGlobal.FreeGridStockData>(globalent);

            this.Entities
                .ForEach(
                    (
                        ref CubeGridArea.BufferData buffer
                    ) =>
                    {
                        disposeCubeInGridArea_(ref buffer);
                    }
                )
                .Run();


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


            void disposeCubeInGridArea_(ref CubeGridArea.BufferData buffer_)
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

            void disposeCubeInFreeStocks_(ref CubeGridGlobal.FreeGridStockData stocks_)
            {
                for (var i = 0; i < stocks_.FreeGridStocks.length; i++)
                {
                    CubeGridAllocater.Dispose(stocks_.FreeGridStocks[i]);
                }
                stocks_.FreeGridStocks.Dispose();
            }

            void disposeCubeInDefault_(ref CubeGridGlobal.DefualtGridData default_)
            {
                default_.DefaultGrid.Dispose();
            }
        }

    }
}
