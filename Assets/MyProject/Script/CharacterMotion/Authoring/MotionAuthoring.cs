﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Common.Extension;

namespace Abarabone.Authoring
{

    public enum EnMotionType
    {
        typeAProgressEveryStreams,
        typeBProgressMotion,
        typeBDirectPositioning,
    }
    public enum EnMotionBlendingType
    {
        blendChannel0,
        blendChannel1,
        overwrite = -1,
    }

    public class MotionAuthoring : MonoBehaviour
    {

        public MotionClip MotionClip;

        public AvatarMask BoneMask;

        public EnMotionType MotionType;

        public EnMotionBlendingType BlendingType;


        //public (Entity motionPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefab)
        public (Entity motionPrefab, StreamEntityUnit[] streamPrefabs, EnMotionBlendingType blendingMode)
            Convert( EntityManager em, Entity drawPrefab )
        {

            var enabledBoneIds = makeEnabledBoneIds_();

            var (motionPrefab, streamPrefabs) = convert( em, drawPrefab, this.MotionClip, enabledBoneIds );

            return (motionPrefab, streamPrefabs, this.BlendingType);


            int[] makeEnabledBoneIds_()
            {
                if( this.BoneMask == null )
                    return this.MotionClip.StreamPaths.Select( ( x, i ) => i ).ToArray();


                var enabledsAndPaths =
                    from id in Enumerable.Range( 0, this.BoneMask.transformCount )
                    select (enabled: this.BoneMask.GetTransformActive( id ), path: this.BoneMask.GetTransformPath( id ))
                    ;

                var qEnabledBoneId =
                    from s in
                        this.MotionClip.StreamPaths
                            .Select( ( x, i ) => (path: x, id: i) )
                    join m in
                        from m in enabledsAndPaths
                        where m.enabled
                        select m
                            on s.path equals m.path
                    select s.id
                    ;
                return qEnabledBoneId.ToArray();
            }

            (Entity motionPrefab, StreamEntityUnit[] streamPrefabs) convert
                (EntityManager em_, Entity drawPrefab_, MotionClip motionClip_, int[] enabledBoneIds_)
            {
                switch( this.MotionType )
                {
                    case EnMotionType.typeAProgressEveryStreams:
                        return (ArchetypeA.Motion, ArchetypeA.Stream)
                            .CreatePrefab( em_, drawPrefab_, motionClip_, enabledBoneIds_ );

                    case EnMotionType.typeBProgressMotion:
                        return (ArchetypeB.Motion, ArchetypeB.Stream)
                            .CreatePrefab( em_, drawPrefab_, motionClip_, enabledBoneIds_ );

                    case EnMotionType.typeBDirectPositioning:
                        return (ArchetypeBd.Motion, ArchetypeBd.Stream)
                            .CreatePrefab( em_, drawPrefab_, motionClip_, enabledBoneIds_ );
                }

                return (Entity.Null, null);
            }
        }



        static class ArchetypeA
        {
            static public ComponentType[] Motion = new ComponentType[]
            {
                //typeof( MotionATag ),// 暫定、MotionB 特別するため
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( Prefab )
            };
            static public ComponentType[] Stream = new ComponentType[]
            {
                typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamCursorData ),
                typeof( StreamInterpolatedData ),
                typeof( Prefab )
            };
        }

        static class ArchetypeB
        {
            static public ComponentType[] Motion = new ComponentType[]
            {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( MotionProgressTimerTag ),//
                typeof( Prefab ),
            };
            static public ComponentType[] Stream = new ComponentType[]
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
        static class ArchetypeBd
        {
            static public ComponentType[] Motion = new ComponentType[]
            {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( Prefab ),
            };
            static public ComponentType[] Stream = new ComponentType[]
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




    public struct StreamEntityUnit
    {
        public string Name;
        public Entity Position;
        public Entity Rotation;
        public Entity Scale;
    }


    static public class MotionPrefabCreator
    {

        //static public (Entity motionPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs)
        static public (Entity motionPrefab, StreamEntityUnit[] streamPrefabs)
            CreatePrefab
            (
                this (ComponentType[] motion, ComponentType[] stream) archetypes,
                EntityManager em, Entity drawPrefab, MotionClip motionClip, int[] enabledBoneIds
            )
        {
            var motionArchetype = archetypes.motion.GetOrCreate( em );
            var streamArchetype = archetypes.stream.GetOrCreate( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            var posStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionPrefab, enabledBoneIds, streamArchetype );
            var rotStreamPrefabs = createStreamOfSectionPrefabs( em, drawPrefab, motionPrefab, enabledBoneIds, streamArchetype );
            
            em.SetComponentData( motionPrefab,
                new MotionStreamLinkData
                {
                    PositionStreamTop = posStreamPrefabs[0],
                    RotationStreamTop = rotStreamPrefabs[0],
                }
            );

            var qStream = queryStreamEntityUnit( posStreamPrefabs, rotStreamPrefabs, motionClip.StreamPaths, enabledBoneIds );
            return (motionPrefab, qStream.ToArray());
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
        static NativeArray<Entity> createStreamOfSectionPrefabs(
            EntityManager em, Entity drawPrefab, Entity motionPrefab,
            int[] enabledBoneIds, EntityArchetype streamArchetype
        )
        {

            var streamEntities = new NativeArray<Entity>( enabledBoneIds.Length, Allocator.Temp );
            
            em.CreateEntity( streamArchetype, streamEntities );

            setStreamRelation_( streamEntities, enabledBoneIds );
            em.SetComponentData( streamEntities, new StreamDrawLinkData { DrawEntity = drawPrefab } );
            em.SetComponentData( streamEntities, new StreamMotionLinkData { MotionEntity = motionPrefab } );
            
            return streamEntities;
            

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
        }

        static IEnumerable<StreamEntityUnit> queryStreamEntityUnit(
            IEnumerable<Entity> posStreamPrefabs,
            IEnumerable<Entity> rotStreamPrefabs,
            string[] streamPaths, int[] enabledBoneIds
        )
        {
            return
                from x in (enabledBoneIds, posStreamPrefabs, rotStreamPrefabs).Zip()
                select new StreamEntityUnit
                {
                    Name = System.IO.Path.GetFileName( streamPaths[ x.x ] ),
                    Position = x.y,
                    Rotation = x.z,
                    Scale = Entity.Null,
                };
        }

    }
}

