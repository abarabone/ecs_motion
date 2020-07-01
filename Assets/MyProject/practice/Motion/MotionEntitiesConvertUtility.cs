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



    static public class ArchetypeA
    {
        static public ComponentType[] Motion = new ComponentType[]
        {
                //typeof( MotionATag ),// 暫定、MotionB 特別するため
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamCursorData ),
                typeof( StreamInterpolatedData ),
        };
    }

    static public class ArchetypeB
    {
        static public ComponentType[] Motion = new ComponentType[]
        {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData ),//
                typeof( MotionProgressTimerTag )//
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamInterpolatedData ),
                typeof( StreamMotionLinkData )//
        };
    }
    static public class ArchetypeBd
    {
        static public ComponentType[] Motion = new ComponentType[]
        {
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeData ),
                typeof( MotionCursorData )//
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( StreamDrawLinkData ),
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamInterpolatedData ),
                typeof( StreamMotionLinkData )//
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

        static public void ConvertMotionEntities
            (
                this GameObjectConversionSystem gcs, GameObject motionMain, Transform[] bones,
                ComponentType[] motionTypes, ComponentType[] streamTypes,
                MotionClip motionClip, AvatarMask streamMask
            )
        {

            var em = gcs.DstEntityManager;
            var enabledBoneObjects = getEnabledBoneObjects( bones, motionMain, streamMask );
            var enabledBoneIds = getEnabledBoneIds( bones, motionMain, streamMask );


            var motionEntity = createMotionEntity( gcs, motionMain, motionTypes );

            var posStreamEntities = createStreamEntitiesOfSection( gcs, enabledBoneObjects, streamTypes );
            var rotStreamEntities = createStreamEntitiesOfSection( gcs, enabledBoneObjects, streamTypes );


            initMotionEntity( em, motionEntity, posStreamEntities, rotStreamEntities, motionClip );

            initStreamEntities( gcs, motionEntity, posStreamEntities, enabledBoneIds );
            initStreamEntities( gcs, motionEntity, rotStreamEntities, enabledBoneIds );


            setBoneStreamLinks( gcs, posStreamEntities, rotStreamEntities, enabledBoneObjects );
        }


        static GameObject[] getEnabledBoneObjects( Transform[] bones, GameObject root, AvatarMask streamMask )
        {
            var enabledBoneHashSet = streamMask.ToEnabledBoneHashSet();
            Debug.Log(root.MakePath());

            return bones
                .Select( bone => bone.gameObject )
                .Do(x=>Debug.Log(x.MakePath()))
                .Where( go => enabledBoneHashSet.Contains( go.MakePath( root ) ) )
                .ToArray();
        }
        static int[] getEnabledBoneIds( Transform[] bones, GameObject root, AvatarMask streamMask )
        {
            var enabledBoneHashSet = streamMask.ToEnabledBoneHashSet();

            return bones
                .Select( ( bone, i ) => (go: bone.gameObject, i) )
                .Where( x => enabledBoneHashSet.Contains( x.go.MakePath( root ) ) )
                .Select( x => x.i )
                .ToArray();
        }


        // モーションエンティティ生成
        // メインオブジェクトの関連エンティティとして生成し、リンクもここでつける。
        static Entity createMotionEntity
            ( GameObjectConversionSystem gcs, GameObject motionMain, ComponentType[] motionTypes )
        {
            var em = gcs.DstEntityManager;

            var motionArchetype = em.CreateArchetype( motionTypes );
            var motionEntity = gcs.CreateAdditionalEntity( motionMain, motionArchetype );

            em.SetName( motionEntity, $"{motionMain.name} motion" );

            return motionEntity;
        }

        static void initMotionEntity
            ( EntityManager em, Entity motionEntity, Entity[] poss, Entity[] rots, MotionClip motionClip )
        {

            var motionBlobData = motionClip.ConvertToBlobData();
            em.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );

            em.SetComponentData( motionEntity,
                new MotionInitializeData
                {
                    MotionIndex = 5,
                    DelayTime = 0.0f,
                    IsContinuous = true,
                    IsLooping = true,
                    TimeScale = 1.0f,
                }
            );

            em.SetComponentData( motionEntity,
                new MotionStreamLinkData
                {
                    PositionStreamTop = poss.First(),
                    RotationStreamTop = rots.First(),
                }
            );
        }

        static void setLinkForObject
            ( GameObjectConversionSystem gcs, GameObject top, Entity motionEntity )
        {

        }




        // ストリームエンティティ生成
        // ボーンオブジェクトの関連エンティティとして生成する。
        // 
        static Entity[] createStreamEntitiesOfSection
            (
                GameObjectConversionSystem gcs, GameObject[] enabledBoneObjects,
                ComponentType[] streamTypes
            )
        {
            var em = gcs.DstEntityManager;

            var streamArchetype = em.CreateArchetype( streamTypes );
            var streamEntities = enabledBoneObjects
                .Select( boneObject => gcs.CreateAdditionalEntity( boneObject, streamArchetype ) )
                .Do( ent => em.SetName(ent, $"{em.GetName(ent)} stream") )
                .ToArray();

            return streamEntities;
        }


        static void initStreamEntities
            (
                GameObjectConversionSystem gcs,
                Entity motionEntity, Entity[] streamEntities, int[] enabledBoneIds
            )
        {

            var em = gcs.DstEntityManager;

            em.SetComponentData( streamEntities, new StreamMotionLinkData { MotionEntity = motionEntity } );

            var qRelation = queryStreamRelation_( gcs, streamEntities, enabledBoneIds );
            em.SetComponentData( streamEntities, qRelation );

            return;


            IEnumerable<StreamRelationData> queryStreamRelation_
                ( GameObjectConversionSystem gcs_, Entity[] streamEntities_, int[] enabledBoneIds_ )
            {
                var em_ = gcs_.DstEntityManager;

                var qNext = streamEntities_
                    .Skip( 1 )
                    .Append( Entity.Null );

                return
                    from x in (qNext, enabledBoneIds_).Zip()
                    let next = x.x
                    let boneid = x.y
                    select new StreamRelationData
                    {
                        NextStreamEntity = next,
                        BoneId = boneid,
                    };
            }
        }



        static void setBoneStreamLinks
            ( GameObjectConversionSystem gcs, Entity[] poss, Entity[] rots, GameObject[] enabledBoneObjects )
        {
            var em = gcs.DstEntityManager;

            var qStreamLink =
                from x in (poss, rots).Zip()
                let pos = x.x
                let rot = x.y
                select new BoneStream0LinkData
                {
                    PositionStreamEntity = pos,
                    RotationStreamEntity = rot,
                };

            var qBoneEntity =
                from bone in enabledBoneObjects
                select gcs.GetPrimaryEntity( bone )
                ;

            em.AddComponentData( qBoneEntity, qStreamLink );
        }
    }
}

