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

    [DisallowMultipleComponent]
    public class CharacterAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {
        

        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawResources );

            var motionAuthors = this.GetComponents<MotionAuthoring>();
            var motionAndStreamPrefabs = motionAuthors.Select( x => x.Convert( em, drawPrefab ) ).ToArray();

            var boneAuthor = this.GetComponent<BoneAuthoring>();
            var (bonePrefabs, posturePrefab) = boneAuthor.Convert( em, motionAndStreamPrefabs.Select(x=>x.streamPrefabs), drawPrefab );
            
            var colliderAuthor = this.GetComponent<ColliderAuthoring>();
            var jointPrefabs = colliderAuthor.Convert( em, posturePrefab, bonePrefabs );

            var qStream =
                from x in motionAndStreamPrefabs
                from y in x.streamPrefabs
                select y
                ;
            var qChildren = Enumerable
                .Empty<Entity>()
                .Append( posturePrefab )
                .Append( drawPrefab )
                .Append( (from x in motionAndStreamPrefabs select x.motionPrefab).First() )
                .Concat( from x in qStream select x.Position )
                .Concat( from x in qStream select x.Rotation )
                .Concat( bonePrefabs.Select( x => x.Entity ) )
                .Concat( jointPrefabs )
                ;

            var prefab = CharactorPrefabCreator.CreatePrefab( em, qChildren );

            em.SetComponentData( prefab,
                new CharacterLinkData
                {
                    PostureEntity = posturePrefab,
                    DrawEntity = drawPrefab,
                    MotionEntity = motionAndStreamPrefabs.Select( x => x.motionPrefab ).First(),
                }
            );

            // 暫定
            {
                em.AddComponentData( posturePrefab, new GroundHitResultData { } );
            }


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

