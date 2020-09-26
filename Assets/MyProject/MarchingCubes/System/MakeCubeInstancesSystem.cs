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
    using _ga = CubeGridArrayUnsafe;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    [UpdateBefore(typeof(BeginDrawCsBarier))]
    public class MakeCubeInstancesSystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;

        //NativeList<>

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override void OnUpdate()
        {
            var globalent = this.GetSingletonEntity<CubeGridGlobal.InfoData>();
            var globalInfo = this.EntityManager.GetComponentData<CubeGridGlobal.InfoData>(globalent);
            var globalDefaults = this.EntityManager.GetBuffer<CubeGridGlobal.DefualtGridData>(globalent);
            var globalStocks = this.EntityManager.GetBuffer<CubeGridGlobal.FreeGridStockData>(globalent);


            //[WriteOnly]
            var NativeList<CubeInstance> dstCubeInstances;
            ////[WriteOnly]
            var NativeList<CubeUtility.GridInstanceData> dstGridData;

            var dstCubeInstances = new NativeList<CubeInstance>();


            this.Entities
                .WithBurst()
                .ForEach(
                        (
                            in CubeGridArea.BufferData buf,
                            in CubeGridArea.InfoData dim,
                            in CubeGridArea.InfoWorkData unit
                        ) =>
                    {
                        var gridId = 0;

                        // 0 は 1 以上との境界面を描くことが目的だが、0 同士の境界面が生成された場合、描画されてしまう、要考慮
                        for (var iy = 0; iy < dim.GridWholeLength.y - 1; iy++)
                            for (var iz = 0; iz < dim.GridWholeLength.z - 1; iz++)
                                for (var ix = 0; ix < dim.GridWholeLength.x - 1; ix++)
                                {

                                    var gridset = _ga.getGridSet_(ref this.gridArray, ix, iy, iz, yspan, zspan);
                                    var gridcount = getEachCount(ref gridset);

                                    if (!isNeedDraw_(gridcount.L, gridcount.R)) continue;
                                    //if( !isNeedDraw_( ref gridset ) ) continue;

                                    var dstCubeInstances = new InstanceCubeByList { list = this.dstCubeInstances };
                                    SampleAllCubes(ref gridset, ref gridcount, gridId, ref dstCubeInstances);
                                    //SampleAllCubes( ref gridset, gridId, dstCubeInstances );

                                    var data = new CubeUtility.GridInstanceData
                                    {
                                        Position = (new int4(ix, iy, iz, 0) - new int4(1, 1, 1, 0)) * new float4(32, -32, -32, 0)
                                    };
                                    this.dstGridData.Add(data);

                                    gridId++;

                                }

                        var gridScale = 1.0f / new float3(32, 32, 32);
                        CubeUtility.GetNearGridList(this.dstGridData, gridScale);

                    }
                )
                .ScheduleParallel();

            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }
}
