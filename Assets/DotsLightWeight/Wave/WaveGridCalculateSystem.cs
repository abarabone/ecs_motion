using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

namespace DotsLite.WaveGrid
{
    using DotsLite.Misc;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class WaveGridCalculateSystem : SystemBase
    {

        WaveGridMasterData grid;


        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<WaveGridMasterData>();

            if (!this.HasSingleton<WaveGridMasterData>()) return;
            this.grid = this.GetSingleton<WaveGridMasterData>();
            Debug.Log(this.grid);

            this.grid.Currs[00] = -3f;
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

            this.Dependency = new WaveGridCopyJob
            {
                pNext = (float4*)grid.Nexts.GetUnsafeReadOnlyPtr(),
                pCurr = (float4*)grid.Currs.GetUnsafePtr(),
                pPrev = (float4*)grid.Prevs.GetUnsafePtr(),
            }
            .Schedule(total >> 2, 128, this.Dependency);

            //this.Dependency = new WaveGridCaluclationJob
            //{
            //    pNext = (float4*)grid.Nexts.GetUnsafePtr(),
            //    pCurr = (float4*)grid.Currs.GetUnsafeReadOnlyPtr(),
            //    pPrev = (float4*)grid.Prevs.GetUnsafeReadOnlyPtr(),
            //    span = span,
            //    harfsqdt = harfsqdt,
            //}
            //.Schedule(total >> 2, 128, this.Dependency);
            this.Dependency = new WaveGridCaluclationSimpleJob
            {
                Nexts = grid.Nexts,
                Currs = grid.Currs,
                Prevs = grid.Prevs,
                span = span,
                harfsqdt = harfsqdt,
            }
            .Schedule(total, 128, this.Dependency);

        }

        [BurstCompile]
        unsafe struct WaveGridCaluclationJob : IJobParallelFor
        {
            //[ReadOnly]
            //public NativeArray<float4> Prevs;
            //[ReadOnly]
            //public NativeArray<float4> Currs;

            //public NativeArray<float4> Nexts;

            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pPrev;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pCurr;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pNext;

            public int2 span;
            public float harfsqdt;

            public void Execute(int index)
            {
                const float c2 = 0.8f;
                const float d = 0.999f;

                var span = this.span;
                var mask = span - 1;

                var i = new int2(index & mask.x, index >> math.countbits(mask.x));
                int _(int ix, int iy) => (ix & mask.x) + (iy & mask.y) * span.x;

                var hc = this.pCurr[index];
                var hl = this.pCurr[_(i.x-1, i.y)];
                var hr = this.pCurr[_(i.x+1, i.y)];
                var hu = this.pCurr[_(i.x, i.y-1)];
                var hd = this.pCurr[_(i.x, i.y+1)];

                var aw = c2 * (hl - hc + hr - hc);
                var ah = c2 * (hu - hc + hd - hc);

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
            public NativeArray<float> Prevs;
            [NoAlias]
            [ReadOnly]
            public NativeArray<float> Currs;

            [NoAlias]
            public NativeArray<float> Nexts;

            public int2 span;
            public float harfsqdt;

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
            [NativeDisableUnsafePtrRestriction] public float4* pNext;
            //public NativeArray<float4> Prevs;

            //public NativeArray<float4> Currs;

            //[ReadOnly]
            //public NativeArray<float4> Nexts;

            public void Execute(int index)
            {
                var hc = this.pCurr[index];
                var hn = this.pNext[index];
                this.pPrev[index] = hc;
                this.pCurr[index] = hn;
            }
        }

        [BurstCompile]
        unsafe struct WaveGridCopy256Job : IJobParallelFor
        {
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public v2* pPrev;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pCurr;
            [NoAlias]
            [NativeDisableUnsafePtrRestriction] public float4* pNext;
            //public NativeArray<float4> Prevs;

            //public NativeArray<float4> Currs;

            //[ReadOnly]
            //public NativeArray<float4> Nexts;

            public void Execute(int index)
            {
                var hc = this.pCurr[index];
                var hn = this.pNext[index];
                this.pPrev[index] = hc;
                this.pCurr[index] = hn;
            }
        }
    }
}
