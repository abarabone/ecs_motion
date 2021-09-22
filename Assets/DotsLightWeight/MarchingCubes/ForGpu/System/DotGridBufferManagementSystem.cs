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


            var em = this.EntityManager;

            this.Entities
                .WithoutBurst()
                .ForEach((
                    Entity ent,
                    MarchingCubeGlobalData data,
                    Global.InitializeData init) =>
                {
                    var shres = new GlobalShaderResources();
                    shres.Alloc(init.asset, init.maxGridInstances);
                    data.Alloc(init.maxFreeGrids, shres);

                    //em.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();


            var gres = this.GetSingleton<MarchingCubeGlobalData>().ShaderResources;

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        DotGridArea.InitializeData init,
                        DotGridArea.ResourceGpuModeData res
                    ) =>
                    {
                        var mat = init.CubeMaterial;
                        var cs = init.GridToCubesShader;
                        var mesh = gres.mesh;

                        gres.SetResourcesTo(mat, cs);

                        var shres = new DotGridAreaResourcesForGpu();
                        shres.SetResourcesTo(mat, cs);
                        shres.SetBuffers(mesh);
                        res.CubeMaterial = mat;
                        res.GridToCubeShader = cs;
                        res.ShaderResources = shres;
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
