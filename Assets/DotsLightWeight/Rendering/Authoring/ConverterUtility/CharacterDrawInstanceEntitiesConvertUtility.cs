using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Draw.Authoring
{
    using CharacterMotion;
    using Draw;
    using Character;
    using DotsLite.CharacterMotion.Authoring;
    using DotsLite.Model.Authoring;

    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using Unity.Linq;
    using DotsLite.Model;
    using DotsLite.EntityTrimmer.Authoring;

    static public class CharacterDrawInstanceEntitiesConvertUtility
    {

        static public void CreateDrawInstanceEntities(
            this GameObjectConversionSystem gcs,
            ModelGroupAuthoring.ModelAuthoringBase top, PostureAuthoring posture,
            Transform[] bones, EnBoneType boneMode)
        {

            var em = gcs.DstEntityManager;

            var drawInstanceEntity = createDrawInstanceEntity( gcs, top, posture, bones, boneMode );

            setBoneComponentValues( gcs, bones, drawInstanceEntity );

            setStreamComponentValues( gcs, bones, drawInstanceEntity );

            //// ほんとはここでやりたいけど、motion entity を取る方法がわからない → 辞書化すればよいかも
            //setMotionComponentValues(gcs, top, drawInstanceEntity);

            // いずれはスイッチで必要か否か選択できるようにしたい
            gcs.InsertTransformOffsetLink(posture, drawInstanceEntity, bones);

        }


        // ----------------------------------------------------------------------------------

        
        static Entity createDrawInstanceEntity(
            GameObjectConversionSystem gcs,
            ModelGroupAuthoring.ModelAuthoringBase top, PostureAuthoring posture,
            Transform[] bones, EnBoneType boneMode )
        {
            var em = gcs.DstEntityManager;

            var ent = Entity.Null;
            switch(boneMode)
            {
                case EnBoneType.reelup_chain:
                {
                    ent = gcs.CreateAdditionalEntity(top.gameObject, archetypeForReelupChain_());
                    setDrawComponents_();
                    setBoneTopLink_();
                }
                break;
                case EnBoneType.jobs_per_depth:
                {
                    ent = gcs.CreateAdditionalEntity(top.gameObject, archetypeForJobPerDepth_());
                    setDrawComponents_();
                }
                break;
            }

            em.SetName_(ent, $"{top.name} draw");
            return ent;


            EntityArchetype archetypeForReelupChain_() =>
                em.CreateArchetype(
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.BoneModelTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.PostureLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.BoneLinkData),
                    typeof(DrawInstance.TransformOffsetData),
                    typeof(Marker.Translation),
                    typeof(Marker.Rotation),
                    typeof(Bone.TransformTargetTag)
                );
            EntityArchetype archetypeForJobPerDepth_() =>
                em.CreateArchetype(
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.BoneModelTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.PostureLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.TransformOffsetData),
                    typeof(Marker.Translation),
                    typeof(Marker.Rotation)
                );

            void setDrawComponents_()
            {
                em.SetComponentData(ent,
                    new DrawInstance.ModelLinkData
                    {
                        //DrawModelEntityCurrent = gcs.GetPrimaryEntity(top.QueryModel.First().AsGameObject),
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(top.QueryModel.Single()),
                    }
                );
                em.SetComponentData(ent,
                    new DrawInstance.PostureLinkData
                    {
                        PostureEntity = gcs.GetPrimaryEntity(posture),
                    }
                );
                em.SetComponentData(ent,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );
                var tfmain = posture.transform;
                em.SetComponentData(ent,
                    new DrawInstance.TransformOffsetData
                    {
                        Position = tfmain.localPosition,
                        Rotation = tfmain.localRotation,
                    }
                );
            }

            void setBoneTopLink_()
            {
                em.SetComponentData(ent,
                    new DrawInstance.BoneLinkData
                    {
                        BoneRelationTop = gcs.GetPrimaryEntity(bones.First()),
                    }
                );
            }
        }


        static void setBoneComponentValues
            (
                GameObjectConversionSystem gcs,
                Transform[] bones, Entity drawInstanceEntity
            )
        {
            var em = gcs.DstEntityManager;

            var boneEntities = bones
                .Select( bone => gcs.GetPrimaryEntity( bone ) )
                .ToArray();

            addComponents_( em, boneEntities );
            setDrawComponet_( em, boneEntities, drawInstanceEntity );
            setBoneId_( em, boneEntities );

            return;


            void addComponents_( EntityManager em_, IEnumerable<Entity> boneEntities_ )
            {
                var addtypes = new ComponentTypes
                (
                    typeof( BoneDraw.LinkData ),
                    typeof( BoneDraw.IndexData ),
                    typeof( BoneDraw.TargetWorkData )
                );

                em.AddComponents( boneEntities_, addtypes );
            }


            void setDrawComponet_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstanceEntity_ )
            {
                var drawModelLinker = em_.GetComponentData<DrawInstance.ModelLinkData>( drawInstanceEntity_ );

                em_.SetComponentData(
                    boneEntities_,
                    new BoneDraw.LinkData
                    {
                        DrawInstanceEntity = drawInstanceEntity_,
                        DrawModelEntityCurrent = drawModelLinker.DrawModelEntityCurrent,
                    }
                );
            }

            void setBoneId_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_ )
            {
                var boneLength = boneEntities_.Count();

                em_.SetComponentData( boneEntities_,
                    from i in Enumerable.Range( 0, boneLength )
                    select new BoneDraw.IndexData { BoneLength = boneLength, BoneId = i }
                );
            }
        }


        // ほんとはここでやりたいけど、motion entity を取る方法がわからない
        //static void setMotionComponentValues
        //    (
        //        GameObjectConversionSystem gcs,
        //        GameObject top, Entity drawInstanceEntity
        //    )
        //{

        //    var em = gcs.DstEntityManager;

        //    var qMotionEntity =
        //        from ma in top.GetComponentsInChildren<MotionAuthoring>().Do(x => Debug.Log($"dm {x.name}"))
        //        from ent in gcs.GetEntities(top).Do(x=>Debug.Log($"dm {em.GetName_(x)}"))
        //        where em.HasComponent<Motion.DrawCullingData>(ent)
        //        select ent
        //        ;

        //    em.SetComponentData( qMotionEntity,
        //        new Motion.DrawCullingData
        //        {
        //            DrawInstanceEntity = drawInstanceEntity,
        //        }
        //    );
            
        //}


        static void setStreamComponentValues
            (
                GameObjectConversionSystem gcs,
                Transform[] bones, Entity drawInstanceEntity
            )
        {

            var em = gcs.DstEntityManager;

            var qStreamEntity =
                from bone in bones
                from ent in gcs.GetEntities( bone )
                where em.HasComponent<Stream.RelationData>(ent)
                select ent
                ;

            em.AddComponentData( qStreamEntity, new Stream.DrawLinkData { DrawEntity = drawInstanceEntity } );

        }


    }

}
