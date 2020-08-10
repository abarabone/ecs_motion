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

        public GameObject Muzzle;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            //var top = this.GetComponentInParent<CharacterModelAuthoring>();// プレハブだとできない（ children はできるのに）
            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;


            var waponSelectorEntity = conversionSystem.CreateAdditionalEntity(this.gameObject);

            dstManager.AddComponentData(waponSelectorEntity,
                new WaponSelector.CharacterMainLink
                {
                    CharacterMainEntity = conversionSystem.GetPrimaryEntity(main),
                }
            );

            dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.ToggleModeTag { } );

            var waponEnities = this.Wapons
                .Where(x => x != null)
                .Take(4)
                .Select(w => conversionSystem.GetPrimaryEntity(w))
                .ToArray();

            if (waponEnities.Length >= 1)
                dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab0 { WaponPrefab = waponEnities[0] });
            if (waponEnities.Length >= 2)
                dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab1 { WaponPrefab = waponEnities[1] });
            if (waponEnities.Length >= 3)
                dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab2 { WaponPrefab = waponEnities[2] });
            if (waponEnities.Length >= 4)
                dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab3 { WaponPrefab = waponEnities[3] });


        }

    }
}