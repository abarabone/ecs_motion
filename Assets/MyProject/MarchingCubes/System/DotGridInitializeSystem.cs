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
                //.WithBurst()
                .WithReadOnly(defaults)
                .ForEach(
                        (
                            ref DotGridArea.BufferData buf,
                            ref DotGridArea.InfoData dim,
                            ref DotGridArea.InfoWorkData unit
                        ) =>
                        {
                            var p = DotGridExtension.GetGrid(ref defaults, ref stocks, ref buf, ref unit, 0, 0, 0);

                            //Debug.Log(p.p->pUnits == null);
                            p[1, 0, 0] = 1;
                            p[1, 20, 10] = 1;

                            DotGridExtension.BackGridIfFilled(ref defaults, ref stocks, ref p);
                        }
                )
                .Run();

        }
    }
}
