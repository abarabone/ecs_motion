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
using Abss.Character;
using Abss.Common.Extension;

namespace Abss.Arthuring
{


    public class CharacterAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public interface IBoneConverter
        {
            (NativeArray<Entity> bonePrefabs, Entity posturePrefab) Convert
                ( EntityManager em, IEnumerable<NameAndEntity> streamPrefab, Entity drawPrefab );
        }


        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawResources );

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var (motionPrefab, streamPrefabs) = motionAuthor.Convert( em, drawPrefab );
            var motionAuthor1 = this.GetComponent<MotionAuthoring>();//
            var (motionPrefab1, streamPrefabs1) = motionAuthor.Convert( em, drawPrefab );//

            var boneAuthor = this.GetComponent<IBoneConverter>();
            var (bonePrefabs, posturePrefab) = boneAuthor.Convert( em, streamPrefabs, drawPrefab );
            foreach( var bone in from x in bonePrefabs join y in streamPrefabs1 on x equals y.Name )
            {
                var linker = em.GetComponentData<BoneStreamLinkBlend2Data>( bone );

            }

            var colliderAuthor = this.GetComponent<ColliderAuthoring>();
            var jointPrefabs = colliderAuthor.Convert( em, posturePrefab, bonePrefabs );

            var qChildren = Enumerable
                .Empty<Entity>()
                .Append( posturePrefab )
                .Append( drawPrefab )
                .Append( motionPrefab )
                .Concat( streamPrefabs.Select(x=>x.Entity) )
                .Append( motionPrefab1 )//
                .Concat( streamPrefabs1.Select( x => x.Entity ) )//
                .Concat( bonePrefabs )
                .Concat( jointPrefabs )
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

            // 暫定
            {
                em.AddComponentData( posturePrefab, new GroundHitResultData { } );
            }


            if( bonePrefabs.IsCreated ) bonePrefabs.Dispose();
            if( jointPrefabs.IsCreated ) jointPrefabs.Dispose();


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
                typeof( MoveHandlingData ),
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

