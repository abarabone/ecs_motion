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
    public class DotGridFillingSystem : Common.BeginInitializationCommandSystemBase
    {
        protected override void OnUpdate()
        {
            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();
            var defaults = globaldata.DefaultGrids;

            var cmd = this.cmdSystem.CreateCommandBuffer();

            this.Entities
                .WithBurst()
                .WithReadOnly(defaults)
                .ForEach(
                    (
                        Entity ent,
                        ref DotGridArea.BufferData buf,
                        in DotGridArea.InitializeData init,
                        in DotGridArea.InfoWorkData work,
                        in DotGridArea.InfoData info
                    ) =>
                    {

                        for (var i = 0; i < buf.Grids.length; i++)
                        {
                            buf.Grids[i] = defaults[(int)init.FillMode];
                        }

                        cmd.RemoveComponent<DotGridArea.InitializeData>(ent);
                    }
                )
                .Run();


            var stocks = globaldata.FreeStocks;
            var rnd = Unity.Mathematics.Random.CreateFromIndex(0);

            this.Entities
                //.WithBurst()
                .WithReadOnly(defaults)
                .WithAll<DotGridArea.InitializeData>()
                .ForEach(
                        (
                            ref DotGridArea.BufferData buf,
                            ref DotGridArea.InfoData dim,
                            ref DotGridArea.InfoWorkData unit
                        ) =>
                        {
                            for (var ig = 0; ig < 32; ig++)
                            {
                                var rg = rnd.NextInt3() & (4-1);
                                var p = DotGridExtension.GetGrid(ref defaults, ref stocks, ref buf, ref unit, rg.x, rg.y, rg.z);

                                for (var i = 0; i < 32; i++)
                                {
                                    var r = rnd.NextInt3() & (32 - 1);
                                    p[r.x, r.y, r.z] = 1;
                                }

                                DotGridExtension.BackGridIfFilled(ref defaults, ref stocks, ref p);
                            }
                        }
                )
                .Run();

        }
    }
}
