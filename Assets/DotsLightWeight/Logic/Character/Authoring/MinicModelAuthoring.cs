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


            var top = this.gameObject;
            var main = top.transform.GetChild(0).gameObject;

            initMain_(conversionSystem, main);

            createStateEntity_(conversionSystem, main);

            return;


            static void initMain_(GameObjectConversionSystem gcs, GameObject main)
            {
                var em = gcs.DstEntityManager;

                var mainEntity = gcs.GetPrimaryEntity(main);

                var types = new ComponentTypes
                (
                    typeof(PlayerTag),

                    typeof(HorizontalMovingTag),
                    typeof(MoveHandlingData),
                    typeof(GroundHitResultData)
                );
                em.AddComponents(mainEntity, types);
            }

            static void createStateEntity_(GameObjectConversionSystem gcs, GameObject main)
            {
                var em = gcs.DstEntityManager;

                var types = em.CreateArchetype
                (
                    typeof(MinicWalkActionState),
                    typeof(CharacterAction.LinkData)
                );
                var ent = em.CreateEntity(types);


                var mainEntity = gcs.GetPrimaryEntity(main);
                em.SetComponentData(ent, new CharacterAction.LinkData
                { 
                    MainEntity = mainEntity,

                });


                em.SetName_(ent, $"{main.transform.parent.name} state");
            }


        }

    }
}
