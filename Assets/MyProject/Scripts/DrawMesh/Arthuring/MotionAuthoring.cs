using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public class MotionAuthoring : MonoBehaviour
    {

        public MotionClip MotionClip;

        public AvatarMask BoneMask;

        public bool IsMotionB;


        public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) Convert( EntityManager em, Entity drawPrefab )
        {
            var motionClip = this.MotionClip;

            var boneMasks = (Enumerable.Range( 1, motionClip.StreamPaths.Length ), motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => this.BoneMask.GetTransformActive( x.x ) )
                .ToArray();

            if( this.IsMotionB )
                return MotionBPrefabCreator.CreatePrefab( em, drawPrefab, motionClip, boneMasks );

            return MotionPrefabCreator.CreatePrefab( em, drawPrefab, motionClip, boneMasks );
        }
    }



    static public class MotionPrefabCreator
    {

        
        static EntityArchetypeCache motionArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( MotionATag ),// 暫定、MotionB 特別するため
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( Prefab )
            )
        );
        
        static EntityArchetypeCache streamArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( StreamInterpolatedData ),
                typeof( Prefab )
            )
        );


        static public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) CreatePrefab
            ( EntityManager em, Entity drawPrefab, MotionClip motionClip, bool[] boneMasks )
        {

            var motionArchetype = motionArchetypeCache.GetOrCreateArchetype( em );
            var streamArchetype = streamArchetypeCache.GetOrCreateArchetype( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            using( var posStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype ) )
            using( var rotStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype ) )
            {

                em.SetComponentData( motionPrefab,
                    new MotionStreamLinkData
                    {
                        PositionStreamTop = posStreamPrefabs[0],
                        RotationStreamTop = rotStreamPrefabs[0],
                    }
                );

                var streamPrefabs = (posStreamPrefabs, rotStreamPrefabs).Concat( Allocator.Temp );
                return (motionPrefab, streamPrefabs);
            }


            // モーションエンティティ生成
            Entity createMotionPrefab
                //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype motionArchetype_ )
                (EntityManager em_, MotionClip motionClip_, EntityArchetype motionArchetype_)
            {
                var motionBlobData = motionClip.ConvertToBlobData();

                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
                em_.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 0 } );

                return motionEntity;
            }

            // ストリームエンティティ生成
            NativeArray<Entity> createStreamOfSectionPrefabs
            //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype streamArchetype_ )
            ( EntityManager em_, Entity drawPrefab_, MotionClip motionClip_, bool[] boneMasks_, EntityArchetype streamArchetype_ )
            {
                var streamLength = motionClip.StreamPaths.Length;
                var enableLength = boneMasks_.Where( x => x ).Count();

                var streamEntities = new NativeArray<Entity>( enableLength, Allocator.Temp );
                em_.CreateEntity( streamArchetype_, streamEntities );

                var qNext = streamEntities.Skip( 1 ).Append( Entity.Null );
                var qEnableId = boneMasks_.Select( (x, i) => (x, i) ).Where( x => x.x ).Select( x => x.i );

                var qNextLinker = 
                    from x in (qNext, qEnableId).Zip()
                    let next = x.x
                    let id = x.y
                    select new StreamRelationData
                    {
                        NextStreamEntity = x.x,
                        BoneId = x.y,
                    };
                em_.SetComponentData( streamEntities, qNextLinker );

                em_.SetComponentData( streamEntities,
                    streamEntities.Select( _ => new StreamDrawLinkData { DrawEntity = drawPrefab_ } )
                );

                return streamEntities;
            }
        }

    }

    // コンポーネントデータを生成して返すだけにして（配列だとしても）、
    // エンティティにくっつけるのとは別にしたほうがいいかも




    static public class MotionBPrefabCreator
    {


        static EntityArchetypeCache motionArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( MotionProgressTimerTag ),//
                typeof( Prefab )
            )
        );

        static EntityArchetypeCache streamArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamInterpolatedData ),
                typeof( StreamMotionLinkData ),//
                typeof( Prefab )
            )
        );


        static public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) CreatePrefab
            ( EntityManager em, Entity drawPrefab, MotionClip motionClip, bool[] boneMasks )
        {

            var motionArchetype = motionArchetypeCache.GetOrCreateArchetype( em );
            var streamArchetype = streamArchetypeCache.GetOrCreateArchetype( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            using( var posStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype ) )
            using( var rotStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype ) )
            {

                em.SetComponentData( motionPrefab,
                    new MotionStreamLinkData
                    {
                        PositionStreamTop = posStreamPrefabs[ 0 ],
                        RotationStreamTop = rotStreamPrefabs[ 0 ],
                    }
                );

                var streamPrefabs = (posStreamPrefabs, rotStreamPrefabs).Concat( Allocator.Temp );
                em.SetComponentData( streamPrefabs,
                    Enumerable.Repeat(new StreamMotionLinkData { MotionEntity = motionPrefab }, streamPrefabs.Length) );
                
                return (motionPrefab, streamPrefabs);
            }


            // モーションエンティティ生成
            Entity createMotionPrefab
                //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype motionArchetype_ )
                ( EntityManager em_, MotionClip motionClip_, EntityArchetype motionArchetype_ )
            {
                var motionBlobData = motionClip.ConvertToBlobData();

                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
                em_.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 0 } );

                return motionEntity;
            }

            // ストリームエンティティ生成
            NativeArray<Entity> createStreamOfSectionPrefabs
            //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype streamArchetype_ )
            ( EntityManager em_, Entity drawPrefab_, MotionClip motionClip_, bool[] boneMasks_, EntityArchetype streamArchetype_ )
            {
                var streamLength = motionClip.StreamPaths.Length;
                var enableLength = boneMasks_.Where( x => x ).Count();

                var streamEntities = new NativeArray<Entity>( enableLength, Allocator.Temp );
                em_.CreateEntity( streamArchetype_, streamEntities );

                var qNext = streamEntities.Skip( 1 ).Append( Entity.Null );
                var qEnableId = boneMasks_.Select( ( x, i ) => (x, i) ).Where( x => x.x ).Select( x => x.i );

                var qNextLinker =
                    from x in (qNext, qEnableId).Zip()
                    let next = x.x
                    let id = x.y
                    select new StreamRelationData
                    {
                        NextStreamEntity = x.x,
                        BoneId = x.y,
                    };
                em_.SetComponentData( streamEntities, qNextLinker );

                em_.SetComponentData( streamEntities,
                    streamEntities.Select( _ => new StreamDrawLinkData { DrawEntity = drawPrefab_ } )
                );

                return streamEntities;
            }
        }

    }
}

