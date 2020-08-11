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
    public class WaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        public class FunctionUnitAuthoring : MonoBehaviour
        { }


        public FunctionUnitAuthoring UnitForMainTrigger;
        public FunctionUnitAuthoring UnitForSubTrigger;




        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    var qUnit = this.GetComponentsInChildren<FunctionUnitAuthoring>()
        //        //.Take(4)// とりあえず４つまでとする
        //        .Select(x => x.gameObject);

        //    referencedPrefabs.AddRange(qUnit);
        //}


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            //var units = this.GetComponentsInChildren<FunctionUnitAuthoring>()
            //    //.Take(4)// とりあえず４つまでとする
            //    .Select(x => x.gameObject)
            //    .Select(x => conversionSystem.GetPrimaryEntity(x))
            //    .ToArray();

            //addWaponComponents_(conversionSystem, entity);

            //initFunctionUnit_(conversionSystem, entity, units);

            //setFunctionChainLink_(conversionSystem, entity, units);

            addTriggerType_(conversionSystem, this.UnitForMainTrigger, FunctionUnitWithWapon.TriggerType.main);
            addTriggerType_(conversionSystem, this.UnitForSubTrigger, FunctionUnitWithWapon.TriggerType.sub);

            return;


            void addTriggerType_
                (GameObjectConversionSystem gcs_, FunctionUnitAuthoring unit, FunctionUnitWithWapon.TriggerType type)
            {
                if (unit == null) return;

                var ent = conversionSystem.GetPrimaryEntity(unit);
                dstManager.AddComponentData(ent,
                    new FunctionUnitWithWapon.TriggerTypeData
                    {
                        Type = type,
                    }
                );
            }

            //void addWaponComponents_(GameObjectConversionSystem gcs_, Entity wapon_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var addtypes = new ComponentTypes
            //    (
            //        //typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
            //        typeof(Wapon.FunctionUnitPrefabsData)
            //    );
            //    em.AddComponents(wapon_, addtypes);
            //}

            //void initFunctionUnit_(GameObjectConversionSystem gcs_, Entity wapon_, Entity[] units_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var prefabs = new Wapon.FunctionUnitPrefabsData { };

            //    if (units.Length >= 1) prefabs.FunctionUnitPrefab0 = units_[0];
            //    if (units.Length >= 2) prefabs.FunctionUnitPrefab0 = units_[1];
            //    if (units.Length >= 3) prefabs.FunctionUnitPrefab0 = units_[2];
            //    if (units.Length >= 4) prefabs.FunctionUnitPrefab0 = units_[3];

            //    dstManager.SetComponentData(wapon_, prefabs);
            //}

            //void setFunctionChainLink_(GameObjectConversionSystem gcs_, Entity wapon_, Entity[] units_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var nexts = units_.Skip(1).Append(Entity.Null);
            //    foreach( var (unit, next) in (units, nexts).Zip())
            //    {
            //        em.AddComponentData(unit,
            //            new FunctionUnit.UnitChainLinkData
            //            {
            //                NextUnitEntity = next,
            //            }
            //        );
            //    }
            //}

        }
    }

}
