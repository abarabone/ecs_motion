using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Object;

namespace Abss.Arthuring
{

    public class PrefabSettingsAuthoring : MonoBehaviour
    {

        public ConvertToMainCustomPrefabEntityBehaviour[] PrefabGameObjects;

        public Entity[] PrefabEntities { get; private set; }


        List<Entity> ents = new List<Entity>();

        void Awake()
        {
            var em = World.Active.EntityManager;
            

            var drawMeshCsSystem = em.World.GetExistingSystem<DrawMeshCsSystem>();
            var drawMeshCsResourceHolder = drawMeshCsSystem.GetResourceHolder();

            this.PrefabEntities = this.PrefabGameObjects
                .Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder ) )
                .ToArray();


            foreach( var model in Enumerable.Range( 0, this.PrefabEntities.Length ) )
            {
                var mlinker = em.GetComponentData<CharacterLinkData>( this.PrefabEntities[model] );
                ref var mclip = ref em.GetComponentData<MotionClipData>( mlinker.MotionEntity ).ClipData.Value;

                foreach( var i in Enumerable.Range( 0, 5000 ) )
                {
                    this.ents.Add( em.Instantiate( this.PrefabEntities[ model ] ) );

                    var chlinker = em.GetComponentData<CharacterLinkData>( this.ents[this.ents.Count-1] );
                    em.SetComponentData( chlinker.PostureEntity, new Translation { Value = new float3( i, 0, model ) } );
                    em.SetComponentData( chlinker.MotionEntity, new MotionInfoData { MotionIndex = i % mclip.Motions.Length } );
                }
            }
        }

        void Update()
        {
            if( !Input.GetMouseButtonDown( 0 ) ) return;
            if( this.ents.Count == 0 ) return;

            var em = World.Active.EntityManager;

            var ent = this.ents.Last();
            em.DestroyEntity( ent );
            this.ents.Remove( ent );
        }


        public abstract class ConvertToMainCustomPrefabEntityBehaviour : ConvertToCustomPrefabEntityBehaviour
        { }

        public abstract class ConvertToCustomPrefabEntityBehaviour : MonoBehaviour
        {
            abstract public Entity Convert( EntityManager em, DrawMeshResourceHolder drawres );
        }
    }


    /// <summary>
    /// アーキタイプを EntityManager ごとにキャッシュする。
    /// もしかすると、CreateArchetype() 自体にそういった仕組みがあるかもしれない、その場合は不要となる。
    /// </summary>
    public class EntityArchetypeCache
    {

        Dictionary<EntityManager, EntityArchetype> archetypeCache;

        Func<EntityManager, EntityArchetype> createFunc;


        public EntityArchetypeCache( Func<EntityManager, EntityArchetype> createFunc )
        {
            this.archetypeCache = new Dictionary<EntityManager, EntityArchetype>();
            this.createFunc = createFunc;
        }

        public EntityArchetype GetOrCreateArchetype( EntityManager em )
        {
            if( this.archetypeCache.TryGetValue( em, out var archetype ) ) return archetype;

            archetype = this.createFunc( em );

            archetypeCache.Add( em, archetype );

            return archetype;
        }
    }
}

