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
using System.Runtime.CompilerServices;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;
    using DotsLite.MarchingCubes.Data;
    using DotsLite.Draw;
    using DotsLite.Utilities;

    // game object が none active でも disable でコンバートされてしまうようなので、
    // メモリ確保等は system でやらないといけないのかも
    // render の shader まわりもかなぁ

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ResourceInitializeSystem : DependencyAccessableSystemBase
    {
        //CommandBufferDependency.Sender cmddep;

        protected override void OnCreate()
        {
            base.OnCreate();

            //this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }


        protected unsafe override void OnUpdate()
        {
            //using var cmdScope = this.cmddep.WithDependencyScope();

            //var cmd = cmdScope.CommandBuffer;//.AsParallelWriter();
            var em = this.EntityManager;

            this.Entities
                .WithName("Common")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    Common.InitializeData init,
                    Common.DrawShaderResourceData res,
                    in Common.AssetData asset) =>
                {
                    //res.Alloc(init);
                    res.Alloc(asset);

                    em.RemoveComponent<Common.InitializeData>(ent);
                })
                .Run();

            var commonres = this.GetSingleton<Common.DrawShaderResourceData>();
            this.Entities
                .WithName("CubeDrawModel")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((
                    Entity ent,
                    DrawModel.GeometryData geom,
                    DrawModel.ComputeArgumentsBufferData args,
                    CubeDrawModel.InitializeData init,
                    CubeDrawModel.MakeCubesShaderResourceData res) =>
                {
                    res.MakeCubesShader = init.cubeMakeShader;
                    res.XLineLengthPerGrid = init.unitOnEdge.z * init.unitOnEdge.y / (32/init.unitOnEdge.x);
                    res.Alloc(init);

                    var mesh = geom.Mesh;
                    var mat = geom.Material;
                    var cs = init.cubeMakeShader;
                    res.SetResourcesTo(mat, cs);
                    commonres.SetResourcesTo(mat);
                    args.SetInstancingArgumentBuffer(mesh);

                    em.RemoveComponent<CubeDrawModel.InitializeData>(ent);
                })
                .Run();

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity ent,
                    ref BitGridArea.GridLinkData glinks,
                    ref BitGridArea.GridInstructionIdData gids,
                    in BitGridArea.UnitDimensionData dim,
                    in BitGridArea.InitializeData init) =>
                {
                    glinks.Alloc(init.gridLength);
                    gids.Alloc(init.gridLength);
                    glinks.initGridLinks(dim, Entity.Null);
                    gids.initGridInstructionIds(dim);

                    em.RemoveComponent<BitGridArea.InitializeData>(ent);
                })
                .Run();
        }

        protected override void OnDestroy()
        {
            this.Entities
                .WithName("GridArea_Destroy")
                .WithoutBurst()
                .ForEach((ref BitGridArea.GridLinkData gridarr) =>
                {
                    gridarr.Dispose();
                })
                .Run();

            this.Entities
                .WithName("CubeDrawModel_Destroy")
                .WithoutBurst()
                .ForEach((CubeDrawModel.MakeCubesShaderResourceData res) =>
                {
                    res.Dispose();
                })
                .Run();

            this.Entities
                .WithName("Common_Destroy")
                .WithoutBurst()
                .ForEach((Common.DrawShaderResourceData res) =>
                {
                    res.Dispose();
                })
                .Run();

            base.OnDestroy();
        }

    }

    static class InitUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void initGridInstructionIds(
            ref this BitGridArea.GridInstructionIdData ids, in BitGridArea.UnitDimensionData dim)
        {
            var totalsize = dim.GridLength.x * dim.GridLength.y * dim.GridLength.z;

            for (var i = 0; i < totalsize; i++) ids.pId3dArray[i] = -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void initGridLinks(
            ref this BitGridArea.GridLinkData links, in BitGridArea.UnitDimensionData dim, Entity defaultGrid)
        {
            var totalsize = dim.GridLength.x * dim.GridLength.y * dim.GridLength.z;

            for (var i = 0; i < totalsize; i++) links.pGrid3dArray[i] = defaultGrid;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetInstancingArgumentBuffer(this DrawModel.ComputeArgumentsBufferData shaderArg, Mesh mesh)
        {
            var iargparams = new IndirectArgumentsForInstancing(mesh, 1);// 1 はダミー、0 だと怒られる
            shaderArg.InstancingArgumentsBuffer.SetData(ref iargparams);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetResourcesTo(this Common.DrawShaderResourceData res, Material mat)
        {
            // せつめい - - - - - - - - - - - - - - - - -

            //uint4 cube_patterns[ 254 ][2];
            // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
            // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

            //uint4 cube_vtxs[ 12 ];
            // x: near vertex index (x>>0 | y>>8 | z>>16)
            // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
            // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
            // w: pos(x>>0 | y>>8 | z>>16)

            // - - - - - - - - - - - - - - - - - - - - -

            mat.SetConstantBuffer_("static_data", res.GeometryElementData.Buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetResourcesTo(this CubeDrawModel.MakeCubesShaderResourceData res, Material mat, ComputeShader cs)
        {
            cs?.SetBuffer(0, "dotgrids", res.GridBitLines.Buffer);
            cs?.SetBuffer(0, "cube_instances", res.CubeInstances.Buffer);

            mat.SetBuffer("cube_instances", res.CubeInstances.Buffer);

            if (res.CubeIds.Texture == null) return;
            cs?.SetTexture(0, "dst_grid_cubeids", res.CubeIds.Texture);
            mat.SetTexture("grid_cubeids", res.CubeIds.Texture);
        }
    }

}
