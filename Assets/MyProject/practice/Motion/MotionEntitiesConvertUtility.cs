using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Motion.Authoring
{

    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Misc;
    using Abarabone.Motion;
    using Abarabone.Draw;
    using Abarabone.Common.Extension;


    public enum EnMotionType
    {
        typeAProgressEveryStreams,
        typeBProgressMotion,
        typeBDirectPositioning,
    }
    //public enum EnMotionBlendingType
    //{
    //    blendChannel0,
    //    blendChannel1,
    //    overwrite = -1,
    //}
    


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




    //public struct StreamEntityUnit
    //{
    //    public string Name;
    //    public Entity Position;
    //    public Entity Rotation;
    //    public Entity Scale;
    //}


    static public class MotionEntitiesConvertUntility
    {
        
        static public void CreatePrefab
            (
                GameObjectConversionSystem gcs, GameObject motionMain, Transform[] bones,
                ComponentType[] motionTypes, ComponentType[] streamTypes,
                MotionClip motionClip, AvatarMask streamMask
            )
        {

            var motionPrefab = initMotionEntity( em, motionClip, motionArchetype );
            var posStreamPrefabs = createStreamEntitiesOfSection( em, drawPrefab, motionPrefab, enabledBoneIds, streamArchetype );
            var rotStreamPrefabs = createStreamEntitiesOfSection( em, drawPrefab, motionPrefab, enabledBoneIds, streamArchetype );

            initMotionEntity( em, motionPrefab, posStreamPrefabs, rotStreamPrefabs );
        }


        static Entity[] getBoneEntities( GameObjectConversionSystem gcs, Transform[] bones, AvatarMask streamMask )
        {
            var enableBoneHashSet = streamMask.ToEnabledBoneHashSet();

            bones.
        }


        // モーションエンティティ生成
        static void initMotionEntity
            ( GameObjectConversionSystem gcs, GameObject motionMain, MotionClip motionClip, ComponentType[] motionTypes )
        {
            var em = gcs.DstEntityManager;

            var motionArchetype = em.CreateArchetype( motionTypes );
            var motionEntity = gcs.GetPrimaryEntity( motionMain );
            em.SetArchetype( motionEntity, motionArchetype );

            var motionBlobData = motionClip.ConvertToBlobData();
            em.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
            em.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 0 } );
        }


        // ストリームエンティティ生成
        static Entity[] createStreamEntitiesOfSection
            ( GameObjectConversionSystem gcs, GameObject motionMainain, Transform[] bones, ComponentType[] streamTypes )
        {
            var em = gcs.DstEntityManager;

            var streamArchetype = em.CreateArchetype( streamTypes );
            var streamEntities = bones
                .Select( bone => gcs.CreateAdditionalEntity( em, bone.gameObject, streamArchetype ) )
                .ToArray();

            return streamEntities;
        }

        static void initStreamEntities( GameObjectConversionSystem gcs, Entity[] streamEntities )
        {

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

        static void initMotionEntity( EntityManager em, Entity motion, Entity[] poss, Entity[] rots )
        {
            em.SetComponentData( motion,
                new MotionStreamLinkData
                {
                    PositionStreamTop = poss.First(),
                    RotationStreamTop = rots.First(),
                }
            );
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

