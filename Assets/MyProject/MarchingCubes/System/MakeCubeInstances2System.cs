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
    using Abarabone.Draw;
    using Unity.Transforms;
    using Utilities;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    [UpdateBefore(typeof(BeginDrawCsBarier))]
    public class MakeCubeInstances2System : SystemBase
    {

        BeginDrawCsBarier presentationBarier;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }
        protected override unsafe void OnStartRunning()
        {
            base.OnStartRunning();

        }


        protected unsafe override void OnUpdate()
        {
            //var globaldata = this.GetSingleton<MarchingCubeGlobalData>();
            //var cubeInstances = globaldata.CubeInstances;
            //var gridInstances = globaldata.GridInstances;
            //cubeInstances.Clear();
            //gridInstances.Clear();

            this.Entities
                .WithAll<DotGridArea.Mode2>()//
                .WithBurst()
                .ForEach(
                        (
                            ref DotGridArea.OutputCubesData output,
                            in DotGridArea.BufferData buf,
                            in DotGridArea.InfoData dim,
                            in DotGridArea.InfoWorkData unit,
                            in Translation pos,
                            in Rotation rot
                        ) =>
                        {
                            var pDst = (uint4*)UnsafeUtility.Malloc(16 * 16 * 16, 32, Allocator.Temp);// output.CubeInstances.Ptr;
                            var dstIdx = 0;

                            var gridId = 0;


                            // 0 ‚Í 1 ˆÈã‚Æ‚Ì‹«ŠE–Ê‚ğ•`‚­‚±‚Æ‚ª–Ú“I‚¾‚ªA0 “¯m‚Ì‹«ŠE–Ê‚ª¶¬‚³‚ê‚½ê‡A•`‰æ‚³‚ê‚Ä‚µ‚Ü‚¤A—vl—¶
                            for (var iy = 0; iy < dim.GridWholeLength.y - 1; iy++)
                                for (var iz = 0; iz < dim.GridWholeLength.z - 1; iz++)
                                    for (var ix = 0; ix < dim.GridWholeLength.x - 1; ix++)
                                    {

                                        var i = math.dot(new int3(ix, iy, iz), unit.GridSpan);
                                        var grid = buf.Grids[i];

                                        if (grid.IsFullOrEmpty) continue;


                                        grid.SampleAlternateCubes(gridId, pDst, (uint*)output.CubeInstances.Ptr, ref output.CubeInstances.length);


                                        var data = new GridInstanceData
                                        {
                                            Position = new float4(pos.Value, 0.0f) +
                                                (new int4(ix, iy, iz, 0) - new int4(1, 1, 1, 0)) * new float4(32, -32, -32, 0)
                                        };
                                        output.GridInstances.AddNoResize(data);

                                        gridId++;

                                    }


                            var gridScale = 1.0f / new float3(32, 32, 32);
                            CubeUtility.GetNearGridList(output.GridInstances.AsNativeArray(), gridScale);

                            UnsafeUtility.Free(pDst, Allocator.Temp);
                        }
                )
                .Schedule();
            //.ScheduleParallel();

            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }
}
