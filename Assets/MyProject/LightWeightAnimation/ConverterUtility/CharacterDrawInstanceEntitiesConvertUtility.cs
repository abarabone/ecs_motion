using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Draw.Authoring
{
    using CharacterMotion;
    using Draw;
    using Character;
    using Abarabone.CharacterMotion.Authoring;
    using Abarabone.Model.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Unity.Linq;

    static public class CharacterDrawInstanceEntitiesConvertUtility
    {

        static public void CreateDrawInstanceEntities
            ( this GameObjectConversionSystem gcs, GameObject top, GameObject main, Transform[] bones )
        {

            var em = gcs.DstEntityManager;

            var drawInstanceEntity = createDrawInstanceEntity( gcs, top, main, bones );

            setBoneComponentValues( gcs, bones, drawInstanceEntity );

            setStreamComponentValues( gcs, bones, drawInstanceEntity );

            setMotionComponentValues(gcs, top, drawInstanceEntity);

        }


        // ----------------------------------------------------------------------------------

        
        static Entity createDrawInstanceEntity
            ( GameObjectConversionSystem gcs, GameObject top, GameObject main, Transform[] bones )
        {
            var em = gcs.DstEntityManager;


            var archetype = em.CreateArchetype
            (
                typeof( DrawInstance.MeshTag ),
                typeof( DrawInstance.ModeLinkData ),
                typeof( DrawInstance.PostureLinkData ),
                typeof( DrawInstance.TargetWorkData ),
                typeof( DrawInstance.BoneLinkData )
            );
            var ent = gcs.CreateAdditionalEntity( top, archetype );
            
            em.SetComponentData( ent,
                new DrawInstance.ModeLinkData
                {
                    DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary( top ),
                }
            );
            em.SetComponentData(ent,
                new DrawInstance.PostureLinkData
                {
                    PostureEntity = gcs.GetPrimaryEntity(main),
                }
            );
            em.SetComponentData( ent,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );
            em.SetComponentData(ent,
                new DrawInstance.BoneLinkData
                {
                    BoneRelationTop = gcs.GetPrimaryEntity(bones.First()),
                }
            );


            em.SetName_(ent, $"{top.name} draw" );
            return ent;
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
                    typeof( DrawTransform.LinkData ),
                    typeof( DrawTransform.IndexData ),
                    typeof( DrawTransform.TargetWorkData )
                );

                em.AddComponents( boneEntities_, addtypes );
            }


            void setDrawComponet_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstanceEntity_ )
            {
                var drawModelLinker = em_.GetComponentData<DrawInstance.ModeLinkData>( drawInstanceEntity_ );

                em_.SetComponentData(
                    boneEntities_,
                    new DrawTransform.LinkData
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
                    select new DrawTransform.IndexData { BoneLength = boneLength, BoneId = i }
                );
            }
        }


        static void setMotionComponentValues
            (
                GameObjectConversionSystem gcs,
                GameObject top, Entity drawInstanceEntity
            )
        {

            var em = gcs.DstEntityManager;

            var qMotionEntity =
                from ma in top.GetComponentsInChildren<MotionAuthoring>().Do(x => Debug.Log($"dm {x.name}"))
                from ent in gcs.GetEntities(top).Do(x=>Debug.Log($"dm {em.GetName_(x)}"))
                where em.HasComponent<Motion.DrawCullingData>(ent)
                select ent
                ;

            em.SetComponentData( qMotionEntity,
                new Motion.DrawCullingData
                {
                    DrawInstanceEntity = drawInstanceEntity,
                }
            );
            
        }


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
