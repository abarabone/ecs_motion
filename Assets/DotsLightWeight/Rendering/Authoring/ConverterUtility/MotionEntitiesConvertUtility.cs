using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace DotsLite.CharacterMotion.Authoring
{

    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Misc;
    using DotsLite.CharacterMotion;
    using DotsLite.Draw;
    using DotsLite.Common.Extension;
    using DotsLite.Character;
    using DotsLite.Model;
    using Unity.Linq;
    using DotsLite.Model.Authoring;

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
                typeof( Motion.InfoData ),
                typeof( Motion.ClipData ),
                typeof( Motion.StreamLinkData ),
                typeof( Motion.InitializeData ),
                typeof( Motion.DrawCullingData )
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( Stream.DrawLinkData ),
                typeof( Stream.RelationData ),
                typeof( Stream.KeyShiftData ),
                typeof( Stream.NearKeysCacheData ),
                typeof( Stream.CursorData ),
                typeof( Stream.InterpolationData ),
                typeof( Stream.DrawTargetData )
        };
    }

    static public class ArchetypeB
    {
        static public ComponentType[] Motion = new ComponentType[]
        {
                typeof( Motion.InfoData ),
                typeof( Motion.ClipData ),
                typeof( Motion.StreamLinkData ),
                typeof( Motion.InitializeData ),
                typeof( Motion.CursorData ),//
                typeof( Motion.ProgressTimerTag ),//
                typeof( Motion.DrawCullingData )
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( Stream.DrawLinkData ),
                typeof( Stream.RelationData ),
                typeof( Stream.KeyShiftData ),
                typeof( Stream.NearKeysCacheData ),
                typeof( Stream.InterpolationData ),
                typeof( Stream.MotionLinkData ),//
                typeof( Stream.DrawTargetData )
        };
    }
    static public class ArchetypeBd
    {
        static public ComponentType[] Motion = new ComponentType[]
        {
                typeof( Motion.InfoData ),
                typeof( Motion.ClipData ),
                typeof( Motion.StreamLinkData ),
                typeof( Motion.InitializeData ),
                typeof( Motion.CursorData ),//
                typeof( Motion.DrawCullingData )
        };
        static public ComponentType[] Stream = new ComponentType[]
        {
                //typeof( Stream.DrawLinkData ),
                typeof( Stream.RelationData ),
                typeof( Stream.KeyShiftData ),
                typeof( Stream.NearKeysCacheData ),
                typeof( Stream.InterpolationData ),
                typeof( Stream.MotionLinkData ),//
                typeof( Stream.DrawTargetData )
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
                this GameObjectConversionSystem gcs,
                MotionAuthoring motionMain, PostureAuthoring posture, Transform[] bones,
                ComponentType[] motionTypes, ComponentType[] streamTypes,
                MotionClip motionClip, AvatarMask streamMask
            )
        {

            var em = gcs.DstEntityManager;
            var enabledBoneObjects = getEnabledBoneObjects( bones, posture.gameObject, streamMask );
            var enabledBoneIds = getEnabledBoneIds( bones, posture.gameObject, streamMask );


            var motionEntity = createMotionEntity( gcs, motionMain, motionTypes );

            var posStreamEntities = createStreamEntitiesOfSection( gcs, enabledBoneObjects, streamTypes );
            var rotStreamEntities = createStreamEntitiesOfSection( gcs, enabledBoneObjects, streamTypes );


            initMotionEntity( em, motionEntity, posStreamEntities, rotStreamEntities, motionClip );

            initStreamEntities( gcs, motionEntity, posStreamEntities, enabledBoneIds );
            initStreamEntities( gcs, motionEntity, rotStreamEntities, enabledBoneIds );


            setBoneStreamLinks( gcs, posStreamEntities, rotStreamEntities, enabledBoneObjects );

            //setLinkForObjectEntity(gcs, motionMain, motionEntity);

            setDrawLink(gcs, motionEntity, motionMain);
        }


        static GameObject[] getEnabledBoneObjects( Transform[] bones, GameObject root, AvatarMask streamMask )
        {
            var enabledBoneHashSet = streamMask.ToEnabledBoneHashSet();
            //Debug.Log(root.MakePath());

            return bones
                .Select( bone => bone.gameObject )
                //.Do(x=>Debug.Log(x.MakePath()))
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
            ( GameObjectConversionSystem gcs, MotionAuthoring motionMain, ComponentType[] motionTypes )
        {
            var em = gcs.DstEntityManager;

            var motionArchetype = em.CreateArchetype( motionTypes );
            //var motionEntity = gcs.CreateAdditionalEntity( motionMain.gameObject, motionArchetype );
            var motionEntity = gcs.GetOrCreateEntity(motionMain, new ComponentTypes(motionTypes));// motionArchetype);

            em.SetName_( motionEntity, $"{motionMain.name} motion" );

            gcs.GetEntityDictionary()[motionMain] = motionEntity;
            return motionEntity;
        }

        static void initMotionEntity
            ( EntityManager em, Entity motionEntity, Entity[] poss, Entity[] rots, MotionClip motionClip )
        {

            var motionBlobData = motionClip.ConvertToBlobData();
            em.SetComponentData( motionEntity, new Motion.ClipData { MotionClipData = motionBlobData } );

            em.SetComponentData( motionEntity,
                new Motion.InitializeData
                {
                    MotionIndex = 5,
                    DelayTime = 0.0f,
                    IsContinuous = true,
                    IsLooping = true,
                    TimeScale = 1.0f,
                }
            );

            em.SetComponentData( motionEntity,
                new Motion.StreamLinkData
                {
                    PositionStreamTop = poss.First(),
                    RotationStreamTop = rots.First(),
                }
            );
        }

        //static void setLinkForObjectEntity
        //    (GameObjectConversionSystem gcs, GameObject main, Entity motionEntity)
        //{
        //    var em = gcs.DstEntityManager;
        //    var mainEntity = gcs.GetPrimaryEntity(main);

        //    var link = em.GetComponentData<ObjectMainCharacterLinkData>(mainEntity);
        //    link.MotionEntity = motionEntity;
        //    em.SetComponentData(mainEntity, link);
        //}




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
                .Do( ent => em.SetName_(ent, $"{em.GetName_(ent)} stream") )
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

            em.SetComponentData( streamEntities, new Stream.MotionLinkData { MotionEntity = motionEntity } );

            var qRelation = queryStreamRelation_( gcs, streamEntities, enabledBoneIds );
            em.SetComponentData( streamEntities, qRelation );

            return;


            IEnumerable<Stream.RelationData> queryStreamRelation_
                ( GameObjectConversionSystem gcs_, Entity[] streamEntities_, int[] enabledBoneIds_ )
            {
                var em_ = gcs_.DstEntityManager;

                var qNext = streamEntities_
                    .Skip( 1 )
                    .Append( Entity.Null );

                return
                    from x in (qNext, enabledBoneIds_).Zip()
                    let next = x.src0
                    let boneid = x.src1
                    select new Stream.RelationData
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
                let pos = x.src0
                let rot = x.src1
                select new Bone.Stream0LinkData
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



        // ほんとうは draw instance でやりたい
        //static void setMotionComponentValues
        static void setDrawLink
            (
                GameObjectConversionSystem gcs, Entity motionEntity,
                MotionAuthoring motionMain
            )
        {

            var em = gcs.DstEntityManager;

            var top = motionMain.gameObject
                .AncestorsAndSelf().Where(go => go.GetComponent<ModelGroupAuthoring.ModelAuthoringBase>())
                .First();
                
            var qDrawEntity =
                from ent in gcs.GetEntities(top)
                where em.HasComponent<DrawInstance.TargetWorkData>(ent)
                select ent
                ;

            em.SetComponentData( motionEntity,
                new Motion.DrawCullingData
                {
                    DrawInstanceEntity = qDrawEntity.First(),
                }
            );

        }

    }
}

