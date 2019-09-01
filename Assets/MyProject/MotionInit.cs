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
    //[UpdateAfter( typeof(MotionProgressSystem) )]
    [AlwaysUpdateSystem]
    public class ChCreationSystem : JobComponentSystem
    {

        //MotionDataInNative md;

        EntityArchetype motionArchetype;
        EntityArchetype streamArchetype;

        static public MotionDataInNative md;


        protected override void OnCreate()
        {

            EntityCreation.CreateNewWorld();

            createArchetypes( this.EntityManager );

            dummyEntityCreationAndDestory( this.EntityManager, EntityCreation.World.EntityManager );

            return;


            void createArchetypes( EntityManager em )
            {
                this.motionArchetype = em.CreateArchetype
                (
                    typeof(MotionInfoData)
                );
                this.streamArchetype = em.CreateArchetype
                (
                    typeof(StreamKeyShiftData), typeof(StreamTimeProgressData), typeof(StreamNearKeysCacheData)
                );
            }

            void dummyEntityCreationAndDestory( EntityManager em_main, EntityManager em_creation )
            {
                em_main.CreateEntity( this.motionArchetype );
                em_main.CreateEntity( this.streamArchetype );

                em_creation.MoveEntitiesFrom( out var ents, em_main );
                em_creation.DestroyEntity( ents );
                ents.Dispose();
            }
        }

        protected override void OnDestroy()
        {
            //EntityCreation.World.Dispose();
            ChCreationSystem.md.Dispose();
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        //protected override void OnUpdate()
        {
            var isLeftClick = Input.GetMouseButtonDown(1);
            if( !isLeftClick ) return inputDeps;

            Debug.Log("click");
            var motionIndex = 0;
            var ma = ChCreationSystem.md.CreateAccessor( motionIndex );
            
            var em = EntityCreation.World.EntityManager;
            var eet = em.BeginExclusiveEntityTransaction();
            eet.

            inputDeps = new ChCreateJob
            {

            }
            .Schedule( inputDeps );
            //var ents = MotionUtility.CreateMotionEntities( eet, motionArchetype, streamArchetype, ma );
            //MotionUtility.InitMotion( eet, ents.motionEntity, motionIndex, ma );
            //MotionUtility.InitMotionStream( eet, ents.steamEntities, motionIndex, ma );

            em.EndExclusiveEntityTransaction();

            this.EntityManager.MoveEntitiesFrom( em );
            ents.steamEntities.Dispose();
        }
    }

    public struct ChCreateJob : IJob
    {
        public ExclusiveEntityTransaction eet;
        public EntityArchetype motionArchetype;
        public EntityArchetype streamArchetype;
        public int motionIndex;
        public MotionDataAccessor ma;

        public void Execute()
        {
            var ents = MotionUtility.CreateMotionEntities( eet, motionArchetype, streamArchetype, ma );
            MotionUtility.InitMotion( eet, ents.motionEntity, motionIndex, ma );
            MotionUtility.InitMotionStream( eet, ents.steamEntities, motionIndex, ma );
            
        }
    }



    public static class EntityCreation
    {
        static public World World { get; private set; }
        static public void CreateNewWorld() => EntityCreation.World = new World("entity creation world");
    }

	public static class MotionUtility
	{

        static public (Entity motionEntity, NativeArray<Entity> steamEntities)
            CreateMotionEntities
            ( ExclusiveEntityTransaction em, EntityArchetype motionArchetype, EntityArchetype streamArche, MotionDataAccessor ma )
        {
            var motionEntity = em.CreateEntity( motionArchetype );

            var streamEntities = new NativeArray<Entity>
                ( ma.boneLength * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory );
            em.CreateEntity( streamArche, streamEntities );

            return (motionEntity, streamEntities);
        }

        static public void InitMotion
            ( ExclusiveEntityTransaction em, Entity motionEntity, int motionIndex, MotionDataAccessor ma )
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
            ( ExclusiveEntityTransaction em, NativeArray<Entity> streamEntities, int motionIndex, MotionDataAccessor ma )
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
