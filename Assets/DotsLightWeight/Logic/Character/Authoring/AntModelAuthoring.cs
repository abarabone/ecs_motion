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

namespace Abarabone.Model.Authoring
{
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;

    /// <summary>
    /// 
    /// </summary>
    public class AntModelAuthoring : CharacterModelAuthoring, IConvertGameObjectToEntity
    {


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
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

                var types = new ComponentTypes(new ComponentType []
                {
                    //typeof(PlayerTag),
                    typeof(AntTag),

                    typeof(WallingTag),
                    typeof(WallHunggingData),
                    //typeof(WallHitResultData),
                    typeof(PhysicsGravityFactor),// デフォルトでは付かないっぽい
                    
                    typeof(MoveHandlingData),

                    typeof(AntWalkActionState)
                });
                em.AddComponents(mainEntity, types);
                em.SetComponentData(mainEntity, new PhysicsGravityFactor
                {
                    Value = 1.0f,
                });
            }


        }

    }
}
