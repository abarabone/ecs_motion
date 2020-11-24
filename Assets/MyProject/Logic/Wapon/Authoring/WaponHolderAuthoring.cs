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

    public class WaponHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;

            var wapons = this.GetComponentsInChildren<WaponAuthoring>();

            var qWaponInfos =
                from wapon in wapons
                let muzzle = wapon.transform.parent
                select (wapon, muzzle)
                ;

            var qWaponData =
                from x in qWaponInfos
                select new 


            var holderEntity = conversionSystem.CreateAdditionalEntity(top);
            var holderBuf = dstManager.AddBuffer<WaponHolder.WaponData>(holderEntity);
            holderBuf.Add(new WaponHolder.WaponData
            {
                WaponEntity = 
            });

            var q =
                from x in qWaponInfos
                select new Arms.Wapon
        }
    }
}