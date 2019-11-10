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
    public class MotionStreamInterporationSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new StreamInterporationJob
            {
                MotionCursors = this.GetComponentDataFromEntity<MotionCursorData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile]
        struct StreamInterporationJob : IJobForEach
            <StreamMotionLinkData, StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<MotionCursorData> MotionCursors;


            public void Execute(
                ref StreamMotionLinkData linker,
                ref StreamKeyShiftData shiftInfo,
                ref StreamNearKeysCacheData nearKeys,
                [WriteOnly] ref StreamInterpolatedData dst
            )
            {
                var timer = this.MotionCursors[ linker.MotionEntity ].Timer;

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( timer.TimeProgress );

                dst.Value = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

    }

}
