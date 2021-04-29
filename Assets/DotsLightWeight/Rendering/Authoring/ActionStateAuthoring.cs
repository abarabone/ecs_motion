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

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw.Authoring;
    using DotsLite.Character;
    using DotsLite.Common.Extension;
    using DotsLite.CharacterMotion.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Collision;


    public class ActionStateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.GetComponentInChildren<ModelGroupAuthoring.ModelAuthoringBase>(); 
            var posture = this.GetComponentInChildren<PostureAuthoring>();
            var state = this;//.GetComponentInChildren<ActionStateAuthoring>();
            var motion = this.GetComponentInChildren<MotionAuthoring>();

            createStateEntity_(conversionSystem, top, posture, state, motion);

            var ent = conversionSystem.GetOrCreateEntity(state);
            conversionSystem.AddHitTargetsAllRigidBody(top, ent, Hit.HitType.charactor);

            return;





            static void createStateEntity_(
                GameObjectConversionSystem gcs,
                ModelGroupAuthoring.ModelAuthoringBase top,
                PostureAuthoring posture, ActionStateAuthoring state, MotionAuthoring motion)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes
                (
                    typeof(ActionState.BinderLinkData),
                    typeof(ActionState.PostureLinkData),
                    typeof(ActionState.MotionLinkDate)
                );
                //var ent = gcs.CreateAdditionalEntity(top.gameObject, types);
                var ent = gcs.GetOrCreateEntity(state, types);


                em.SetComponentData(ent,
                    new ActionState.BinderLinkData
                    {
                        BinderEntity = gcs.GetPrimaryEntity(top),
                    }
                );
                em.SetComponentData(ent,
                    new ActionState.PostureLinkData
                    {
                        PostureEntity = gcs.GetPrimaryEntity(posture),
                    }
                );
                em.SetComponentData(ent,
                    new ActionState.MotionLinkDate
                    {
                        MotionEntity = gcs.GetOrCreateEntity(motion),
                    }
                );

                em.SetName_(ent, $"{top.name} state");
            }

        }
    }
}