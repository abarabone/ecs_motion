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
                .WithAll<Gpu.DotGridArea.InitializeData>()
                .ForEach(
                    (
                        Entity entity,
                        ref DotGridArea.LinkToGridData grid,
                        in DotGridArea.DotGridPrefabData prefab
                    ) =>
                    {




                        var newent = em.Instantiate(prefab.Prefab);

                        em.SetComponentData(entity, new DotGrid.UnitData
                        {
                            GridIndexInArea = new int3(0, 0, 0),
                            Unit = new DotGrid32x32x32Unsafe(GridFillMode.Blank),
                        });
                    }
                )
                .Run();
        }

    }
}
