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

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Character;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public class RikuCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources, Action<Mesh, Material, BoneType> initDrawModelComponentsAction )
        {

            var prefab = base.Convert( em, drawResources, initDrawModelComponentsAction );

            em.AddComponentData( prefab, new SoldierWalkActionState { } );

            var post = em.GetComponentData<CharacterLinkData>( prefab );//
            em.AddComponentData( post.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( post.PostureEntity, new HorizontalMovingTag { } );//
            em.AddComponentData( post.PostureEntity, new GroundHitResultData { } );//

            return prefab;
        }

    }

}
