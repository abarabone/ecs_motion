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

        protected override void OnUpdate()
        {

            var em = this.EntityManager;

            this.Entities
                .WithName("Global")
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
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach(
                    (
                        Entity ent,
                        DotGridArea.InitializeData init
                    ) =>
                    {
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

        // IDisposable な component data は、自動的に .Dispose() されるようだ

    }
}
