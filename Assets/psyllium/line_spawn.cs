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
                var ent = drawSettings.PrefabEntities[ model ];

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    this.ents.Add( em.Instantiate( ent ) );
                }
            }


            var tf = this.transform;
            for( var ent = getNext_( this.ents.First() ); ent != Entity.Null; ent = getNext_( ent ) )
            {
                this.nodes.Add( ent );
                new GameObject().transform.parent = tf;
            }

            Entity getNext_( Entity ent_ ) =>
                em.GetComponentData<LineParticlePointNodeLinkData>( ent_ ).NextNodeEntity;

        }

        List<Entity> nodes = new List<Entity>();


        private void Update()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var tfs = this.GetComponentsInChildren<Transform>().Skip( 1 );

            foreach( var x in (this.nodes, tfs).Zip() )
            {
                setPosition_( x.x, x.y.position );
            }

            void setPosition_( Entity ent_, float3 pos_ )
            {
                var tr = em.GetComponentData<Translation>( ent_ );
                tr.Value = pos_;
                em.SetComponentData( ent_, tr );
            }
        }

    }

}

