using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

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
        }


        protected override void OnUpdate()
        {
            var dt = this.Time.DeltaTime;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            this.Dependency = new WaveGridJob
            {
                Prev = this.grid.PrevUnits,
                Next = this.grid.NextUnits,
            }
            .Schedule(this.grid.PrevUnits.Length, 1, this.Dependency);

        }

        struct WaveGridJob : IJobParallelFor
        {
            public NativeArray<WaveGridPrevPoint> Prev;
            public NativeArray<WaveGridNextPoint> Next;

            public void Execute(int index)
            {
                this.Prev[index] = new WaveGridPrevPoint { };
                this.Next[index] = new WaveGridNextPoint { };
            }
        }
    }
}
