﻿using System.Collections;
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
    public class MakeCubeInstancesSystem : SystemBase
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
                .WithNone<DotGridArea.Mode2>()//
                .WithNone<DotGridArea.Parallel>()//
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
                        output.GridInstances.Clear();
                        output.CubeInstances.Clear();


                        var gridId = 0;


                        // 0 は 1 以上との境界面を描くことが目的だが、0 同士の境界面が生成された場合、描画されてしまう、要考慮
                        for (var iy = 0; iy < dim.GridWholeLength.y - 1; iy++)
                            for (var iz = 0; iz < dim.GridWholeLength.z - 1; iz++)
                                for (var ix = 0; ix < dim.GridWholeLength.x - 1; ix++)
                                {

                                    var gridset = buf.getGridSet_(ix, iy, iz, unit.GridSpan);
                                    var gridcount = gridset.getEachCount();
                                    //Debug.Log($"{ix},{iy},{iz} {gridset.L.x.p->CubeCount}");

                                    if (!gridcount.isNeedDraw_()) continue;


                                    gridset.SampleAllCubes(ref gridcount, gridId, (uint*)output.CubeInstances.Ptr, ref output.CubeInstances.length);
                                    
                                    var data = new GridInstanceData
                                    {
                                        Position = new float4(pos.Value, 0.0f) +
                                            (new int4(ix, iy, iz, 0) - new int4(1, 1, 1, 0)) * new float4(32, -32, -32, 0)
                                    };
                                    output.GridInstances.AddNoResize(data);

                                    gridId++;

                                }

                        var gridScale = 1.0f / new float3(32, 32, 32);//Debug.Log(instance.GridInstances.length);
                        CubeUtility.GetNearGridList(output.GridInstances.AsNativeArray(), gridScale);
                    }
                )
                .Schedule();
                //.ScheduleParallel();

            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }
}
