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

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class MinicModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);


            dstManager.AddComponentData(entity, new MinicWalkActionState { });

            var post = dstManager.GetComponentData<CharacterLinkData>(entity);//
            dstManager.AddComponentData(post.PostureEntity, new MoveHandlingData { });//
            dstManager.AddComponentData(post.PostureEntity, new HorizontalMovingTag { });//
            dstManager.AddComponentData(post.PostureEntity, new GroundHitResultData { });//

        }

    }
}
