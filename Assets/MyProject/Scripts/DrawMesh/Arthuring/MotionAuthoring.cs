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
            typeAProgressPerStream,
            typeBProgressMotion,
            typeBDirect,
        }


        public (Entity motionPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefab)
            Convert( EntityManager em, Entity drawPrefab )
        {
            var motionClip = this.MotionClip;

            var boneMasks = (Enumerable.Range( 1, motionClip.StreamPaths.Length ), motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => this.BoneMask.GetTransformActive( x.x ) )
                .ToArray();

            switch(this.Mode)
            {
                case EnMotionType.typeAProgressPerStream:
                    return (Arche.MotionA, Arche.StreamA).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
                case EnMotionType.typeBProgressMotion:
                    return (Arche.MotionB, Arche.StreamB).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
                case EnMotionType.typeBDirect:
                    return (Arche.MotionBd, Arche.StreamBd).CreatePrefab( em, drawPrefab, motionClip, boneMasks );
            }

            return (Entity.Null, null, null);
        }



        static class Arche
        {

            static public ComponentType[] MotionA = new ComponentType[]
            {
                //typeof( MotionATag ),// 暫定、MotionB 特別するため
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

        static public (Entity motionPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs)
            CreatePrefab
            (
                this (ComponentType[] motion, ComponentType[] stream) archetypes,
                EntityManager em, Entity drawPrefab, MotionClip motionClip, bool[] boneMasks
            )
        {
            var motionArchetype = archetypes.motion.GetOrCreate( em );
            var streamArchetype = archetypes.stream.GetOrCreate( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            var posStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionPrefab, motionClip, boneMasks, streamArchetype );
            var rotStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionPrefab, motionClip, boneMasks, streamArchetype );
            
            em.SetComponentData( motionPrefab,
                new MotionStreamLinkData
                {
                    PositionStreamTop = posStreamPrefabs[ 0 ].Entity,
                    RotationStreamTop = rotStreamPrefabs[ 0 ].Entity,
                }
            );
            
            return (motionPrefab, posStreamPrefabs, rotStreamPrefabs);
        }


        // モーションエンティティ生成
        static Entity createMotionPrefab
            ( EntityManager em, MotionClip motionClip, EntityArchetype motionArchetype )
        {
            var motionBlobData = motionClip.ConvertToBlobData();

            var motionEntity = em.CreateEntity( motionArchetype );
            em.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
            em.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 0 } );

            return motionEntity;
        }
        

        // ストリームエンティティ生成
        static NameAndEntity[] createStreamOfSectionPrefabs
        ( EntityManager em, Entity drawPrefab, Entity motionPrefab, MotionClip motionClip, bool[] boneMasks, EntityArchetype streamArchetype )
        {

            var enabledIds = extractEnableIds_( boneMasks );

            using( var streamEntities = new NativeArray<Entity>( enabledIds.Length, Allocator.Temp ) )
            {
                em.CreateEntity( streamArchetype, streamEntities );

                setStreamRelation_( streamEntities, enabledIds );
                setDrawLink_( streamEntities, drawPrefab );
                setMotionLink_( streamEntities, motionPrefab );

                return createNamesAndStreamPrefabs_( streamEntities, motionClip.StreamPaths, enabledIds );
            }


            int[] extractEnableIds_( bool[] boneMasks_ )
            {
                var qEnableId =
                    from x in boneMasks.Select( ( mask, i ) => (mask, i) )
                    where x.mask
                    select x.i
                    ;
                return qEnableId.ToArray();
            }

            void setStreamRelation_( NativeArray<Entity> streamEntities_, int[] enabledIds_ )
            {

                var qNext = streamEntities_
                    .Skip( 1 )
                    .Append( Entity.Null );

                var qNextLinker =
                    from x in (qNext, enabledIds_).Zip()
                    let next = x.x
                    let id = x.y
                    select new StreamRelationData
                    {
                        NextStreamEntity = next,
                        BoneId = id,
                    };
                em.SetComponentData( streamEntities_, qNextLinker );
            }

            void setDrawLink_( NativeArray<Entity> streamEntities_, Entity drawPrefab_ )
            {
                var qDrawLinker =
                    from x in streamEntities_
                    select new StreamDrawLinkData
                    {
                        DrawEntity = drawPrefab_,
                    };
                em.SetComponentData( streamEntities_, qDrawLinker );
            }

            void setMotionLink_( NativeArray<Entity> streamEntities_, Entity motionPrefab_ )
            {
                var qMotionLinker =
                    from x in streamEntities_
                    select new StreamMotionLinkData
                    {
                        MotionEntity = motionPrefab_,
                    };
                em.SetComponentData( streamEntities_, qMotionLinker );
            }

            NameAndEntity[] createNamesAndStreamPrefabs_
                ( NativeArray<Entity> streamEntities_, string[] streamPaths_, int[] enabledIds_ )
            {
                var qNames =
                    from i in enabledIds_
                    select System.IO.Path.GetFileName( streamPaths_[i] )
                    ;
                    
                return (qNames, streamEntities_)
                    .Zip( ( name, ent ) => new NameAndEntity( name, ent ) )
                    .ToArray();
            }
        }

    }
}

