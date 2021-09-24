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

            this.RequireSingletonForUpdate<Global.InitializeData>();
            //this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
        }
        protected override void OnUpdate()
        {
            //    base.OnStartRunning();
            //    this.Enabled = false;


            var em = this.EntityManager;

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    Global.InitializeData init) =>
                {
                    var shres = new GlobalShaderResources();
                    shres.Alloc(init.asset, init.maxGridInstances);

                    var data = new MarchingCubeGlobalData();
                    data.Alloc(init.maxFreeGrids, shres);

                    em.AddComponentData(ent, data);
                    em.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();


            var gres = this.GetSingleton<MarchingCubeGlobalData>().ShaderResources;

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach(
                    (
                        Entity ent,
                        DotGridArea.InitializeData init
                    ) =>
                    {
                        Debug.Log("kita");
                        var mat = init.CubeMaterial;
                        var cs = init.GridToCubesShader;
                        var mesh = gres.mesh;

                        gres.SetResourcesTo(mat, cs);

                        var shres = new DotGridAreaGpuResources();
                        shres.Alloc(init.MaxCubeInstances, init.MaxGridInstructions);
                        shres.SetResourcesTo(mat, cs);
                        shres.SetArgumentBuffer(mesh);

                        var data = new DotGridArea.ResourceGpuModeData
                        {
                            CubeMaterial = mat,
                            GridToCubeShader = cs,
                            ShaderResources = shres,
                        };

                        em.AddComponentData(ent, data);
                        em.RemoveComponent<DotGridArea.InitializeData>(ent);
                    }
                )
                .Run();
        }

        //protected unsafe override void OnUpdate()
        //{ }



        protected override unsafe void OnDestroy()
        //protected override unsafe void OnStopRunning()
        {
            //if (!this.HasSingleton<MarchingCubeGlobalData>()) return;

            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach(
                    (
                        Entity entity,
                        DotGridArea.ResourceGpuModeData rea
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

                        rea.ShaderResources.Dispose();
                    }
                )
                .Run();

            globaldata.Dispose();
        }

    }
}
