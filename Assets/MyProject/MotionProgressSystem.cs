using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;

namespace Abss.Motion
{


    [UpdateInGroup( typeof( MotionGroup ) )]
    public class MotionProgressSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {

        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            return inputDeps;
        }
    }




    //	//static EntityArchetype CreateArchetypes( EntityManager em ) =>
    //	//	em.CreateArchetype
    //	//	(
    //	//		ComponentType..Create<MotionInfoData>(),
    //	//		ComponentType.Create<MotionStreamElement>(),
    //	//		ComponentType.Create<DrawTargetSphere>(),
    //	//		ComponentType.Create<DrawModelInfo>()
    //	//	);
    //}



    ///// <summary>
    ///// ストリーム回転　→補間→　ボーン
    ///// </summary>
    //[BurstCompile]
    //struct StreamInterpolateJob : IJobForEach
    //	<StreamTimeProgressData, StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
    //{

    //	public float	DeltaTime;


    //	public void Execute
    //		(
    //			ref StreamTimeProgressData timer,
    //			ref StreamKeyShiftData shiftInfo,
    //			ref StreamNearKeysCacheData nearKeys,
    //			[WriteOnly] ref StreamInterpolatedData dst
    //		)
    //	{
    //		timer.Progress( DeltaTime );

    //		nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer );

    //		var timeProgressNormalized	= nearKeys.CaluclateTimeNormalized( timer.TimeProgress );

    //		dst.Value = nearKeys.Interpolate( timeProgressNormalized );
    //	}

    //}

}
