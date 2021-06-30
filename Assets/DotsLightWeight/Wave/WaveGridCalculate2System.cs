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

namespace DotsLite.WaveGrid
{
    using DotsLite.Misc;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class WaveGridCalculate2System : SystemBase
    {

        WaveGridMasterData grid;


        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<WaveGridMasterData>();

            if (!this.HasSingleton<WaveGridMasterData>()) return;
            this.grid = this.GetSingleton<WaveGridMasterData>();
            Debug.Log(this.grid);

            this.grid.Currs[10] = -3f;
            this.grid.Currs[11] = -3f;
            this.grid.Currs[12] = -3f;
            this.grid.Currs[13] = -3f;
            this.grid.Currs[14] = -3f;
            this.grid.Currs[15] = -3f;
            this.grid.Currs[16] = -3f;
            this.grid.Currs[17] = -3f;
            this.grid.Currs[18] = -3f;
            this.grid.Currs[19] = -3f;
        }


        protected override unsafe void OnUpdate()
        {
            var dt = this.Time.DeltaTime * 5;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            var span = this.grid.NumGrids * this.grid.UnitLengthInGrid;
            var total = span.x * span.y;

            var span4 = new int2(span.x >> 2, span.y);


            this.Entities
                .ForEach((
                    WaveGrid.WaveGridData grid

                    ) =>
                {

                });

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

            [BurstCompile]
            public void Execute(int index)
            {
                const float c2 = 0.8f;
                const float d = 0.999f;

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

        [BurstCompile]
        unsafe struct WaveGridCopyJob : IJobParallelFor
        {
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pPrev;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pCurr;
            [NoAlias]
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction] public float4* pNext;
            //public NativeArray<float4> Prevs;

            //public NativeArray<float4> Currs;

            //[ReadOnly]
            //public NativeArray<float4> Nexts;

            [BurstCompile]
            public void Execute(int index)
            {
                var i = index << 1;
                this.pPrev[i + 0] = this.pCurr[i + 0];
                this.pCurr[i + 0] = this.pNext[i + 0];
                this.pPrev[i + 1] = this.pCurr[i + 1];
                this.pCurr[i + 1] = this.pNext[i + 1];
            }
        }

        [BurstCompile]
        unsafe struct WaveGridCopySimd256Avx2Job : IJobParallelFor
        {
            [NoAlias]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pPrev;
            [NoAlias]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pCurr;
            [NoAlias]
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pNext;

            [BurstCompile]
            public void Execute(int index)
            {
                if (X86.Avx2.IsAvx2Supported)
                {
                    var hc = X86.Avx2.mm256_stream_load_si256(this.pCurr + index);
                    var hn = X86.Avx2.mm256_stream_load_si256(this.pNext + index);
                    X86.Avx.mm256_stream_si256(this.pPrev + index, hc);
                    X86.Avx.mm256_stream_si256(this.pCurr + index, hn);
                }
            }
        }

        [BurstCompile]
        unsafe struct WaveGridCopySimd256AvxJob : IJobParallelFor
        {
            [NoAlias]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pPrev;
            [NoAlias]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pCurr;
            [NoAlias]
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            [NativeDisableUnsafePtrRestriction] public v256* pNext;

            [BurstCompile]
            public void Execute(int index)
            {
                if (X86.Avx.IsAvxSupported)
                {
                    var hc = X86.Avx.mm256_load_si256(this.pCurr + index);
                    var hn = X86.Avx.mm256_load_si256(this.pNext + index);
                    X86.Avx.mm256_stream_si256(this.pPrev + index, hc);
                    X86.Avx.mm256_stream_si256(this.pCurr + index, hn);
                }
            }
        }

        [BurstCompile]
        unsafe struct WaveGridCopyJob2 : IJobParallelFor
        {
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public void* pPrev;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public void* pCurr;
            [NoAlias] [ReadOnly]
            [NativeDisableUnsafePtrRestriction] public void* pNext;

            [BurstCompile]
            public void Execute(int index)
            {
                if (X86.Avx2.IsAvx2Supported)
                {
                    var pcurr = (v256*)this.pCurr;
                    var pprev = (v256*)this.pPrev;
                    var pnext = (v256*)this.pNext;
                    var hc = X86.Avx2.mm256_stream_load_si256(pcurr + index);
                    var hn = X86.Avx2.mm256_stream_load_si256(pnext + index);
                    X86.Avx.mm256_stream_si256(pprev + index, hc);
                    X86.Avx.mm256_stream_si256(pcurr + index, hn);
                }
                else if (X86.Avx.IsAvxSupported)
                {
                    var pcurr = (v256*)this.pCurr;
                    var pprev = (v256*)this.pPrev;
                    var pnext = (v256*)this.pNext;
                    var hc = X86.Avx.mm256_load_si256(pcurr + index);
                    var hn = X86.Avx.mm256_load_si256(pnext + index);
                    X86.Avx.mm256_stream_si256(pprev + index, hc);
                    X86.Avx.mm256_stream_si256(pcurr + index, hn);
                }
                else
                {
                    var i = index << 1;
                    var pcurr = (float4*)this.pCurr;
                    var pprev = (float4*)this.pPrev;
                    var pnext = (float4*)this.pNext;
                    pprev[i + 0] = pcurr[i + 0];
                    pcurr[i + 0] = pnext[i + 0];
                    pprev[i + 1] = pcurr[i + 1];
                    pcurr[i + 1] = pnext[i + 1];
                }
            }
        }
    }
}
