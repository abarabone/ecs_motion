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

            this.Entities
                .WithBurst()
                .WithReadOnly(defaults)
                .WithAll<DotGridArea.InitializeData>()
                .ForEach(
                        (
                            ref DotGridArea.BufferData buf,
                            ref DotGridArea.InfoData dim,
                            ref DotGridArea.InfoWorkData unit
                        ) =>
                        {
                            for (var ig = 0; ig < 1; ig++)
                            {
                                var rnd = Unity.Mathematics.Random.CreateFromIndex((uint)ig+1);

                                var rg = rnd.NextInt3(0,4);
                                var p = DotGridExtension.GetGrid(ref defaults, ref stocks, ref buf, ref unit, ig, 0, 0);

                                var j = 0;
                                for (var ix = 0; ix < 32; ix++)
                                    for (var iy = 0; iy < 32; iy++)
                                        for (var iz = 0; iz < 32; iz++)
                                        {
                                            //if (!rnd.NextBool()) p[ix, iy, iz] = 1;
                                            if ((j++ & 1) == 0) p[ix, iy, iz] = 1;
                                        }

                                DotGridExtension.BackGridIfFilled(ref defaults, ref stocks, ref p);
                            }
                        }
                )
                .Run();

        }
    }
}
