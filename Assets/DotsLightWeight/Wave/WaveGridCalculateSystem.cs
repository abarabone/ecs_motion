using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

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

            this.grid.PrevUnits[00] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[11] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[12] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[13] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[14] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[15] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[16] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[17] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[18] = new WaveGridPrevPoint { Curr = -3f };
            this.grid.PrevUnits[19] = new WaveGridPrevPoint { Curr = -3f };
        }


        protected override void OnUpdate()
        {
            var dt = this.Time.DeltaTime * 5;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            var span = this.grid.NumGrids * this.grid.UnitLengthInGrid;
            var total = span.x * span.y;

            this.Dependency = new WaveGridCopyJob
            {
                Prev = this.grid.PrevUnits,
                Next = this.grid.NextUnits,
            }
            .Schedule(total, 1, this.Dependency);

            this.Dependency = new WaveGridCaluclationJob
            {
                Prev = this.grid.PrevUnits,
                Next = this.grid.NextUnits,
                span = span,
                harfsqdt = harfsqdt,
            }
            .Schedule(total, 1, this.Dependency);

        }

        struct WaveGridCaluclationJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<WaveGridPrevPoint> Prev;

            public NativeArray<WaveGridNextPoint> Next;

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

                var hc = this.Prev[index].Curr;
                var hl = this.Prev[_(i.x-1, i.y)].Curr;
                var hr = this.Prev[_(i.x+1, i.y)].Curr;
                var hu = this.Prev[_(i.x, i.y-1)].Curr;
                var hd = this.Prev[_(i.x, i.y+1)].Curr;

                var aw = c2 * (hl - hc + hr - hc);
                var ah = c2 * (hu - hc + hd - hc);

                var hp = this.Prev[index + 0].Prev;
                var hn = hc + (hc - hp) * d + (aw + ah) * harfsqdt;

                this.Next[index] = new WaveGridNextPoint { Next = hn };
            }
        }

        struct WaveGridCopyJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<WaveGridNextPoint> Next;

            public NativeArray<WaveGridPrevPoint> Prev;

            public void Execute(int index)
            {
                var hc = this.Prev[index].Curr;
                var hn = this.Next[index].Next;
                this.Prev[index] = new WaveGridPrevPoint { Curr = hn, Prev = hc };
            }
        }
    }
}
