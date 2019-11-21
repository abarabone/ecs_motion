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
    /// <summary>
    /// つかわない
    /// </summary>
    public class PlayerCharacterAuthoring : CharacterAuthoring
    {



        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources )
        {

            var prefab = base.Convert( em, drawResources );


            em.AddComponentData( prefab, new PlayerTag { } );

            //em.World.GetExistingSystem<PlayerMoveSystem>().TfCamera = this.Camera.transform;
            

            return prefab;
        }

    }

}