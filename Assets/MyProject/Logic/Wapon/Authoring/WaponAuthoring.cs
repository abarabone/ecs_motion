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

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Unity.Physics.Authoring;
    using Abarabone.Model;

    /// <summary>
    /// WaponEntity はインスタンス化しない。
    /// FunctionUnit をインスタンス化するためのリファレンスでしかない。
    /// </summary>
    public partial class WaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        public FunctionUnitAuthoringBase UnitForMainTrigger;
        public FunctionUnitAuthoringBase UnitForSubTrigger;

        

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var units = this.GetComponentsInChildren<FunctionUnitAuthoringBase>()
                .Select(x => x.gameObject)
                .Select(x => conversionSystem.GetPrimaryEntity(x))
                .ToArray();

            addTriggerType_(conversionSystem, this.UnitForMainTrigger, FunctionUnitWithWapon.TriggerType.main);
            addTriggerType_(conversionSystem, this.UnitForSubTrigger, FunctionUnitWithWapon.TriggerType.sub);

            return;


            void addTriggerType_
                (GameObjectConversionSystem gcs_, FunctionUnitAuthoringBase unit, FunctionUnitWithWapon.TriggerType type)
            {
                if (unit == null) return;

                var em = gcs_.DstEntityManager;


                var ent = conversionSystem.GetPrimaryEntity(unit);

                em.AddComponentData(ent,
                    new FunctionUnitWithWapon.TriggerTypeData
                    {
                        Type = type,
                    }
                );
            }

        }
    }

}
