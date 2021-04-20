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

namespace Abarabone.Model.Authoring
{
    using Abarabone.Draw.Authoring;
    using Abarabone.Character;
    using Abarabone.Common.Extension;
    using Abarabone.CharacterMotion.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Utilities;


    public class ActionStateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            var top = this.GetComponentInChildren<ModelGroupAuthoring.ModelAuthoringBase>(); 
            var posture = this.GetComponentInChildren<PostureAuthoring>();
            var state = this;//.GetComponentInChildren<ActionStateAuthoring>();
            var motion = this.GetComponentInChildren<MotionAuthoring>();

            createStateEntity_(conversionSystem, top, posture, state, motion);

            return;





            static void createStateEntity_(
                GameObjectConversionSystem gcs,
                ModelGroupAuthoring.ModelAuthoringBase top,
                PostureAuthoring posture, ActionStateAuthoring state, MotionAuthoring motion)
            {
                var em = gcs.DstEntityManager;

                var types = em.CreateArchetype//new ComponentTypes
                (
                    typeof(ActionState.BinderLinkData),
                    typeof(ActionState.PostureLinkData),
                    typeof(ActionState.MotionLinkDate)
                );
                var ent = gcs.CreateAdditionalEntity(top.gameObject, types);


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
                        MotionEntity = gcs.GetEntityDictionary()[motion],
                    }
                );

                em.SetName_(ent, $"{top.name} state");
                gcs.GetEntityDictionary().Add(state, ent);
            }

        }
    }
}