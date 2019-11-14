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
            (NameAndEntity[] bonePrefabs, Entity posturePrefab) Convert
                ( EntityManager em, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs, Entity drawPrefab );
        }


        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawResources );

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var (motionPrefab, posStreamPrefabs, rotStreamPrefabs) = motionAuthor.Convert( em, drawPrefab );
            var motionAuthor1 = this.GetComponent<MotionAuthoring>();//
            var (motionPrefab1, posStreamPrefabs1, rotStreamPrefabs1) = motionAuthor.Convert( em, drawPrefab );//

            var boneAuthor = this.GetComponent<IBoneConverter>();
            var (bonePrefabs, posturePrefab) = boneAuthor.Convert( em, streamPrefabs, drawPrefab );
            foreach( var z in
                from x in bonePrefabs
                join y in streamPrefabs1
                    on x.Name equals y.Name
                group y.Entity by x.Entity
            )
            {
                var linker = em.GetComponentData<BoneStreamLinkBlend2Data>( z.Key );
                linker.PositionStream1Entity = z.ElementAt( 0 );
                linker.RotationStream1Entity = z.ElementAt( 1 );
                linker.weight0 = 0.5f;
                em.SetComponentData( z.Key, linker );
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
                .Concat( bonePrefabs.Select(x=>x.Entity) )
                .Concat( jointPrefabs )
                ;

            var prefab = CharactorPrefabCreator.CreatePrefab( em, qChildren );

            em.SetComponentData( prefab,
                new CharacterLinkData
                {
                    PostureEntity = posturePrefab,
                    DrawEntity = drawPrefab,
                    MotionEntity = motionPrefab,
                    Motion2Entity = motionPrefab1,//
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

