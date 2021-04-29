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

namespace DotsLite.Arms.Authoring
{
    public partial class WaponAuthoring : MonoBehaviour, IWaponAuthoring, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var units = this.GetComponentsInChildren<IFunctionUnitAuthoring>()
                .Cast<MonoBehaviour>();

            dstManager.AddComponentData(entity,
                new WaponTemplate.UnitsData
                {
                    FunctionEntity0 = conversionSystem.GetPrimaryEntity(units.ElementAtOrDefault(0)),
                    FunctionEntity1 = conversionSystem.GetPrimaryEntity(units.ElementAtOrDefault(1)),
                }
            );
        }
    }
}
