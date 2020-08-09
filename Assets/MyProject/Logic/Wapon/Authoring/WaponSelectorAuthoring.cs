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

    public class WaponSelectorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public WaponAuthoring[] Wapons;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var qWapon = this.Wapons
                .Take(4)
                .Select(w => conversionSystem.GetPrimaryEntity(w))
                .Select(wapon => enumWapon_(wapon))
                .ForEach(holder => dstManager.AddComponentData(entity, holder));

            return;


            IEnumerable<WaponSelector.IWaponEntityHolder> enumWapon_(Entity ent)
            {
                yield return new WaponSelector.WaponEntity0 { WaponEntity = ent };
                yield return new WaponSelector.WaponEntity1 { WaponEntity = ent };
                yield return new WaponSelector.WaponEntity2 { WaponEntity = ent };
                yield return new WaponSelector.WaponEntity3 { WaponEntity = ent };
            }
        }

    }
}