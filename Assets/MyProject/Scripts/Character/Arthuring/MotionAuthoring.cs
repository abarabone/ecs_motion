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


        //public (Entity motionPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefab)
        public (Entity motionPrefab, StreamEntityUnit[] streamPrefabs) Convert
            ( EntityManager em, Entity drawPrefab )
        {

            var motionClip = this.MotionClip;
            var enabledBoneIds = makeEnabledBoneIds_();

            switch(this.Mode)
            {
                case EnMotionType.typeAProgressPerStream:
                    return (ArchetypeA.Motion, ArchetypeA.Stream)
                        .CreatePrefab( em, drawPrefab, motionClip, enabledBoneIds );

                case EnMotionType.typeBProgressMotion:
                    return (ArchetypeB.Motion, ArchetypeB.Stream)
                        .CreatePrefab( em, drawPrefab, motionClip, enabledBoneIds );

                case EnMotionType.typeBDirect:
                    return (ArchetypeBd.Motion, ArchetypeBd.Stream)
                        .CreatePrefab( em, drawPrefab, motionClip, enabledBoneIds );
            }

            return (Entity.Null, null);


            int[] makeEnabledBoneIds_()
            {
                if( this.BoneMask == null )
                    return motionClip.StreamPaths.Select( ( x, i ) => i ).ToArray();


                var enabledsAndPaths =
                    from id in Enumerable.Range( 0, this.BoneMask.transformCount )
                    select (enabled: this.BoneMask.GetTransformActive( id ), path: this.BoneMask.GetTransformPath( id ))
                    ;

                var qEnabledBoneId =
                    from s in
                        motionClip.StreamPaths
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
                typeof( motioncur ),
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
            IEnumerable<Entity> posStreamPrefabs, IEnumerable<Entity> rotStreamPrefabs,
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

