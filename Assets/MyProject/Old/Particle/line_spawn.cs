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
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Character;
using Abss.Common.Extension;
using Abss.Particle;

namespace Abss.Arthuring
{

    public class line_spawn : MonoBehaviour
    {

        public int InstanceCountPerModel = 100;

        List<Entity> ents = new List<Entity>();




        void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var drawSettings = this.GetComponent<DrawPrefabSettingsAuthoring>();


            foreach( var model in Enumerable.Range( 0, drawSettings.PrefabEntities.Length ) )
            {
                var prefab = drawSettings.PrefabEntities[ model ];

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    var ent = em.Instantiate( prefab );
                    this.ents.Add( ent );
                    createPointNodes_( ent, i, model );
                }
            }

            void createPointNodes_( Entity entity, int i, int j )
            {
                var ipos = 0;
                var tf = this.transform;
                for( var ent = getNext_( entity ); ent.Entity != Entity.Null; ent = getNext_( ent ) )
                {
                    this.nodes.Add( ent );
                    var go = new GameObject();
                    go.transform.parent = tf;
                    go.transform.position = tf.position + new Vector3( j * 30 + ipos++ * 1.5f, UnityEngine.Random.value * .3f, j + i * 2 + UnityEngine.Random.value * .3f );
                }

                LineParticleNodeEntity getNext_( LineParticleNodeEntity ent_ ) =>
                    em.GetComponentData<LineParticlePointNodeLinkData>( ent_.Entity ).NextNodeEntity;
            }

        }

        List<LineParticleNodeEntity> nodes = new List<LineParticleNodeEntity>();


        private void Update()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var tfs = this.GetComponentsInChildren<Transform>().Skip( 1 );

            foreach( var x in (this.nodes, tfs).Zip() )
            {
                em.SetComponentData( x.x.Entity, new Translation { Value = x.y.position } );
            }
        }

    }

}

