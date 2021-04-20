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
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class MinicModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);


            var state = this.GetComponentInChildren<ActionStateAuthoring>();
            initState_(conversionSystem, state);

            var posture = this.GetComponentInChildren<PostureAuthoring>();
            initPosture_(conversionSystem, posture, state);

            return;


            static void initPosture_
                (GameObjectConversionSystem gcs, PostureAuthoring posture, ActionStateAuthoring state)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(posture);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(PlayerTag),

                    typeof(HorizontalMovingTag),
                    //typeof(MoveHandlingData),
                    typeof(GroundHitResultData),

                    typeof(Control.MoveData),
                    typeof(Control.WorkData),
                    typeof(Control.ActionLinkData)
                });
                em.AddComponents(ent, types);

                var stateEntity = gcs.GetEntityDictionary()[state];
                em.SetComponentData(ent, new Control.ActionLinkData { ActionEntity = stateEntity });
            }

            static void initState_
                (GameObjectConversionSystem gcs, ActionStateAuthoring state)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes//em.CreateArchetype
                (
                    typeof(PlayerTag),

                    typeof(MinicWalkActionState),

                    typeof(Control.ActionData)
                );
                var ent = gcs.GetEntityDictionary()[state];
                em.AddComponents(ent, types);
            }


        }

    }
}
