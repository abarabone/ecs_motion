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
    using Abarabone.Motion;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class MinicModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {

        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);

            var gcs = conversionSystem;
            var em = dstManager;


            var top = this.gameObject;
            var main = top.transform.GetChild(0).gameObject;
            
            var mainEntity = gcs.GetPrimaryEntity(main);


            var types = new ComponentTypes(
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
