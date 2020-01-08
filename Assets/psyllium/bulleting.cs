using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;
using Abss.Draw;

public class bulleting : MonoBehaviour
{
    void Start()
    {

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        createBullet_( new float3( 0, 0, 0 ) );
        createBullet_( new float3( 2, 0, 0 ) );



        Entity createBullet_( float3 pos )
        {
            var ent = em.CreateEntity();
            em.AddComponentData( ent, new Translation { Value = pos } );
            em.AddComponentData( ent, new Rotation { Value = quaternion.identity } );

            return ent;
        }
    }
}
