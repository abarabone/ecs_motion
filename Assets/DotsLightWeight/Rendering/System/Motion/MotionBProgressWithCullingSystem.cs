﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;



using Abarabone.SystemGroup;
using Abarabone.Draw;

namespace Abarabone.CharacterMotion
{

    /// <summary>
    /// ストリーム回転 → 補間
    /// </summary>
    [UpdateAfter(typeof(MotionBInitializeSystem))]
    //[UpdateBefore(typeof(MotionStreamInterporationSystem))]
    [UpdateBefore(typeof(MotionStreamInterporationWithCullingSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    public class MotionProgressWithCullingSystem : SystemBase//JobComponentSystem
    {


        protected override void OnUpdate()
        {

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .ForEach
                (
                    (ref Motion.CursorData cursor, in Motion.DrawCullingData cull) =>
                    {
                        if (!cull.IsDrawTarget) return;


                        progressTimeForLooping(ref cursor);

                        cursor.Progress(deltaTime);
                    }
                )
                .ScheduleParallel();

            return;


            void progressTimeForLooping( ref Motion.CursorData cousor )
            {
                var isEndOfStream = cousor.CurrentPosition >= cousor.TotalLength;

                var timeOffset = getTimeOffsetOverLength( in cousor, isEndOfStream );

                cousor.CurrentPosition -= timeOffset;

                return;


                float getTimeOffsetOverLength( in Motion.CursorData cursor_, bool isEndOfStream_ ) =>
                    math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
            }

        }


        //protected JobHandle OnUpdate( JobHandle inputDeps )
        //{


        //    inputDeps = new MotionProgressJob
        //    {
        //        DeltaTime = UnityEngine.Time.deltaTime,//Time.DeltaTime,
        //    }
        //    .Schedule( this, inputDeps );


        //    return inputDeps;
        //}



        ///// <summary>
        ///// ストリーム回転 → 補間
        ///// </summary>
        //[BurstCompile, RequireComponentTag(typeof(Motion.ProgressTimerTag))]
        //struct MotionProgressJob : IJobForEach
        //    <Motion.CursorData>
        //{

        //    public float DeltaTime;


        //    public void Execute(
        //        ref Motion.CursorData cursor
        //    )
        //    {

        //        progressTimeForLooping( ref cursor );

        //        cursor.Progress( this.DeltaTime );

        //    }


        //    void progressTimeForLooping(
        //        ref Motion.CursorData cousor
        //    )
        //    {
        //        var isEndOfStream = cousor.CurrentPosition >= cousor.TotalLength;

        //        var timeOffset = getTimeOffsetOverLength( in cousor, isEndOfStream );

        //        cousor.CurrentPosition -= timeOffset;

        //        return;


        //        float getTimeOffsetOverLength( in Motion.CursorData cursor_, bool isEndOfStream_ )
        //        {
        //            return math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
        //        }
        //    }

        //}
    }

}
