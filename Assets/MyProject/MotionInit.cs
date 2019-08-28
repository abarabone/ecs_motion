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
    public class MotionInitSystem : ComponentSystem//JobComponentSystem
    {

        World entityCreationWorld;
        MotionDataInNative md;

        EntityArchetype motionArchetype;
        EntityArchetype streamArchetype;

        protected override void OnCreate()
        {
            this.entityCreationWorld = new World("entity creation world");
            
            var em = this.EntityManager;
            this.motionArchetype = em.CreateArchetype
            (
                typeof(MotionInfoData)
            );
            this.streamArchetype = em.CreateArchetype
            (
                typeof(StreamKeyShiftData), typeof(StreamTimeProgressData), typeof(StreamNearKeysCacheData)
            );

            //em.CreateEntity()
        }

        //protected override JobHandle OnUpdate( JobHandle inputDeps )
        protected override void OnUpdate()
        {
            var isLeftClick = Input.GetMouseButtonDown(0);
            if( !isLeftClick ) return;

            var motionIndex = 0;
            var ma = md.CreateAccessor( motionIndex );

            var em = this.entityCreationWorld.EntityManager;

            var ents = MotionUtility.CreateMotionEntities( em, motionArchetype, streamArchetype, ma );
            MotionUtility.InitMotion( em, ents.motionEntity, motionIndex, ma );
            MotionUtility.InitMotionStream( em, ents.steamEntities, motionIndex, ma );
        }
    }



    public static class a
    {
        static public EntityManager EntityManager { get; set; }
    }

	public static class MotionUtility
	{

        static public (Entity motionEntity, NativeArray<Entity> steamEntities)
            CreateMotionEntities
            ( EntityManager em, EntityArchetype motionArchetype, EntityArchetype streamArche, MotionDataAccessor ma )
        {
            var motionEntity = em.CreateEntity( motionArchetype );

            var streamEntities = new NativeArray<Entity>
                ( ma.boneLength * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory );
            em.CreateEntity( streamArche, streamEntities );

            return (motionEntity, streamEntities);
        }

        static public void InitMotion
            ( EntityManager em, Entity motionEntity, int motionIndex, MotionDataAccessor ma )
        {
            
            em.SetComponentData<MotionInfoData>
            (
                motionEntity,
                new MotionInfoData
                {
                    MotionIndex = motionIndex,
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
        }

        static public void InitMotionStream
            ( EntityManager em, NativeArray<Entity> streamEntities, int motionIndex, MotionDataAccessor ma )
        {
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
