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

namespace DotsLite.MarchingCubes
{
    using MarchingCubes;
    using DotsLite.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(DotGridLinksInitializeSystem))]
    public class InstantiateDotGridSystem : SystemBase
    {

        protected override unsafe void OnUpdate()
        {
            var em = this.EntityManager;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<DotGridArea.InitializeData>()
                .ForEach(
                    (
                        Entity entity,
                        ref DotGridArea.LinkToGridData grid,
                        in DotGridArea.DotGridPrefabData prefab
                    ) =>
                    {


                        var i = new int3(0, 0, 0);
                        create_(prefab.Prefab, new int3(0, 0, 0), ref grid);
                        //create_(prefab.Prefab, new int3(1, 0, 0), ref grid);
                        //create_(prefab.Prefab, new int3(0, 0, 1), ref grid);
                        //create_(prefab.Prefab, new int3(1, 0, 1), ref grid);
                        return;


                        void create_(Entity prefab, int3 i, ref DotGridArea.LinkToGridData grid)
                        {
                            var index = new DotGrid.GridIndex().Set(i, grid.GridSpan);

                            grid.pGridIds[index.serial] = grid.nextSeed++;

                            var newent = em.Instantiate(prefab);

                            em.SetComponentData(newent, new DotGrid.UnitData
                            {
                                GridIndexInArea = index,
                                Unit = new DotGrid32x32x32Unsafe(GridFillMode.Blank),
                            });
                            em.SetComponentData(newent, new DrawInstance.WorldBbox
                            {
                                Bbox = new AABB
                                {
                                    Center = i * 32 + new float3(16, -16, -16),
                                    Extents = new float3(32 / 2, 32 / 2, 32 / 2),
                                }
                            });
                            em.SetComponentData(newent, new Translation
                            {
                                Value = i * 32,
                            });
                        }
                    }
                )
                .Run();
        }

    }
}
