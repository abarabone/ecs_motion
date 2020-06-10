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
                var cursor = this.MotionCursors[ linker.MotionEntity ];

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref cursor );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( cursor.CurrentPosition );

                dst.Value = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

        //[BurstCompile]
        //struct StreamInterporationJob : IJobForEach
        //    <StreamMotionLinkData, StreamInterpolatedData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<MotionCursorData> MotionCursors;
        //    [ReadOnly]
        //    public ComponentDataFromEntity<MotionClipData> MotionClips;//

        //    public void Execute(
        //        ref StreamMotionLinkData linker,
        //        [WriteOnly] ref StreamInterpolatedData dst
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
