﻿using System.Collections;
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

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class CubeGridBufferManagementSystem : SystemBase
    {


        protected unsafe override void OnCreate()
        {
            base.OnCreate();

            this.Enabled = false;

;
            var globalInfo = this.GetSingleton<CubeGridGlobal.InfoData>();

            var gbuffer = new UnsafeList<UIntPtr>(globalInfo.MaxCubeInstances, Allocator.Persistent);
            var solid = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);
            var blank = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Blank);

            gbuffer.Add((UIntPtr)solid.pUnits);
            gbuffer.Add((UIntPtr)blank.pUnits);

            this.SetSingleton(new CubeGridGlobal.DefualtGridBlankData { DefaultGrid = blank });
            this.SetSingleton(new CubeGridGlobal.DefualtGridSolidData { DefaultGrid = solid });


            this.Entities
                .ForEach(
                    (
                        ref CubeGridArea.BufferData buffer,
                        in CubeGridArea.InfoData info,
                        in CubeGridArea.InitializeData init
                    ) =>
                    {

                        var wholeLength = info.GridLength + 2;
                        var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

                        var gridarea = allocGridArea_(totalSize, init.FillMode);
                        gbuffer.Add((UIntPtr)gridarea.Ptr);

                        buffer.Grids = gridarea;
                    }
                )
                .Run();

            this.SetSingleton(
                new CubeGridGlobal.BufferData
                {
                    CubeBuffers = gbuffer,
                }
            );


            UnsafeList<CubeGrid32x32x32Unsafe> allocGridArea_(int totalSize, GridFillMode fillMode)
            {
                var buffer = new UnsafeList<CubeGrid32x32x32Unsafe>(totalSize, Allocator.Persistent);
                buffer.length = totalSize;

                var defaultGrid = fillMode == GridFillMode.Solid
                    ? this.GetSingleton<CubeGridGlobal.DefualtGridSolidData>().DefaultGrid
                    : this.GetSingleton<CubeGridGlobal.DefualtGridBlankData>().DefaultGrid;

                for (var i = 0; i < totalSize; i++)
                {
                    buffer[i] = defaultGrid;
                }

                return buffer;
            }
        }



        protected override void OnUpdate()
        { }



        protected override void OnDestroy()
        {

            this.Entities
                .ForEach(
                    (
                        in CubeGridArea.BufferData buffer
                    ) =>
                    {

                        buffer.Grids.Dispose();

                    }
                )
                .Run();

            disposeGridStocksAndGlobalBuffer_();


            base.OnDestroy();

            return;



            void disposeGridStocksAndGlobalBuffer_()
            {
                var globufBuffer = this.GetSingleton<CubeGridGlobal.BufferData>();

                for (var i = 0; i < globufBuffer.CubeBuffers.length; i++)
                {

                    CubeGridAllocater.Dispose(globufBuffer.CubeBuffers[i]);

                }
                globufBuffer.CubeBuffers.Dispose();
            }

        }

    }
}
