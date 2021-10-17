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

namespace DotsLite.MarchingCubes.Gpu
{
    using MarchingCubes;
    using DotsLite.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MarchingCubesShaderResourceInitializeSystem : SystemBase
    {

        protected unsafe override void OnUpdate()
        //{ }

        //protected override void OnCreate()
        {

            var em = this.EntityManager;

            this.Entities
                .WithName("Global")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    Global.InitializeData init,
                    MarchingCubeGlobalData data) =>
                {
                    data.Alloc(init.asset, init.maxFreeGrids, init.maxGridInstances);

                    em.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();


            var gres = this.GetSingleton<MarchingCubeGlobalData>().ShaderResources;
            var pBlank = this.GetSingleton<MarchingCubeGlobalData>().DefaultGrids[(int)GridFillMode.Blank].pXline;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach(
                    (
                        Entity ent,
                        DotGridArea.InitializeData init,
                        DotGridArea.ResourceGpuModeData data,
                        DrawModel.GeometryData geom,
                        DotGridArea.LinkToGridData links
                    ) =>
                    {
                        var mat = geom.Material;//init.CubeMaterial;
                        var cs = init.GridToCubesShader;
                        var mesh = geom.Mesh;// gres.mesh;


                        data.CubeMaterial = mat;
                        data.GridToCubeShader = cs;
                        data.ShaderResources.Alloc(init.MaxCubeInstances, init.MaxGrids);


                        gres.SetResourcesTo(mat, cs);

                        data.ShaderResources.SetResourcesTo(mat, cs);
                        data.ShaderResources.SetArgumentBuffer(mesh);


                        var totalsize = links.GridLength.x * links.GridLength.y * links.GridLength.z;
                        var pIds = (int*)UnsafeUtility.Malloc(sizeof(int) * totalsize, 4, Allocator.Persistent);
                        for (var i = 0; i < totalsize; i++) pIds[i] = -1;
                        links.pGridIds = pIds;
                        var ppXLines = (uint**)UnsafeUtility.Malloc(sizeof(uint*) * totalsize, 4, Allocator.Persistent);
                        for (var i = 0; i < totalsize; i++) ppXLines[i] = pBlank;
                        links.ppGridXLines = ppXLines;


                        em.RemoveComponent<DotGridArea.InitializeData>(ent);
                    }
                )
                .Run();
        }

        // IDisposable な component data は、自動的に .Dispose() されるようだ

    }
}
