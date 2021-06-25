using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace DotsLite.WaveGrid
{
    using DotsLite.Misc;

    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class WaveGridSystem : SystemBase
    {

        WaveGridData grid;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<WaveGridData>();

            if (!this.HasSingleton<WaveGridData>()) return;
            this.grid = this.GetSingleton<WaveGridData>();
        }


        protected override void OnUpdate()
        {
            var dt = this.Time.DeltaTime;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            this.Dependency = new WaveGridJob
            {
                Units = this.grid.Units,
            }
            .Schedule(this.grid.Units.Length, 1, this.Dependency);

        }

        struct WaveGridJob : IJobParallelFor
        {
            public NativeArray<WaveUnit> Units;

            public void Execute(int index)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
