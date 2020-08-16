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
    using Abarabone.Authoring;
    using Abarabone.Model.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Unity.Linq;
    using Abarabone.Model;

    static public class CharacterDrawInstanceEntitiesConvertUtility
    {

        static public void CreateDrawInstanceEntities
            (
                this GameObjectConversionSystem gcs, GameObject top, GameObject main,
                Transform[] bones, BoneType boneType, int instanceDataVectorLength = 0
            )
        {

            var em = gcs.DstEntityManager;

            var drawInstanceEntity = createDrawInstanceEntity( gcs, top, main );

            setBoneComponentValues( gcs, bones, drawInstanceEntity, boneType, instanceDataVectorLength );

            setStreamComponentValues( gcs, bones, drawInstanceEntity );

        }


        // ----------------------------------------------------------------------------------

        
        static Entity createDrawInstanceEntity
            ( GameObjectConversionSystem gcs, GameObject top, GameObject main )
        {
            var em = gcs.DstEntityManager;


            var archetype = em.CreateArchetype
            (
                typeof( DrawInstance.MeshTag ),
                typeof( DrawInstance.ModeLinkData ),
                typeof( DrawInstance.PostureLinkData ),
                typeof( DrawInstance.TargetWorkData )
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


            em.SetName_(ent, $"{top.name} draw" );
            return ent;
        }


        static void setBoneComponentValues
            (
                GameObjectConversionSystem gcs,
                Transform[] bones, Entity drawInstanceEntity, BoneType boneType, int instanceDataVectorLength
            )
        {
            var em = gcs.DstEntityManager;

            var boneEntities = bones
                .Select( bone => gcs.GetPrimaryEntity( bone ) )
                .ToArray();

            addComponents_( em, boneEntities );
            setDrawComponet_( em, boneEntities, drawInstanceEntity );
            setBoneId_( em, boneEntities, (int)boneType, instanceDataVectorLength );

            return;


            void addComponents_( EntityManager em_, IEnumerable<Entity> boneEntities_ )
            {
                var addtypes = new ComponentTypes
                (
                    typeof( DrawTransform.LinkData ),
                    typeof( DrawTransform.IndexData ),
                    typeof(DrawTransform.VectorBufferData)
                );

                em.AddComponents( boneEntities_, addtypes );
            }


            void setDrawComponet_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstanceEntity_ )
            {
                em_.SetComponentData(
                    boneEntities_,
                    new DrawTransform.LinkData
                    {
                        DrawInstanceEntity = drawInstanceEntity_,
                    }
                );
            }

            void setBoneId_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_, int vectorLengthInBone_, int instanceDataVectorLength_ )
            {
                var boneLength = boneEntities_.Count();

                em_.SetComponentData( boneEntities_,
                    from i in Enumerable.Range( 0, boneLength )
                    select new DrawTransform.IndexData
                    {
                        BoneId = i,
                        BoneLength = boneLength,
                        VectorBufferOffsetOfBone = vectorLengthInBone_ * i + instanceDataVectorLength_,
                    }
                );
            }
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
