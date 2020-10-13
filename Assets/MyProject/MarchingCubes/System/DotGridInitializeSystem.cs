using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridFillingSystem : BeginInitializationEntityCommandBufferSystem
    {
        protected override void OnUpdate()
        {
            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();
            var defaults = globaldata.DefaultGrids;

            var cmd = this.

            this.Entities
                .ForEach(
                    (
                        ref DotGridArea.BufferData buf,
                        in DotGridArea.InitializeData init,
                        in DotGridArea.InfoWorkData work,
                        in DotGridArea.InfoData info
                    ) =>
                    {

                        for(var i = 0; i < buf.Grids.length; i++)
                        {
                            buf.Grids[i] = defaults[(int)GridFillMode.Blank];
                        }

                        this.EntityManager.RemoveComponent<GridArea.>
                    }
                )
                .Run();

        }
    }
}
