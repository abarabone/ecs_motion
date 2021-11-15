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
using Unity.Transforms;
using System.Runtime.CompilerServices;

namespace DotsLite.MarchingCubes.another
{
    using DotsLite.MarchingCubes.another.Data;
    using DotsLite.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(DotGridLinksInitializeSystem))]
    [UpdateAfter(typeof(Gpu.MarchingCubesShaderResourceInitializeSystem))]
    public class BitGridInstantiateSystem : SystemBase
    {

        protected override unsafe void OnUpdate()
        {
            var em = this.EntityManager;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<BitGridArea.InitializeData>()
                .ForEach((
                    //Entity entity,
                    ref BitGridArea.GridInstructionIdData gids,
                    ref BitGridArea.GridLinkData glinks,
                    ref BitGridArea.GridInstructionIdSeedData seed,
                    in BitGridArea.GridTypeData type,
                    in BitGridArea.UnitDimensionData dim,
                    in Translation pos,
                    in BitGridArea.BitGridPrefabData prefab) =>
                {
                    var pfab = prefab.Prefab;
                    var t = type.UnitOnEdge;
                    var blen = prefab.BitLineBufferLength;
                    var span = dim.GridSpan.xyz;
                    var basepos = pos.Value;
                    create_(em, pfab, new int3(0, 0, 0), t, blen, span, ref glinks, ref gids, ref seed, basepos);
                    //create_(em, pfab, new int3(1, 0, 0), t, blen, span, ref glinks, ref gids, ref seed, basepos);
                    //create_(em, pfab, new int3(0, 0, 1), t, blen, span, ref glinks, ref gids, ref seed, basepos);
                    //create_(em, pfab, new int3(1, 0, 1), t, blen, span, ref glinks, ref gids, ref seed, basepos);
                })
                .Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void create_(EntityManager em, Entity prefab, int3 i,
            int unitOnEdge, uint bufferLength, int3 span,
            ref BitGridArea.GridLinkData glinks,
            ref BitGridArea.GridInstructionIdData gids,
            ref BitGridArea.GridInstructionIdSeedData seed,
            float3 basepos)
        {
            var index = new BitGrid.Tools.IndexInArea(i, span);

            var newent = em.Instantiate(prefab);

            gids.pId3dArray[index.serial] = seed.NextId++;
            glinks.pGrid3dArray[index.serial] = newent;

            var u = unitOnEdge;
            var hf = u >> 1;
            var pos = basepos + (i * u + new int3(hf, -hf, -hf));

            em.SetComponentData(newent, new BitGrid.BitLinesData().Alloc(bufferLength, GridFillMode.Blank));
            em.SetComponentData(newent, new BitGrid.LocationInAreaData
            {
                IndexInArea = index,
            });
            em.SetComponentData(newent, new DrawInstance.WorldBbox
            {
                Bbox = new AABB
                {
                    Center = pos,
                    Extents = hf,
                }
            });
            em.SetComponentData(newent, new Translation
            {
                Value = pos,
            });
        }

        protected override void OnDestroy()
        {
            this.Entities
                .WithoutBurst()
                .ForEach((ref DotGrid.Unit32Data grid) =>
                {
                    grid.Dispose();
                })
                .Run();

            this.Entities
                .WithoutBurst()
                .ForEach((ref DotGrid.Unit16Data grid) =>
                {
                    grid.Dispose();
                })
                .Run();
        }
    }
}
