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

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;
using Abarabone.Common.Extension;

using Material = UnityEngine.Material;

namespace Abarabone.Authoring
{

    public class AntCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, int, Entity> initDrawModelComponentsAction )
        {

            var prefab = base.Convert( em, initDrawModelComponentsAction );

            em.AddComponentData( prefab, new AntTag { } );
            em.AddComponentData( prefab, new AntWalkActionState { } );

            var linker = em.GetComponentData<CharacterLinkData>( prefab );//
            em.AddComponentData( linker.PostureEntity, new AntTag { } );//
            em.AddComponentData( linker.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( linker.PostureEntity, new WallHunggingData { } );//
            //em.RemoveComponent<PhysicsVelocity>( linker.PostureEntity );//

            em.AddComponentData( linker.PostureEntity, new WallingTag { } );
            //em.AddComponentData( linker.PostureEntity, new WallHitResultData { } );
            em.AddComponentData( linker.PostureEntity, new PhysicsGravityFactor { Value = 1.0f } );

            return prefab;
        }

    }

}
