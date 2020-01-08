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
    
    public class spawn : MonoBehaviour
    {
        
        public int InstanceCountPerModel = 100;
        
        List<Entity> ents = new List<Entity>();


        void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var drawSettings = FindObjectOfType<DrawPrefabSettingsAuthoring>();


            // モーション設定
            foreach( var model in Enumerable.Range( 0, drawSettings.PrefabEntities.Length ) )
            {
                var mlinker = em.GetComponentData<CharacterLinkData>( drawSettings.PrefabEntities[ model ] );
                ref var mclip = ref em.GetComponentData<MotionClipData>( mlinker.MainMotionEntity ).ClipData.Value;

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    this.ents.Add( em.Instantiate( drawSettings.PrefabEntities[ model ] ) );

                    var chlinker = em.GetComponentData<CharacterLinkData>( this.ents[ this.ents.Count - 1 ] );
                    em.SetComponentData( chlinker.PostureEntity, new Translation { Value = new float3( i * 3, 0, -model * 5 ) } );
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

