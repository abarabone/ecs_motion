using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public class CharactorAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {




        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var (motionPrefab, streamPrefabs) = motionAuthor.Convert( em );

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawResources );

            var boneAuthor = this.GetComponent<BoneAuthoring>();
            var (bonePrefabs, posturePrefab) = boneAuthor.Convert( em, motionPrefab, streamPrefabs, drawPrefab );


            var qChildren = Enumerable
                .Empty<Entity>()
                .Append( motionPrefab )
                .Concat( streamPrefabs )
                .Concat( bonePrefabs )
                .Append( posturePrefab )
                .Append( drawPrefab )
                ;

            var prefab = CharactorPrefabCreator.CreatePrefab( em, qChildren );

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

