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

namespace DotsLite.MarchingCubes
{
    using MarchingCubes;
    using DotsLite.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridBufferManagementSystem : SystemBase
    {


        //EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            //this.RequireSingletonForUpdate<Resource.Initialize>();
            this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
        }
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.Enabled = false;

            var gres = this.GetSingleton<MarchingCubeGlobalData>().ShaderResources;

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        DotGridArea.ResourceGpuModeData ares
                    ) =>
                    {
                        var mat = ares.CubeMaterial;
                        var cs = ares.GridToCubeShader;
                        var mesh = gres.mesh;

                        gres.SetResourcesTo(mat, cs);
                        ares.ShaderResources.SetResourcesTo(mat, cs, mesh);
                    }
                )
                .Run();
        }

        protected unsafe override void OnUpdate()
        { }



        protected override unsafe void OnDestroy()
        //protected override unsafe void OnStopRunning()
        {
            if (!this.HasSingleton<MarchingCubeGlobalData>()) return;

            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        DotGridArea.ResourceGpuModeData area
                        //ref DotGridArea.ShaderInputData buf
                    ) =>
                    {
                        //for (var i = 0; i < buf.Grids.length; i++)
                        //{
                        //    var defs = globaldata.DefaultGrids;
                        //    var grid = buf.Grids[i];
                        //    if (grid.pUnits == null) continue;
                        //    if (defs.IsDefault(grid)) continue;

                        //    buf.Grids[i].Dispose();
                        //}

                        //output.CubeInstances.Dispose();
                        //output.GridInstances.Dispose();
                        //buf.GridInstractions.Dispose();

                        area.ShaderResources.Dispose();
                    }
                )
                .Run();

            globaldata.Dispose();
        }

    }
}
