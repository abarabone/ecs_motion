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

        public EnMotionType Mode;
        public enum EnMotionType
        {
            typeA_progress_per_stream,
            typeB_progress_motion,
            typeB_direct,
        }


        public (Entity motionPrefab, NameAndEntity[] streamPrefabs) Convert
            ( EntityManager em, Entity drawPrefab )
        {
            var motionClip = this.MotionClip;

            var boneMasks = (Enumerable.Range( 1, motionClip.StreamPaths.Length ), motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => this.BoneMask.GetTransformActive( x.x ) )
                .ToArray();

            switch(this.Mode)
            {
                case EnMotionType.typeA_progress_per_stream:
                    return (Arche.MotionA, Arche.StreamA).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
                case EnMotionType.typeB_progress_motion:
                    return (Arche.MotionB, Arche.StreamB).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
                case EnMotionType.typeB_direct:
                    return (Arche.MotionBd, Arche.StreamBd).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
            }

            return (Entity.Null, null);
        }



        static class Arche
        {

            static public ComponentType[] MotionA = new ComponentType[]
            {
                typeof( MotionATag ),// 暫定、MotionB 特別するため
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( Prefab )
            };
            static public ComponentType[] StreamA = new ComponentType[]
            {
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( StreamInterpolatedData ),
                typeof( Prefab )
            };


            static public ComponentType[] MotionB = new ComponentType[]
            {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( MotionProgressTimerTag ),//
                typeof( Prefab ),
            };
            static public ComponentType[] StreamB = new ComponentType[]
            {
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamInterpolatedData ),
                typeof( StreamMotionLinkData ),//
                typeof( Prefab ),
            };

            static public ComponentType[] MotionBd = new ComponentType[]
            {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( Prefab ),
            };
            static public ComponentType[] StreamBd = new ComponentType[]
            {
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamInterpolatedData ),
                typeof( StreamMotionLinkData ),//
                typeof( Prefab ),
            };

        }
    }






    static public class MotionPrefabCreator
    {

        static public (Entity motionPrefab, NameAndEntity[] streamPrefabs) CreatePrefab
        (
            this (ComponentType[] motion, ComponentType[] stream) archetypes,
            EntityManager em, Entity drawPrefab, MotionClip motionClip, bool[] boneMasks
        )
        {

            var motionArchetype = archetypes.motion.GetOrCreate( em );
            var streamArchetype = archetypes.stream.GetOrCreate( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            var posStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype );
            var rotStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionClip, boneMasks, streamArchetype );
            
            em.SetComponentData( motionPrefab,
                new MotionStreamLinkData
                {
                    PositionStreamTop = posStreamPrefabs[ 0 ].Entity,
                    RotationStreamTop = rotStreamPrefabs[ 0 ].Entity,
                }
            );

            var streamPrefabs = (posStreamPrefabs, rotStreamPrefabs).Concat().ToArray();
            em.SetComponentData(
                streamPrefabs.Select( x => x.Entity ),
                Enumerable.Repeat(new StreamMotionLinkData { MotionEntity = motionPrefab }, streamPrefabs.Length)
            );
                
            return (motionPrefab, streamPrefabs);


            // モーションエンティティ生成
            Entity createMotionPrefab
                ( EntityManager em_, MotionClip motionClip_, EntityArchetype motionArchetype_ )
            {
                var motionBlobData = motionClip.ConvertToBlobData();

                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
                em_.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 0 } );

                return motionEntity;
            }

            // ストリームエンティティ生成
            NameAndEntity[] createStreamOfSectionPrefabs
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
                        NextStreamEntity = next,
                        BoneId = id,
                    };
                em_.SetComponentData( streamEntities, qNextLinker );

                var qDrawLinker =
                    from x in streamEntities
                    select new StreamDrawLinkData
                    {
                        DrawEntity = drawPrefab_,
                    };
                em_.SetComponentData( streamEntities, qDrawLinker );

                var qNames =
                    from i in qEnableId select System.IO.Path.GetFileName(motionClip.StreamPaths[i]);
                return (qNames, streamEntities).Zip( (name,ent) => new NameAndEntity(name,ent) ).ToArray();
            }
        }

    }
}

