using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics.Systems;

namespace DotsLite.Misc
{

    //public static class TimerUtility
    //{

    //    static float PrevDeltaTime(this Unity.Core.TimeData timer) =>
    //        TimerSystem.prevDeltaTime;

    //    static float PrevDeltaTimeRcp(this Unity.Core.TimeData timer) =>
    //        TimerSystem.prevDeltaTimeRcp;

    //}

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //[UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class TimeEx : SystemBase
    {
        public static float PrevDeltaTime = 1.0f / 60.0f;
        public static float PrevDeltaTimeRcp = 1.0f / PrevDeltaTime;

        protected override void OnUpdate()
        {

            PrevDeltaTime = this.Time.DeltaTime;

            PrevDeltaTimeRcp = 1.0f / PrevDeltaTime;

        }
    }
}