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
    using Motion;
    using Draw;
    using Character;
    using Abarabone.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;

    static public class CharacterDrawEntitiesConvertUtility
    {

        static public void CreateEntities
            ( this GameObjectConversionSystem gcs, EntityManager em, GameObject mainGameObject, IEnumerable<Transform> bones )
        {



        }


        static EntityArchetypeCache archetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( DrawInstanceModeLinkData ),
                typeof( DrawInstanceTargetWorkData )
            )
        );


        static public Entity CreatePrefab( EntityManager em, int boneLength, Entity modelEntity )
        {
            var archetype = archetypeCache.GetOrCreateArchetype( em );

            var ent = em.CreateEntity( archetype );

            em.SetComponentData( ent,
                new DrawInstanceModeLinkData
                {
                    DrawModelEntity = modelEntity,
                }
            );

            em.SetComponentData( ent,
                new DrawInstanceTargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            return ent;
        }

        static void setDrawComponet
            ( this EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstancePrefab )
        {
            var drawModelLinker = em_.GetComponentData<DrawInstanceModeLinkData>( drawInstancePrefab );

            em_.SetComponentData(
                boneEntities_,
                new DrawTransformLinkData
                {
                    DrawInstanceEntity = drawInstancePrefab,
                    DrawModelEntity = drawModelLinker.DrawModelEntity,
                }
            );
        }

        static void setBoneId
            ( this EntityManager em_, IEnumerable<Entity> boneEntities_ )
        {
            var boneLength = boneEntities_.Count();

            em_.SetComponentData( boneEntities_,
                from i in Enumerable.Range( 0, boneLength )
                select new DrawTransformIndexData { BoneLength = boneLength, BoneId = i }
            );
        }



    }

}
