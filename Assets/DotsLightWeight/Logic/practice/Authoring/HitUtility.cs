using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Targeting;
    using DotsLite.Common.Extension;
    using DotsLite.Collision;

    
    static public class HitUtility
    {

        public static void AddHitTargetsAllRigidBody(this GameObjectConversionSystem gcs,
            ModelGroupAuthoring.ModelAuthoringBase top, Entity mainEntity, Hit.HitType type)
        {

            top.GetComponentsInChildren<Unity.Physics.Authoring.PhysicsBodyAuthoring>()
                //.Do(x => Debug.Log(x.name))
                .Select(x => gcs.GetPrimaryEntity(x))
                .ForEach(x => gcs.DstEntityManager.AddComponentData(x,
                    new Hit.TargetData
                    {
                        HitType = type,
                        MainEntity = mainEntity,
                    })
                );

        }


    }

}
