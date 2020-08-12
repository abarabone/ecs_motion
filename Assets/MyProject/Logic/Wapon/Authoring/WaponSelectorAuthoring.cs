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

            //var top = this.GetComponentInParent<CharacterModelAuthoring>();// プレハブだとできない（ children はできるのに）
            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;



            var waponSelectorEntity = conversionSystem.CreateAdditionalEntity(this.gameObject);
            dstManager.SetName_(waponSelectorEntity, $"{main.name} wapon selector");

            var types = new ComponentTypes
            (
                typeof(WaponSelector.LinkData),
                typeof(WaponSelector.ToggleModeData),
                typeof(WaponSelector.WaponLink0)
                //typeof(Disabled)// 何も持っていない状態用
            );
            dstManager.AddComponents(waponSelectorEntity, types);

            dstManager.SetComponentData(waponSelectorEntity,
                new WaponSelector.LinkData
                {
                    OwnerMainEntity = conversionSystem.GetPrimaryEntity(main),
                    muzzleBodyEntity = entity,
                }
            );
            dstManager.SetComponentData(waponSelectorEntity,
                new WaponSelector.ToggleModeData
                {
                    CurrentWaponCarryId = 0,
                    WaponCarryLength = 0,
                }
            );

            if (this.Wapons.Length >= 2) dstManager.AddChunkComponentData<WaponSelector.WaponLink1>(waponSelectorEntity);
            if (this.Wapons.Length >= 3) dstManager.AddChunkComponentData<WaponSelector.WaponLink2>(waponSelectorEntity);
            if (this.Wapons.Length >= 4) dstManager.AddChunkComponentData<WaponSelector.WaponLink3>(waponSelectorEntity);



            if (this.Wapons.Length > 0)
            {
                var msg = new WaponMessage.ReplaceWapon4MsgData
                {
                    NumPeplace = math.min(this.Wapons.Where(x => x != null).Count(), 4),
                    WaponPrefab0 = this.Wapons.Length >= 1 ? conversionSystem.GetPrimaryEntity(this.Wapons[0]) : Entity.Null,
                    WaponPrefab1 = this.Wapons.Length >= 2 ? conversionSystem.GetPrimaryEntity(this.Wapons[1]) : Entity.Null,
                    WaponPrefab2 = this.Wapons.Length >= 3 ? conversionSystem.GetPrimaryEntity(this.Wapons[2]) : Entity.Null,
                    WaponPrefab3 = this.Wapons.Length >= 4 ? conversionSystem.GetPrimaryEntity(this.Wapons[3]) : Entity.Null,
                };
                dstManager.AddComponentData(waponSelectorEntity, msg);
            }



            //var waponEnities = this.Wapons
            //    .Where(x => x != null)
            //    .Take(4)
            //    .Select(w => conversionSystem.GetPrimaryEntity(w))
            //    .ToArray();

            //if (waponEnities.Length >= 1)
            //    dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab0 { WaponPrefab = waponEnities[0] });
            //if (waponEnities.Length >= 2)
            //    dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab1 { WaponPrefab = waponEnities[1] });
            //if (waponEnities.Length >= 3)
            //    dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab2 { WaponPrefab = waponEnities[2] });
            //if (waponEnities.Length >= 4)
            //    dstManager.AddComponentData(waponSelectorEntity, new WaponSelector.WaponPrefab3 { WaponPrefab = waponEnities[3] });


        }

    }
}