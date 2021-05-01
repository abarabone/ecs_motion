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
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class MinicModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {

        public float ArmorDurability;


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);


            var state = this.GetComponentInChildren<ActionStateAuthoring>();
            initState_(conversionSystem, state, this);

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

                em.SetComponentData(ent, new Control.ActionLinkData
                {
                    ActionEntity = gcs.GetOrCreateEntity(state),
                });
            }

            static void initState_
                (GameObjectConversionSystem gcs, ActionStateAuthoring state, MinicModelAuthoring minic)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes
                (
                    typeof(PlayerTag),

                    typeof(MinicWalkActionState),
                    typeof(Armor.SimpleDamageData),

                    typeof(Control.ActionData)
                );
                var ent = gcs.GetOrCreateEntity(state, types);

                em.SetComponentData(ent, new Armor.SimpleDamageData
                {
                    Durability = minic.ArmorDurability,
                });
            }


        }

    }
}
