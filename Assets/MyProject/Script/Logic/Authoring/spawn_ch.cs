﻿using System.Collections;
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

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;

namespace Abarabone.Authoring
{
    
    public class spawn_ch : MonoBehaviour
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

                var mlinker = em.GetComponentData<CharacterLinkData>( ent );
                ref var mclip = ref em.GetComponentData<MotionClipData>( mlinker.MainMotionEntity ).ClipData.Value;

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    this.ents.Add( em.Instantiate( ent ) );

                    var chlinker = em.GetComponentData<CharacterLinkData>( this.ents.Last() );
                    em.SetComponentData( chlinker.PostureEntity, new Translation { Value = this.transform.position.As_float3() + new float3( i * 3, 0, -model * 5 ) } );
                    em.SetComponentData( chlinker.MainMotionEntity, new MotionInitializeData { MotionIndex = i % mclip.Motions.Length } );
                }
            }


            // 先頭キャラのみ
            em.AddComponentData( this.ents[ 0 ], new PlayerTag { } );
            var post = em.GetComponentData<CharacterLinkData>( this.ents[ 0 ] ).PostureEntity;
            em.AddComponentData( post, new PlayerTag { } );//
            //em.AddComponentData( post, new MoveHandlingData { } );//
            
        }
        
    }

}

