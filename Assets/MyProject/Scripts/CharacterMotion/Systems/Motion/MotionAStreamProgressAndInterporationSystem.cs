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
    public class MotionStreamProgressAndInterporationSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new StreamProgressAndInterporationJob
            {
                DeltaTime = Time.deltaTime,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile]
        struct StreamProgressAndInterporationJob : IJobForEach
            <StreamCursorData, StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
        {

            public float DeltaTime;


            public void Execute(
                ref StreamCursorData timer,
                ref StreamKeyShiftData shiftInfo,
                ref StreamNearKeysCacheData nearKeys,
                [WriteOnly] ref StreamInterpolatedData dst
            )
            {
                timer.Cursor.Progress( DeltaTime );

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer.Cursor );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( timer.Cursor.CurrentPosition );

                dst.Value = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

    }

}
