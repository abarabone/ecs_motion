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

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;

namespace Abarabone.Authoring
{
    
    public class spawn : MonoBehaviour
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

                    em.SetComponentData( this.ents.Last(), new Translation { Value = this.transform.position.As_float3() + new float3( i, 0, -model * 3 ) } );
                    em.SetComponentData( this.ents.Last(), new Rotation { Value = quaternion.identity } );
                }
            }
            var a = new DrawInstanceEntity();


        }

    }

}

