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

namespace DotsLite.MarchingCubes.another
{
    using DotsLite.Dependency;
    using DotsLite.MarchingCubes.another.Data;
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
                .WithName("common")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    Common.InitializeData init,
                    Common.DrawShaderResourceData res) =>
                {
                    Data.Resource.CubeGeometryConstantBuffer.Create(init.asset);

                    //em.RemoveComponent<Global.InitializeData>(ent);
                    cmd.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();

            this.Entities
                .WithName("drawModel")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    DrawModel.MakeCubesShaderResourceData res) =>
                {
                    Data.Resource.CubeGeometryConstantBuffer.Create(init.asset);

                    //em.RemoveComponent<Global.InitializeData>(ent);
                    cmd.RemoveComponent<Global.InitializeData>(ent);
                })
                .Run();

            //var globalcommon = this.GetSingleton<Global.CommonData>();
            //var globalwork32 = this.GetSingleton<Global.Work32Data>();
            //var globalwork16 = this.GetSingleton<Global.Work16Data>();
            ////var gres = globalwork32.ShaderResources;
            //var pBlank32 = globalwork32.DefaultGrids[(int)GridFillMode.Blank].pXline;
            //var pBlank16 = globalwork16.DefaultGrids[(int)GridFillMode.Blank].pXline;

            //this.Entities
            //    .WithName("GridArea")
            //    .WithoutBurst()
            //    .WithStructuralChanges()
            //    .ForEach((
            //        Entity ent,
            //        DotGridArea.InitializeData init,
            //        DotGridArea.ResourceGpuModeData data,
            //        DrawModel.GeometryData geom,
            //        ref DotGridArea.LinkToGridData links,
            //        in DotGridArea.GridTypeData type,
            //        in DrawModel.ComputeArgumentsBufferData shaderArg) =>
            //    {
            //        var mat = geom.Material;
            //        var mesh = geom.Mesh;
            //        var cs = init.GridToCubesShader;

            //        switch (type.UnitOnEdge)
            //        {
            //            case 32:
            //                Debug.Log("mc sh res ini 32");
            //                links.AllocGridAreaBuffers(pBlank32);
            //                data.ShaderResources.Alloc(init.MaxCubeInstances, 32, init.MaxGrids);

            //                globalcommon.ShaderResources.SetResourcesTo(mat);
            //                globalwork32.ShaderResources.SetResourcesTo(mat, cs);
            //                break;

            //            case 16:
            //                Debug.Log("mc sh res ini 16");
            //                links.AllocGridAreaBuffers(pBlank16);
            //                data.ShaderResources.Alloc(init.MaxCubeInstances, 16, init.MaxGrids);

            //                globalcommon.ShaderResources.SetResourcesTo(mat);
            //                globalwork16.ShaderResources.SetResourcesTo(mat, cs);
            //                break;

            //            default:
            //                break;
            //        }

            //        data.GridToCubeShader = cs;
            //        data.ShaderResources.SetResourcesTo(mat, cs);
            //        shaderArg.SetInstancingArgumentBuffer(mesh);


            //        //em.RemoveComponent<DotGridArea.InitializeData>(ent);
            //        cmd.RemoveComponent<DotGridArea.InitializeData>(ent);
            //    })
            //    .Run();
        }

        protected override void OnDestroy()
        {
            //this.Entities
            //    .WithName("GridArea_Destroy")
            //    .WithoutBurst()
            //    .ForEach((
            //        DotGridArea.ResourceGpuModeData data,
            //        ref DotGridArea.LinkToGridData links) =>
            //    {
            //        data.Dispose();
            //        links.Dispose();
            //    })
            //    .Run();

            this.Entities
                .WithName("Global_Destroy")
                .WithoutBurst()
                .ForEach((
                    Common.DrawShaderResourceData res) =>
                {
                    res.GeometryElementData.Dispose();
                })
                .Run();

            base.OnDestroy();
        }

    }

    //static class InitUtility
    //{
    //    public static void SetInstancingArgumentBuffer(this DrawModel.ComputeArgumentsBufferData shaderArg, Mesh mesh)
    //    {
    //        var iargparams = new IndirectArgumentsForInstancing(mesh, 1);// 1 はダミー、0 だと怒られる
    //        shaderArg.InstancingArgumentsBuffer.SetData(ref iargparams);
    //    }

    //    public unsafe static void AllocGridAreaBuffers(ref this DotGridArea.LinkToGridData links, uint *pBlank)
    //    {
    //        var totalsize = links.GridLength.x * links.GridLength.y * links.GridLength.z;

    //        var pIds = (int*)UnsafeUtility.Malloc(sizeof(int) * totalsize, 4, Allocator.Persistent);
    //        for (var i = 0; i < totalsize; i++) pIds[i] = -1;
    //        links.pGridPoolIds = pIds;

    //        var ppXLines = (uint**)UnsafeUtility.Malloc(sizeof(uint*) * totalsize, 4, Allocator.Persistent);
    //        for (var i = 0; i < totalsize; i++) ppXLines[i] = pBlank;
    //        links.ppGridXLines = ppXLines;
    //    }
    //}


    //public partial struct CommonShaderResources
    //{
    //    public void SetResourcesTo(Material mat)
    //    {
    //        // せつめい - - - - - - - - - - - - - - - - -

    //        //uint4 cube_patterns[ 254 ][2];
    //        // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
    //        // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

    //        //uint4 cube_vtxs[ 12 ];
    //        // x: near vertex index (x>>0 | y>>8 | z>>16)
    //        // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
    //        // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
    //        // w: pos(x>>0 | y>>8 | z>>16)

    //        // - - - - - - - - - - - - - - - - - - - - -

    //        mat.SetConstantBuffer_("static_data", this.CubeGeometryConstants.Buffer);
    //    }
    //}
    //public partial struct WorkingShaderResources
    //{
    //    public void SetResourcesTo(Material mat, ComputeShader cs)
    //    {
    //        mat.SetTexture("grid_cubeids", this.GridCubeIds.Texture);

    //        cs?.SetTexture(0, "dst_grid_cubeids", this.GridCubeIds.Texture);
    //    }
    //}
    //public partial struct DotGridAreaGpuResources
    //{
    //    public void SetResourcesTo(Material mat, ComputeShader cs)
    //    {
    //        cs?.SetBuffer(0, "dotgrids", this.GridDotContentDataBuffer.Buffer);
    //        cs?.SetBuffer(0, "cube_instances", this.CubeInstances.Buffer);
    //        //cs?.SetBuffer(0, "grid_instructions", this.GridInstructions.Buffer);

    //        mat.SetBuffer("cube_instances", this.CubeInstances.Buffer);
    //        //mat.SetBuffer("grid_instructions", this.GridInstructions.Buffer);
    //        //mat.SetConstantBuffer_("grid_constant", this.GridInstructions.Buffer);
    //    }
    //}
}
