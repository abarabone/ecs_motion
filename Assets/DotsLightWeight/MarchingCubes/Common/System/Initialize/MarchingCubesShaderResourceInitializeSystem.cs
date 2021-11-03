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

namespace DotsLite.MarchingCubes.Gpu
{
    using DotsLite.Dependency;
    using MarchingCubes;
    using DotsLite.Draw;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MarchingCubesShaderResourceInitializeSystem : DependencyAccessableSystemBase
    {
        CommandBufferDependency.Sender cmddep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }


        protected unsafe override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();

            var cmd = cmdScope.CommandBuffer;//.AsParallelWriter();
            //var em = this.EntityManager;
            

            this.Entities
                .WithName("Global")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    Global.InitializeData init,
                    Global.CommonData common,
                    Global.Work32Data work32,
                    Global.Work16Data work16) =>
                {
                    common.Alloc(init.asset);
                    work32.Alloc(init.maxGridInstances);
                    work16.Alloc(init.maxGridInstances);

                    //em.RemoveComponent<Global.InitializeData>(ent);
                    cmd.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();


            var globalcommon = this.GetSingleton<Global.CommonData>();
            var globalwork32 = this.GetSingleton<Global.Work32Data>();
            var globalwork16 = this.GetSingleton<Global.Work16Data>();
            //var gres = globalwork32.ShaderResources;
            var pBlank32 = globalwork32.DefaultGrids[(int)GridFillMode.Blank].pXline;
            var pBlank16 = globalwork16.DefaultGrids[(int)GridFillMode.Blank].pXline;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    DotGridArea.InitializeData init,
                    DotGridArea.ResourceGpuModeData data,
                    DrawModel.GeometryData geom,
                    ref DotGridArea.LinkToGridData links,
                    in DotGridArea.GridTypeData type,
                    in DrawModel.ComputeArgumentsBufferData shaderArg) =>
                {
                    var mat = geom.Material;//init.CubeMaterial;
                    var mesh = geom.Mesh;// gres.mesh;
                    var cs = init.GridToCubesShader;

                    switch (type.UnitOnEdge)
                    {
                        case 32:
                            Debug.Log("mc sh res ini 32");
                            links.AllocGridAreaBuffers(pBlank32);
                            data.ShaderResources.Alloc(init.MaxCubeInstances, 32, init.MaxGrids);

                            globalcommon.ShaderResources.SetResourcesTo(mat);
                            globalwork32.ShaderResources.SetResourcesTo(mat, cs);
                            break;

                        case 16:
                            Debug.Log("mc sh res ini 16");
                            links.AllocGridAreaBuffers(pBlank16);
                            data.ShaderResources.Alloc(init.MaxCubeInstances, 16, init.MaxGrids);

                            globalcommon.ShaderResources.SetResourcesTo(mat);
                            globalwork16.ShaderResources.SetResourcesTo(mat, cs);
                            break;

                        default:
                            break;
                    }

                    //data.CubeMaterial = mat;
                    data.GridToCubeShader = cs;
                    data.ShaderResources.SetResourcesTo(mat, cs);
                    shaderArg.SetInstancingArgumentBuffer(mesh);


                    //em.RemoveComponent<DotGridArea.InitializeData>(ent);
                    cmd.RemoveComponent<DotGridArea.InitializeData>(ent);
                })
                .Run();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Entities
                .WithName("Global_Destroy")
                .WithoutBurst()
                .ForEach((
                    Global.CommonData common,
                    Global.Work32Data work32,
                    Global.Work16Data work16) =>
                {
                    common.Dispose();
                    work32.Dispose();
                    work16.Dispose();
                })
                .Run();

            this.Entities
                .WithName("GridArea_Destroy")
                .WithoutBurst()
                .ForEach((
                    DotGridArea.ResourceGpuModeData data,
                    in DotGridArea.LinkToGridData links) =>
                {
                    data.Dispose();
                    links.Dispose();
                })
                .Run();
        }

    }

    static class InitUtility
    {
        public static void SetInstancingArgumentBuffer(this DrawModel.ComputeArgumentsBufferData shaderArg, Mesh mesh)
        {
            var iargparams = new IndirectArgumentsForInstancing(mesh, 1);// 1 はダミー、0 だと怒られる
            shaderArg.InstancingArgumentsBuffer.SetData(ref iargparams);
        }

        public unsafe static void AllocGridAreaBuffers(ref this DotGridArea.LinkToGridData links, uint *pBlank)
        {
            var totalsize = links.GridLength.x * links.GridLength.y * links.GridLength.z;

            var pIds = (int*)UnsafeUtility.Malloc(sizeof(int) * totalsize, 4, Allocator.Persistent);
            for (var i = 0; i < totalsize; i++) pIds[i] = -1;
            links.pGridPoolIds = pIds;

            var ppXLines = (uint**)UnsafeUtility.Malloc(sizeof(uint*) * totalsize, 4, Allocator.Persistent);
            for (var i = 0; i < totalsize; i++) ppXLines[i] = pBlank;
            links.ppGridXLines = ppXLines;
        }
    }
}
