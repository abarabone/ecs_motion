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


            inputDeps = new MotionProgressJob
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
        struct MotionProgressJob : IJobForEach
            <MotionProgressTimerTag, MotionCursorData>
        {

            public float DeltaTime;


            public void Execute(
                //[ReadOnly] ref MotionInfoData info,
                //[ReadOnly] ref MotionClipData clip,
                [ReadOnly] ref MotionProgressTimerTag tag,
                ref MotionCursorData cursor
            )
            {

                progressTimeForLooping( ref cursor.Timer );

                cursor.Timer.Progress( this.DeltaTime );

            }


            void progressTimeForLooping(
                ref StreamTimeProgressData timer
            )
            {
                var isEndOfStream = timer.TimeProgress >= timer.TimeLength;

                var timeOffset = getTimeOffsetOverLength( in timer, isEndOfStream );

                timer.TimeProgress -= timeOffset;

                return;


                float getTimeOffsetOverLength( in StreamTimeProgressData progress_, bool isEndOfStream_ )
                {
                    return math.select( 0.0f, progress_.TimeLength, isEndOfStream_ );
                }
            }

        }
    }

}
