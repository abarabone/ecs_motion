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

    public struct ChCreateMessage
    {

    }



    [UpdateInGroup( typeof( MotionGroup ) )]
    //[UpdateAfter( typeof(MotionProgressSystem) )]
    [AlwaysUpdateSystem]
    public class ChCreationSystem : JobComponentSystem
    {

        //MotionDataInNative md;

        //EntityArchetype motionArchetype;
        //EntityArchetype streamArchetype;

        static public MotionDataInNative md;

        public NativeQueue<ChCreateMessage> CreateMessageQueue;


        protected override void OnCreate()
        {
            EntityCreation.CreateNewWorld();
            EntityCreation.CreateArchetypes();
            EntityCreation.dummyEntityCreationAndDestory( this.EntityManager );

            this.CreateMessageQueue = new NativeQueue<ChCreateMessage>( Allocator.Persistent );
        }

        protected override void OnDestroy()
        {
            //EntityCreation.World.Dispose();
            ChCreationSystem.md.Dispose();
            this.CreateMessageQueue.Dispose();
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        //protected override void OnUpdate()
        {
            var isLeftClick = Input.GetMouseButtonDown(1);
            if( !isLeftClick ) return inputDeps;

            //Debug.Log("click");
            var motionIndex = 0;
            var ma = ChCreationSystem.md.CreateAccessor( motionIndex );
            

            var cem = EntityCreation.World.EntityManager;
            var eet = cem.BeginExclusiveEntityTransaction();

            cem.ExclusiveEntityTransactionDependency = new ChCreateJob
            {
                eet = eet,
                motionArchetype = EntityCreation.motionArchetype,
                streamArchetype = EntityCreation.streamArchetype,
                motionIndex = motionIndex,
                ma = ma
            }
            .Schedule( cem.ExclusiveEntityTransactionDependency );

            cem.EndExclusiveEntityTransaction();
            this.EntityManager.MoveEntitiesFrom( cem );


            return JobHandle.CombineDependencies(inputDeps,cem.ExclusiveEntityTransactionDependency);
        }
    }

    //[BurstCompile]
    public struct ChCreateJob : IJob
    {
        public ExclusiveEntityTransaction eet;
        public EntityArchetype motionArchetype;
        public EntityArchetype streamArchetype;
        public int motionIndex;
        public MotionDataAccessor ma;

        public void Execute()
        {
            MotionUtility.CreateMotionEntities
                ( eet, motionArchetype, streamArchetype, streamLength:ma.boneLength * 2, out var ment, out var sents );

            MotionUtility.InitMotion( eet, ment, motionIndex, ma );
            MotionUtility.InitMotionStream( eet, sents, motionIndex, ma );

            sents.Dispose();
        }
    }



    public static class EntityCreation
    {
        static public World World { get; private set; }
        static public World CreateNewWorld() => EntityCreation.World = new World("entity creation world");

        static public EntityArchetype motionArchetype { get; private set; }
        static public EntityArchetype streamArchetype { get; private set; }
        static public void CreateArchetypes()
        {
            EntityCreation.motionArchetype = EntityCreation.World.EntityManager.CreateArchetype
            (
                typeof(MotionInfoData)
            );
            EntityCreation.streamArchetype = EntityCreation.World.EntityManager.CreateArchetype
            (
                //typeof(StreamKeyShiftData), 
                typeof(StreamTimeProgressData)
                //typeof(StreamNearKeysCacheData)
            );
        }
        
        static public void dummyEntityCreationAndDestory( EntityManager em_main )
        {
            var em_creation = EntityCreation.World.EntityManager;

            em_creation.CreateEntity( EntityCreation.motionArchetype );
            em_creation.CreateEntity( EntityCreation.streamArchetype );

            em_main.MoveEntitiesFrom( out var ents, em_creation );
            em_main.DestroyEntity( ents );
            ents.Dispose();
        }
    }

	public static class MotionUtility
	{

        static public void CreateMotionEntities
            (
                ExclusiveEntityTransaction em, EntityArchetype motionArchetype, EntityArchetype streamArche,
                int streamLength,
                out Entity motionEntity, out NativeArray<Entity> streamEntities
            )
        {
            motionEntity = em.CreateEntity( motionArchetype );

            streamEntities = new NativeArray<Entity>
                ( streamLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory );

            em.CreateEntity( streamArche, streamEntities );
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

                em.SetComponentData( streamEntities[ i ], timer );
                //em.SetComponentData( streamEntities[ i ], shifter );
                //em.SetComponentData( streamEntities[ i ], cache );
            }
        }

    }
}
