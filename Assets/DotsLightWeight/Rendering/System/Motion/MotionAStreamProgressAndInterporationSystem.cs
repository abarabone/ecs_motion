using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;



using DotsLite.SystemGroup;

namespace DotsLite.CharacterMotion
{

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    public class MotionStreamProgressAndInterporationSystem : SystemBase
    {

        
        protected override void OnUpdate()
        {
            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        ref Stream.CursorData timer,
                        ref Stream.KeyShiftData shiftInfo,
                        ref Stream.NearKeysCacheData nearKeys,
                        ref Stream.InterpolationData dst
                    )
                =>
                    {
                        timer.Cursor.Progress(deltaTime);

                        nearKeys.ShiftKeysIfOverKeyTimeForLooping(ref shiftInfo, ref timer.Cursor);
                        //nearKeys.ShiftKeysIfOverKeyTime(ref shiftInfo, in timer.Cursor);

                        var timeProgressNormalized = nearKeys.CaluclateTimeNormalized(timer.Cursor.CurrentPosition);

                        dst.Interpolation = nearKeys.Interpolate(timeProgressNormalized);
                    }
                )
                .ScheduleParallel();
        }

    }

}
