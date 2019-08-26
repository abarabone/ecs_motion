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
    [UpdateAfter( typeof(MotionProgressSystem) )]
    public class MotionInitSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {

        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var isLeftClick = Input.GetMouseButtonDown(0);

            return inputDeps;
        }
    }





	public static class MotionUtility
	{

        static public void MotionInit
            (
                //EntityCommandBuffer ecb,
                EntityManager em,
                EntityArchetype motionArchetype, EntityArchetype streamArche,
                int motionIndex, MotionDataInNative md
            )
        {

            var ent = em.CreateEntity( motionArchetype );
            var ma = md.CreateAccessor( motionIndex );

            em.SetComponentData<MotionInfoData>
            (
                ent,
                new MotionInfoData
                {
                    MotionIndex = 0,
                    DataAccessor = ma
                }
            );
            //		//em.SetComponentData<DrawTargetSphere>
            //		//(
            //		//	ent,
            //		//	new DrawTargetSphere
            //		//	{
            //		//		center	= 0,
            //		//		radius	= 1
            //		//	}
            //		//);
            //		//em.SetComponentData<DrawModelInfo>
            //		//(
            //		//	ent,
            //		//	new DrawModelInfo
            //		//	{
            //		//		modelIndex	= 0
            //		//	}
            //		//);
            var streamEntities = new NativeArray<Entity>( ma.boneLength * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory );
            em.CreateEntity( streamArche, streamEntities );
            for( var i = 0; i < ma.boneLength * 2; ++i )
            {
                var timer = new StreamTimeProgressData
                {
                    TimeScale = 1.0f,
                    TimeLength = ma.TimeLength
                };
                var shifter = new StreamKeyShiftData
                {
                    Keys = ma.GetStreamSlice( i >> 2, KeyStreamSection.positions + ( i & 1 ) ).Keys
                };
                var cache = new StreamNearKeysCacheData();
                cache.InitializeKeys( ref shifter, ref timer );

                em.SetComponentData( streamEntities[i], timer );
                em.SetComponentData( streamEntities[i], shifter );
                em.SetComponentData( streamEntities[i], cache );
            }
    }


    }
}
