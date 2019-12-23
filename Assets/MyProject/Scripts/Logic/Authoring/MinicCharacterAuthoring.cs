﻿using System.Collections;
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

    public class MinicCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsAction )
        {

            var prefab = base.Convert( em, drawResources, initDrawModelComponentsAction );

            
            em.AddComponentData( prefab, new MinicWalkActionState { } );

            var post = em.GetComponentData<CharacterLinkData>( prefab );//
            em.AddComponentData( post.PostureEntity, new MoveHandlingData { } );//
            em.AddComponentData( post.PostureEntity, new HorizontalMovingTag { } );//
            em.AddComponentData( post.PostureEntity, new GroundHitResultData { } );//

            return prefab;
        }

    }

}
