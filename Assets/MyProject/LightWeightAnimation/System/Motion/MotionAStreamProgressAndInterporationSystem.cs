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
    
    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup ))]
    //[UpdateAfter(typeof())]
    public class MotionStreamProgressAndInterporationSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            //inputDeps = new StreamProgressAndInterporationJob
            //{
            //    DeltaTime = UnityEngine.Time.deltaTime,//Time.DeltaTime,
            //}
            //.Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile]
        struct StreamProgressAndInterporationJob : IJobForEach
            <Stream.CursorData, Stream.KeyShiftData, Stream.NearKeysCacheData, Stream.InterpolationData>
        {

            public float DeltaTime;


            public void Execute(
                ref Stream.CursorData timer,
                ref Stream.KeyShiftData shiftInfo,
                ref Stream.NearKeysCacheData nearKeys,
                [WriteOnly] ref Stream.InterpolationData dst
            )
            {
                timer.Cursor.Progress( DeltaTime );

                nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer.Cursor );

                var timeProgressNormalized = nearKeys.CaluclateTimeNormalized( timer.Cursor.CurrentPosition );

                dst.Interpolation = nearKeys.Interpolate( timeProgressNormalized );
            }

        }

    }

}
