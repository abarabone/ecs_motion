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


            // モーション設定
            foreach( var model in Enumerable.Range( 0, drawSettings.PrefabEntities.Length ) )
            {
                var ent = drawSettings.PrefabEntities[ model ];

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    this.ents.Add( em.Instantiate( ent ) );
                }
            }

            Enumerable
                .Repeat( new GameObject(), this.ents.Count )
                .ForEach( go => go.transform.parent = this.transform );
        }


        private void Update()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            this.gameObject
                .Descendants()
                .Select( go => go.transform )
                .Zip( this.positions, ( tf, ent ) => (tf, ent) )
                .Do( x => em.SetComponentData( x.ent, new Translation { Value = x.tf.position } ) );

            for( var ent = getNext_( instanceEntity_ ); ent != Entity.Null; ent = getNext_( ent ) )
            {



            }

            Entity getNext_( Entity ent_ ) =>
                em.GetComponentData<LineParticlePointNodeLinkData>( ent_ ).NextNodeEntity;
        }

    }

}

