using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Physics;
using Unity.Physics.Extensions;

namespace DotsLite.HeightGrid
{
    using DotsLite.Misc;
    using DotsLite.Utilities;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    public class WaveCalculateSystem : SystemBase
    {

        GridMaster.Data gridMaster;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<GridMaster.Data>();
        }
        protected override void OnStartRunning()
        {
            //if (!this.HasSingleton<GridMaster.Data>()) return;

            this.gridMaster = this.GetSingleton<GridMaster.Data>();
            Debug.Log(this.gridMaster);

            //this.gridMaster.Currs[10] = -1f;
            //this.gridMaster.Currs[11] = -1f;
            //this.gridMaster.Currs[12] = -3f;
            //this.gridMaster.Currs[13] = -2f;
            //this.gridMaster.Currs[14] = -1f;
            //this.gridMaster.Currs[15] = -1f;
            //this.gridMaster.Currs[16] = -5f;
            //this.gridMaster.Currs[17] = 1f;
            //this.gridMaster.Currs[18] = 1f;
            //this.gridMaster.Currs[19] = 1f;
        }


        protected override unsafe void OnUpdate()
        {
            var dt = this.Time.DeltaTime;// * 3;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            this.Entities
                .WithoutBurst()
                .ForEach((ref GridMaster.HeightFieldData heights, in GridMaster.DimensionData dim) =>
                {
                    var span = dim.NumGrids * dim.UnitLengthInGrid;
                    var total = span.x * span.y;

                    var span4 = new int2(span.x >> 2, span.y);


                    this.Dependency = new WaveGridCaluclationJob
                    {
                        pNext = (float4*)gridMaster.Nexts.GetUnsafePtr(),
                        pCurr = (float4*)gridMaster.Currs.GetUnsafeReadOnlyPtr(),
                        pPrev = (float4*)gridMaster.Prevs.GetUnsafeReadOnlyPtr(),
                        span = span4,
                        harfsqdt = harfsqdt,
                        c2 = dim.Constraint2,
                        d = dim.Dumping,
                    }
                    .Schedule(total >> 2, 64, this.Dependency);
                    //this.Dependency = new WaveGridCaluclationSimpleJob
                    //{
                    //    Nexts = grid.Nexts,
                    //    Currs = grid.Currs.AsReadOnly(),
                    //    Prevs = grid.Prevs.AsReadOnly(),
                    //    span = span,
                    //    harfsqdt = harfsqdt,
                    //}
                    //.Schedule(total, 128, this.Dependency);

                })
                .Run();

        }

        [BurstCompile]
        unsafe struct WaveGridCaluclationJob : IJobParallelFor
        {
            //[ReadOnly]
            //public NativeArray<float4> Prevs;
            //[ReadOnly]
            //public NativeArray<float4> Currs;

            //public NativeArray<float4> Nexts;

            [NoAlias][ReadOnly]
            [NativeDisableUnsafePtrRestriction] public float4* pPrev;
            [NoAlias][ReadOnly]
            [NativeDisableUnsafePtrRestriction] public float4* pCurr;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pNext;

            public int2 span;
            public float harfsqdt;
            public float d;
            public float c2;


            [BurstCompile]
            public void Execute(int index)
            {
                //const float c2 = 0.8f;
                //const float d = 0.999f;

                var span = this.span;
                var mask = span - 1;

                var i = new int2(index & mask.x, index >> math.countbits(mask.x));


                int h(int iy) => i.x + (iy & mask.y) * span.x;
                int w(int ix) => (ix & mask.x) + i.y * span.x;

                var hc = this.pCurr[index];

                var hu = this.pCurr[h(i.y - 1)];
                var hd = this.pCurr[h(i.y + 1)];

                var hl = hc.wxyz;
                hl.x = this.pCurr[w(i.x - 1)].w;
                var hr = hc.yzwx;
                hr.w = this.pCurr[w(i.x + 1)].x;


                ////var iw = new int4(i.x - 1, i.x + 1, i.x, i.x);
                ////var ih = new int4(i.y, i.y, i.y - 1, i.y + 1);
                //var iw = i.xxxx + new int4(-1, 1, 0, 0);
                //var ih = i.yyyy + new int4(0, 0, -1, 1);
                //var iwh = (iw & mask.xxxx) + (ih & mask.yyyy) * span.xxxx;

                //var hc = this.pCurr[index];

                //var hu = this.pCurr[iwh.z];
                //var hd = this.pCurr[iwh.w];

                //var hl = hc.wxyz;
                //hl.x = this.pCurr[iwh.x].w;
                //var hr = hc.yzwx;
                //hr.w = this.pCurr[iwh.y].x;


                var hc2 = hc + hc;
                var aw = c2 * (hl + hr - hc2);
                var ah = c2 * (hu + hd - hc2);

                var hp = this.pPrev[index];
                var hn = hc + (hc - hp) * d + (aw + ah) * harfsqdt;

                this.pNext[index] = hn;
            }
        }

        [BurstCompile]
        struct WaveGridCaluclationSimpleJob : IJobParallelFor
        {
            [NoAlias]
            [ReadOnly]
            public NativeArray<float>.ReadOnly Prevs;
            [NoAlias]
            [ReadOnly]
            public NativeArray<float>.ReadOnly Currs;

            [NoAlias]
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<float> Nexts;

            public int2 span;
            public float harfsqdt;

            [BurstCompile]
            public void Execute(int index)
            {
                const float c2 = 0.8f;
                const float d = 0.999f;

                var span = this.span;
                var mask = span - 1;

                var i = new int2(index & mask.x, index >> math.countbits(mask.x));
                int _(int ix, int iy) => (ix & mask.x) + (iy & mask.y) * span.x;

                var hc = this.Currs[index];
                var hl = this.Currs[_(i.x - 1, i.y)];
                var hr = this.Currs[_(i.x + 1, i.y)];
                var hu = this.Currs[_(i.x, i.y - 1)];
                var hd = this.Currs[_(i.x, i.y + 1)];

                var aw = c2 * (hl - hc + hr - hc);
                var ah = c2 * (hu - hc + hd - hc);

                var hp = this.Prevs[index];
                var hn = hc + (hc - hp) * d + (aw + ah) * harfsqdt;

                this.Nexts[index] = hn;
            }
        }

    }
}
