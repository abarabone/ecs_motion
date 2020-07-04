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

namespace Abarabone.Authoring
{
    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Misc;
    using Abarabone.CharacterMotion;
    using Abarabone.Draw;
    using Abarabone.Character;
    using Abarabone.Common.Extension;
    using Abarabone.Model;


    public class MinicCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, int, Entity> initDrawModelComponentsAction )
        {

            var prefab = base.Convert( em, initDrawModelComponentsAction );

            
            em.AddComponentData( prefab, new MinicWalkActionState { } );

            var post = em.GetComponentData<ObjectMainCharacterLinkData>( prefab );//
            em.AddComponentData( post.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( post.PostureEntity, new HorizontalMovingTag { } );//
            em.AddComponentData( post.PostureEntity, new GroundHitResultData { } );//

            return prefab;
        }

    }

}
