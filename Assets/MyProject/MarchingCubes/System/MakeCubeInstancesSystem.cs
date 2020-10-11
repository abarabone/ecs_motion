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
    using Utilities;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    [UpdateBefore(typeof(BeginDrawCsBarier))]
    public class MakeCubeInstancesSystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<DotGridGlobal.InfoData>();
        }
        protected override unsafe void OnStartRunning()
        {
            var mcdata = this.GetSingleton<MarchingCubeGlobalData>();
            var globalent = this.GetSingletonEntity<DotGridGlobal.InfoData>();
            var defaults = this.GetBufferFromEntity<DotGridGlobal.DefualtGridData>();
            var stocks = this.GetBufferFromEntity<DotGridGlobal.FreeGridStockData>();

            var a = mcdata.FreeStocksPtr;
            this.Entities
                .WithBurst()
                .ForEach(
                        (
                            ref DotGridArea.BufferData buf,
                            ref DotGridArea.InfoData dim,
                            ref DotGridArea.InfoWorkData unit
                        ) =>
                        {
                            var x = a->Rent(GridFillMode.Blank);
                            //var def = defaults[globalent];
                            //var stock = stocks[globalent];

                            ////var x = (def, stock, buf, unit);
                            ////var p = x.GetGrid(0, 0, 0);
                            //var p = DotGridExtension.GetGrid(ref def, ref stock, ref buf, ref unit, 0, 0, 0);

                            //p[1, 0, 0] = 1;
                            //p[1, 20, 10] = 1;
                            //Debug.Log(p.p->CubeCount);

                            ////var z = (def, stock);
                            ////z.BackGridIfFilled(ref p);
                            //DotGridExtension.BackGridIfFilled(ref def, ref stock, ref p);

                            ////defaults[globalent] = def;
                            ////stocks[globalent] = stock;
                        }
                )
                .Schedule();
            //}
            //protected override void OnStartRunning()
            //{
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override void OnUpdate()
        {
            var globalent = this.GetSingletonEntity<DotGridGlobal.InfoData>();

            var instances = this.GetComponentDataFromEntity<DotGridGlobal.InstanceWorkData>();
            //var stocks = this.GetBufferFromEntity<DotGridGlobal.FreeGridStockData>();


            this.Entities
                //.WithBurst()
                .ForEach(
                        (
                            in DotGridArea.BufferData buf,
                            in DotGridArea.InfoData dim,
                            in DotGridArea.InfoWorkData unit
                        ) =>
                    {
                        var instance = instances[globalent];
                        instance.CubeInstances.Clear();// エリアごとにクリアしてたらあかんが、暫定
                        instance.GridInstances.Clear();
                        var gridId = 0;


                        // 0 は 1 以上との境界面を描くことが目的だが、0 同士の境界面が生成された場合、描画されてしまう、要考慮
                        for (var iy = 0; iy < dim.GridWholeLength.y - 1; iy++)
                            for (var iz = 0; iz < dim.GridWholeLength.z - 1; iz++)
                                for (var ix = 0; ix < dim.GridWholeLength.x - 1; ix++)
                                {

                                    var gridset = buf.getGridSet_(ix, iy, iz, unit.GridSpan);
                                    var gridcount = gridset.getEachCount();
                                    Debug.Log($"{ix},{iy},{iz} {gridset.L.x.p->CubeCount}");

                                    if (!gridcount.isNeedDraw_()) continue;
                                    //if( !isNeedDraw_( ref gridset ) ) continue;

                                    var dstCubeInstances = new InstanceCubeByUnsafeList { list = instance.CubeInstances };
                                    dstCubeInstances.SampleAllCubes(ref gridset, ref gridcount, gridId);
                                    //SampleAllCubes( ref gridset, gridId, dstCubeInstances );

                                    var data = new GridInstanceData
                                    {
                                        Position = (new int4(ix, iy, iz, 0) - new int4(1, 1, 1, 0)) * new float4(32, -32, -32, 0)
                                    };
                                    instance.GridInstances.AddNoResize(data);

                                    gridId++;

                                }

                        var gridScale = 1.0f / new float3(32, 32, 32);//Debug.Log(instance.GridInstances.length);
                        CubeUtility.GetNearGridList(instance.GridInstances.AsNativeArray(), gridScale);


                        instances[globalent] = instance;
                    }
                )
                .Run();//.Schedule();
                //.ScheduleParallel();

            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }
}
