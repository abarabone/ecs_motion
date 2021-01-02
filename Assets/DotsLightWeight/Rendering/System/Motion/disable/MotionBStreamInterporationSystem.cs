using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;



using Abarabone.SystemGroup;

namespace Abarabone.CharacterMotion
{
    
    [DisableAutoCreation]
    //[UpdateAfter(typeof(MotionProgressSystem))]//MotionB
    [UpdateBefore(typeof(StreamToBoneSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    public class MotionStreamInterporationSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>(isReadOnly: true);
            var motionCurrings = this.GetComponentDataFromEntity<Motion.DrawCullingData>(isReadOnly: true);

            var streamDeps = this.Entities
                .WithName("standard")
                .WithBurst()
                .WithReadOnly(motionCursors)
                .WithReadOnly(motionCurrings)
                .WithNone<Motion.SleepOnDrawCullingTag>()
                .ForEach(
                    (
                        ref Stream.DrawTargetData drawtarget,
                        ref Stream.KeyShiftData shiftInfo,
                        ref Stream.NearKeysCacheData nearKeys,
                        ref Stream.InterpolationData dst,
                        in Stream.MotionLinkData linker
                    ) =>
                    {
                        drawtarget.IsDrawTarget = motionCurrings[linker.MotionEntity].IsDrawTarget;


                        var cursor = motionCursors[linker.MotionEntity];

                        nearKeys.ShiftKeysIfOverKeyTimeForLooping(ref shiftInfo, ref cursor);

                        var timeProgressNormalized = nearKeys.CaluclateTimeNormalized(cursor.CurrentPosition);

                        dst.Interpolation = nearKeys.Interpolate(timeProgressNormalized);
                    }
                )
                .ScheduleParallel(this.Dependency);

            var streamSleepOnCullingDeps = this.Entities
                .WithName("SleepOnCulling")
                .WithBurst()
                .WithReadOnly(motionCursors)
                .WithReadOnly(motionCurrings)
                .WithAll<Motion.SleepOnDrawCullingTag>()
                .ForEach(
                    (
                        ref Stream.DrawTargetData drawtarget,
                        ref Stream.KeyShiftData shiftInfo,
                        ref Stream.NearKeysCacheData nearKeys,
                        ref Stream.InterpolationData dst,
                        in Stream.MotionLinkData linker
                    ) =>
                    {
                        drawtarget.IsDrawTarget = motionCurrings[linker.MotionEntity].IsDrawTarget;

                        if (!drawtarget.IsDrawTarget) return;


                        var cursor = motionCursors[linker.MotionEntity];

                        nearKeys.ShiftKeysIfOverKeyTimeForLooping(ref shiftInfo, ref cursor);

                        var timeProgressNormalized = nearKeys.CaluclateTimeNormalized(cursor.CurrentPosition);

                        dst.Interpolation = nearKeys.Interpolate(timeProgressNormalized);
                    }
                )
                .ScheduleParallel(this.Dependency);

            this.Dependency = JobHandle.CombineDependencies(streamDeps, streamSleepOnCullingDeps);
        }



        ///// <summary>
        ///// ストリーム回転 → 補間
        ///// </summary>
        //[BurstCompile]
        //struct StreamInterporationJob : IJobForEach
        //    <Stream.MotionLinkData, Stream.KeyShiftData, Stream.NearKeysCacheData, Stream.InterpolationData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<Motion.CursorData> MotionCursors;

        //    public void Execute(
        //        ref Stream.MotionLinkData linker,
        //        ref Stream.KeyShiftData shiftInfo,
        //        ref Stream.NearKeysCacheData nearKeys,
        //        [WriteOnly] ref Stream.InterpolationData dst
        //    )
        //    {
        //        var cursor = this.MotionCursors[ linker.MotionEntity ];

        //        nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref cursor );

        //        var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( cursor.CurrentPosition );

        //        dst.Interpolation = nearKeys.Interpolate( timeProgressNormalized );
        //    }

        //}

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
