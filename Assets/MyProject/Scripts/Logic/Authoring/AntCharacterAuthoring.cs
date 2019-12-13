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
using Unity.Physics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Character;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public class AntCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var prefab = base.Convert( em, drawResources );

            em.AddComponentData( prefab, new AntTag { } );
            em.AddComponentData( prefab, new AntWalkActionState { } );

            var linker = em.GetComponentData<CharacterLinkData>( prefab );//
            em.AddComponentData( linker.PostureEntity, new AntTag { } );//
            em.AddComponentData( linker.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( linker.PostureEntity, new WallHunggingData { } );//
            //em.RemoveComponent<PhysicsVelocity>( post.PostureEntity );//

            em.AddComponentData<WallingTag>( linker.PostureEntity, new WallingTag { } );
            //em.AddComponentData( post.PostureEntity, new WallHitResultData { } );
            em.AddComponentData( linker.PostureEntity, new PhysicsGravityFactor { } );

            return prefab;
        }

    }

}
