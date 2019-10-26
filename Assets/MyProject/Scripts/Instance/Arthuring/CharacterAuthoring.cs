using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Instance;
using Abss.Common.Extension;

namespace Abss.Arthuring
{


    public class CharacterAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public interface IBoneConverter
        {
            (NativeArray<Entity> bonePrefabs, Entity posturePrefab) Convert( EntityManager em, NativeArray<Entity> streamPrefab, Entity drawPrefab );
        }


        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var (motionPrefab, streamPrefabs) = motionAuthor.Convert( em );

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawResources );

            var boneAuthor = this.GetComponent<IBoneConverter>();
            var (bonePrefabs, posturePrefab) = boneAuthor.Convert( em, streamPrefabs, drawPrefab );

            var colliderAuthor = this.GetComponent<ColliderAuthoring>();
            colliderAuthor.Convert( em, posturePrefab, bonePrefabs );

            var qChildren = Enumerable
                .Empty<Entity>()
                .Append( posturePrefab )
                .Append( drawPrefab )
                .Append( motionPrefab )
                .Concat( streamPrefabs )
                .Concat( bonePrefabs )
                ;

            var prefab = CharactorPrefabCreator.CreatePrefab( em, qChildren );

            em.SetComponentData( prefab,
                new CharacterLinkData
                {
                    PostureEntity = posturePrefab,
                    DrawEntity = drawPrefab,
                    MotionEntity = motionPrefab,
                }
            );

            streamPrefabs.Dispose();
            bonePrefabs.Dispose();


            this.gameObject.SetActive( false );

            return prefab;
        }

    }
    


    static public class CharactorPrefabCreator
    {
        
        static EntityArchetypeCache archetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( LinkedEntityGroup ),
                typeof( CharacterLinkData ),
                typeof( Prefab )
            )
        );


        static public Entity CreatePrefab( EntityManager em, IEnumerable<Entity> children )
        {
            var archetype = archetypeCache.GetOrCreateArchetype( em );

            var prefab = em.CreateEntity( archetype );

            em.SetLinkedEntityGroup( prefab, children );

            return prefab;
        }

    }


}

