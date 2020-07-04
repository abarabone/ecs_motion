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

namespace Abarabone.CharacterMotion
{
    
    //[UpdateAfter(typeof())]
    [UpdateAfter(typeof(MotionProgressSystem))]//MotionB
    [UpdateBefore(typeof(StreamToBoneSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup))]
    public class MotionStreamInterporationSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new StreamInterporationJob
            {
                MotionCursors = this.GetComponentDataFromEntity<Motion.CursorData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile]
        struct StreamInterporationJob : IJobForEach
            <Stream.MotionLinkData, Stream.KeyShiftData, Stream.NearKeysCacheData, Stream.InterpolationData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<Motion.CursorData> MotionCursors;

            public void Execute(
                ref Stream.MotionLinkData linker,
                ref Stream.KeyShiftData shiftInfo,
                ref Stream.NearKeysCacheData nearKeys,
                [WriteOnly] ref Stream.InterpolationData dst
            )
            {
                var cursor = this.MotionCursors[ linker.MotionEntity ];

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref cursor );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( cursor.CurrentPosition );

                dst.Interpolation = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

        //[BurstCompile]
        //struct StreamInterporationJob : IJobForEach
        //    <Stream.MotionLinkData, Stream.InterpolatedData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<Motion.CursorData> MotionCursors;
        //    [ReadOnly]
        //    public ComponentDataFromEntity<Motion.ClipData> MotionClips;//

        //    public void Execute(
        //        ref Stream.MotionLinkData linker,
        //        [WriteOnly] ref Stream.InterpolatedData dst
        //    )
        //    {
        //        var timer = this.MotionCursors[ linker.MotionEntity ].Timer;
        //        ref var clip = ref this.MotionClips[ linker.MotionEntity ].ClipData.Value;

        //        nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer );

        //        var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( timer.TimeProgress );

        //        dst.Value = nearKeys.Interpolate( timeProgressNormalized );
        //    }

        //}

    }

}
