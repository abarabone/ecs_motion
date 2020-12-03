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

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class WaponHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {
        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    var wapons = this.GetComponentsInChildren<IWaponAuthoring>()
        //        .Cast<WaponAuthoring>();

        //    foreach (var w in wapons)
        //    {
        //        (w.MainUnit as IDeclareReferencedPrefabs)?.DeclareReferencedPrefabs(referencedPrefabs);
        //        (w.SubUnit as IDeclareReferencedPrefabs)?.DeclareReferencedPrefabs(referencedPrefabs);
        //    }
        //}

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject.Ancestors().First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform.GetChild(0).gameObject;


            var wapons = this.GetComponentsInChildren<WaponAuthoring>();
            var qWaponInfos =
                from wapon in wapons

                let muzzle = wapon.transform.parent.gameObject
                let muzzleEntity = conversionSystem.GetPrimaryEntity(main)

                let qUnit = wapon.GetComponentsInChildren<IFunctionUnitAuthoring>()
                    .Select(u => (u as MonoBehaviour)?.gameObject)
                    .Where(x => x != null)
                    .Select(go => conversionSystem.GetPrimaryEntity(go))
                let unitEntity0 = qUnit.ElementAtOrDefault(0)
                let unitEntity1 = qUnit.ElementAtOrDefault(1)

                //let localMuzzlePosision 

                select (muzzleEntity, unitEntity0, unitEntity1)
                ;


            var holderEntity = entity;//conversionSystem.CreateAdditionalEntity(top);
            dstManager.AddBuffer<WaponHolder.LinkData>(holderEntity);

            var mainEntity = conversionSystem.GetPrimaryEntity(main);

            foreach (var (muzzleEntity, unitEntity0, unitEntity1) in qWaponInfos)
            {
                if (unitEntity0 != Entity.Null)
                {
                    dstManager.AddComponentData(unitEntity0,
                        new FunctionUnit.OwnerLinkData
                        {
                            OwnerMainEntity = mainEntity,
                            MuzzleBodyEntity = muzzleEntity,
                        }
                    );
                }
                if (unitEntity1 != Entity.Null)
                {
                    dstManager.AddComponentData(unitEntity1,
                        new FunctionUnit.OwnerLinkData
                        {
                            OwnerMainEntity = mainEntity,
                            MuzzleBodyEntity = muzzleEntity,
                        }
                    );
                }
                var holderBuf = dstManager.GetBuffer<WaponHolder.LinkData>(holderEntity);
                holderBuf.Add(new WaponHolder.LinkData
                {
                    FunctionEntity0 = unitEntity0,
                    FunctionEntity1 = unitEntity1,
                });
            }

        }
    }
}