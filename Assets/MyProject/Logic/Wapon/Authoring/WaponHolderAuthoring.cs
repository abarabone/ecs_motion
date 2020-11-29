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


            //var wapons = this.GetComponentsInChildren<IWaponAuthoring>()
            //    .Cast<WaponAuthoring>();
            //var qWaponInfos =
            //    from wapon in wapons
            //    let muzzle = wapon.transform.parent.gameObject
            //    select (wapon, muzzle)
            //    ;


            //var holderEntity = conversionSystem.CreateAdditionalEntity(top);
            //var holderBuf = dstManager.AddBuffer<WaponHolder.LinkData>(holderEntity);

            //foreach(var i in qWaponInfos)
            //{
            //    holderBuf.Add(new WaponHolder.LinkData
            //    {
            //        FunctionEntity0 = createFunctionUnitAdditionalEntity_(top, main, i.wapon.MainUnit, i.muzzle),
            //        FunctionEntity1 = createFunctionUnitAdditionalEntity_(top, main, i.wapon.SubUnit, i.muzzle),
            //    });
            //}

            return;


            Entity createFunctionUnitAdditionalEntity_
                (GameObject top, GameObject main, IFunctionUnitAuthoring unit, GameObject muzzle)
            {
                if (unit is IConvertGameObjectToEntity functionUnit)
                {
                    var ent = conversionSystem.CreateAdditionalEntity(top);

                    functionUnit.Convert(ent, dstManager, conversionSystem);

                    dstManager.AddComponentData(ent,
                        new FunctionUnit.OwnerLinkData
                        {
                            OwnerMainEntity = conversionSystem.GetPrimaryEntity(main),
                            MuzzleBodyEntity = conversionSystem.GetPrimaryEntity(muzzle),
                        }
                    );

                    return entity;
                }

                return Entity.Null;
            }
        }
    }
}