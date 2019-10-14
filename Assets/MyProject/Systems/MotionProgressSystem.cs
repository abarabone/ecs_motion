using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;
using Abss.SystemGroup;

namespace Abss.Motion
{
    
    //[UpdateAfter(typeof())]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class MotionProgressSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new StreamInterpolationJob
            {
                DeltaTime = Time.deltaTime,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転　→補間→　ボーン
        /// </summary>
        [BurstCompile]
        struct StreamInterpolationJob : IJobForEach
            <StreamTimeProgressData, StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
        {

            public float DeltaTime;


            public void Execute(
                ref StreamTimeProgressData timer,
                ref StreamKeyShiftData shiftInfo,
                ref StreamNearKeysCacheData nearKeys,
                [WriteOnly] ref StreamInterpolatedData dst
            )
            {
                timer.Progress( DeltaTime );

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( timer.TimeProgress );

                dst.Value = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

    }

}
