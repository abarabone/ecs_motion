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


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);


            var top = this.gameObject;
            var main = top.transform.GetChild(0).gameObject;

            initMain_(conversionSystem, main);

            return;


            void initMain_( GameObjectConversionSystem gcs_, GameObject main_ )
            {
                var em = gcs_.DstEntityManager;

                var mainEntity = gcs_.GetPrimaryEntity(main_);

                var types = new ComponentTypes
                (
                    typeof(PlayerTag),

                    typeof(HorizontalMovingTag),
                    typeof(MoveHandlingData),
                    typeof(GroundHitResultData),

                    typeof(MinicWalkActionState)
                );
                em.AddComponents(mainEntity, types);
            }


        }

    }
}
