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
    using DotsLite.Hit;


    /// <summary>
    /// 
    /// </summary>
    public class HitTargetAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public Hit.HitType HitType;



        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.FindParent<ModelGroupAuthoring.ModelAuthoringBase>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();


            var stateEntity = conversionSystem.GetOrCreateEntity(state);
            dstManager.AddComponentData(entity, new Hit.TargetData
            {
                StateEntity = stateEntity,
                HitType = this.HitType,
            });

        }

    }
}
