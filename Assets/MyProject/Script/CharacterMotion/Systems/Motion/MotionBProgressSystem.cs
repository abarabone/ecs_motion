using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abarabone.Authoring;
using Abarabone.SystemGroup;

namespace Abarabone.Motion
{

    /// <summary>
    /// ストリーム回転 → 補間
    /// </summary>
    //[UpdateAfter(typeof())]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup))]
    public class MotionProgressSystem : SystemBase//JobComponentSystem
    {


        protected override void OnUpdate()
        {

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithName( "MotionProgressSystem" )
                .WithBurst()
                .ForEach
                (
                    (ref MotionCursorData cursor) =>
                    {
                        progressTimeForLooping( ref cursor );

                        cursor.Progress( deltaTime );
                    }
                )
                .ScheduleParallel();

            return;


            void progressTimeForLooping( ref MotionCursorData cousor )
            {
                var isEndOfStream = cousor.CurrentPosition >= cousor.TotalLength;

                var timeOffset = getTimeOffsetOverLength( in cousor, isEndOfStream );

                cousor.CurrentPosition -= timeOffset;

                return;


                float getTimeOffsetOverLength( in MotionCursorData cursor_, bool isEndOfStream_ ) =>
                    math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
            }

        }


        protected JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new MotionProgressJob
            {
                DeltaTime = UnityEngine.Time.deltaTime,//Time.DeltaTime,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile, RequireComponentTag(typeof(MotionProgressTimerTag))]
        struct MotionProgressJob : IJobForEach
            <MotionCursorData>
        {

            public float DeltaTime;


            public void Execute(
                ref MotionCursorData cursor
            )
            {

                progressTimeForLooping( ref cursor );

                cursor.Progress( this.DeltaTime );

            }


            void progressTimeForLooping(
                ref MotionCursorData cousor
            )
            {
                var isEndOfStream = cousor.CurrentPosition >= cousor.TotalLength;

                var timeOffset = getTimeOffsetOverLength( in cousor, isEndOfStream );

                cousor.CurrentPosition -= timeOffset;

                return;


                float getTimeOffsetOverLength( in MotionCursorData cursor_, bool isEndOfStream_ )
                {
                    return math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
                }
            }

        }
    }

}
