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
            initPosture_(conversionSystem, posture);

            return;


            static void initPosture_
                (GameObjectConversionSystem gcs, PostureAuthoring posture)
            {
                var em = gcs.DstEntityManager;

                var postureEntity = gcs.GetPrimaryEntity(posture);

                var types = new ComponentTypes
                (
                    typeof(PlayerTag),

                    typeof(HorizontalMovingTag),
                    typeof(MoveHandlingData),
                    typeof(GroundHitResultData)
                );
                em.AddComponents(postureEntity, types);
            }

            static void initState_
                (GameObjectConversionSystem gcs, ActionStateAuthoring state)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes//em.CreateArchetype
                (
                    typeof(PlayerTag),

                    typeof(MinicWalkActionState)
                    //typeof(CharacterAction.LinkData)
                );
                var ent = gcs.GetEntityDictionary()[state];
                em.AddComponents(ent, types);


                //var postureEntity = gcs.GetPrimaryEntity(posture);
                //em.SetComponentData(ent, new CharacterAction.LinkData
                //{ 
                //    MainEntity = postureEntity,

                //});
            }


        }

    }
}
