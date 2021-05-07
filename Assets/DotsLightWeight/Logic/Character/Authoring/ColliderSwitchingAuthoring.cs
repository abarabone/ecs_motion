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

namespace DotsLite.Character.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Targeting;
    using DotsLite.Collision;
    using DotsLite.Arms.Authoring;

    using Collide = Unity.Physics.Collider;

    /// <summary>
    /// 
    /// </summary>
    public class ColliderSwitchingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Mode mode;

        public enum Mode
        {
            none,
            deading
        }


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var em = dstManager;
            var gcs = conversionSystem;

            var top = this.FindParent<ModelGroupAuthoring.ModelAuthoringBase>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();
            var posture = top.GetComponentInChildren<PostureAuthoring>();

            var binderEntity = gcs.GetPrimaryEntity(top);
            var stateEntity = gcs.GetOrCreateEntity(state);
            var postureEntity = gcs.GetPrimaryEntity(posture);

            //var collider = em.GetComponentData<PhysicsCollider>(entity);
            var collider = 

            switch (this.mode)
            {
                case Mode.deading:

                    em.AddComponentData(stateEntity, new ColliderBank.DeadData
                    {
                        Collider = collider.Value,
                    });

                    break;
            }

            dstManager.DestroyEntity(entity);
        }
    }
}
