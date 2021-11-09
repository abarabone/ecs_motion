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

namespace DotsLite.MarchingCubes
{
    using MarchingCubes;
    using DotsLite.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(DotGridLinksInitializeSystem))]
    [UpdateAfter(typeof(Gpu.MarchingCubesShaderResourceInitializeSystem))]
    public class DotGridInstantiateSystem : SystemBase
    {

        protected override unsafe void OnUpdate()
        {
            var em = this.EntityManager;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<DotGridArea.InitializeData>()
                .ForEach((
                    Entity entity,
                    ref DotGridArea.LinkToGridData grids,
                    in DotGridArea.GridTypeData type,
                    in Translation pos,
                    in DotGridArea.DotGridPrefabData prefab) =>
                {
                    if (type.UnitOnEdge != 32) return;

                    var i = new int3(0, 0, 0);
                    create_<DotGrid32x32x32, DotGrid.Unit32Data>(prefab.Prefab, new int3(0, 0, 0), ref grids, pos, em);
                    create_<DotGrid32x32x32, DotGrid.Unit32Data>(prefab.Prefab, new int3(1, 0, 0), ref grids, pos, em);
                    //create_(prefab.Prefab, new int3(0, 0, 1), ref grids);
                    //create_(prefab.Prefab, new int3(1, 0, 1), ref grids);

                })
                .Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void create_<TGrid, TUnitData>(Entity prefab, int3 i, ref DotGridArea.LinkToGridData grids, Translation basepos, EntityManager em)
            where TGrid : struct, IDotGrid<TGrid>
            where TUnitData : struct, DotGrid.IUnitData<TGrid>, IComponentData
        {
            var index = new DotGrid.GridIndex().Set(i, grids.GridSpan);

            var newent = em.Instantiate(prefab);
            var grid = DotGrid.Create<TGrid>(GridFillMode.Blank);

            grids.pGridPoolIds[index.serial] = grids.nextSeed++;
            grids.ppGridXLines[index.serial] = grid.pXline;

            var u = grid.UnitOnEdge;
            var hf = u >> 1;
            var pos = basepos.Value + i * u + new int3(hf, -hf, -hf);

            em.SetComponentData(newent, new TUnitData()
            {
                Unit = grid,
            });
            em.SetComponentData(newent, new DotGrid.IndexData
            {
                GridIndexInArea = index,
                scale = 1.0f,
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
