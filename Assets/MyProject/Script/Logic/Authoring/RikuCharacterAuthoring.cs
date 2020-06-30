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

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;
using Abarabone.Common.Extension;

namespace Abarabone.Authoring
{

    public class RikuCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, int, Entity> initDrawModelComponentsAction )
        {

            var prefab = base.Convert( em, initDrawModelComponentsAction );

            em.AddComponentData( prefab, new SoldierWalkActionState { } );

            var post = em.GetComponentData<ObjectMainCharacterLinkData>( prefab );//
            em.AddComponentData( post.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( post.PostureEntity, new HorizontalMovingTag { } );//
            em.AddComponentData( post.PostureEntity, new GroundHitResultData { } );//

            return prefab;
        }

    }

}
