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

        public WaponAuthoring[] Wapons;


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


            var holderEntity = entity;//conversionSystem.CreateAdditionalEntity(top);
            dstManager.AddBuffer<WaponHolder.LinkData>(holderEntity);

            var mainEntity = conversionSystem.GetPrimaryEntity(main);


            var wapons = this.GetComponentsInChildren<WaponAuthoring>();


            static (Entity muzzle, Entity[] units)[] listupWaponEntities_
                (GameObjectConversionSystem gcs, GameObject main, IEnumerable<WaponAuthoring> wapons)
            {
                var qWaponEntities =
                    from wapon in wapons

                    let muzzle = wapon.transform.parent.gameObject
                    let muzzleEntity = gcs.GetPrimaryEntity(main)

                    let qUnitEntity = wapon.GetComponentsInChildren<IFunctionUnitAuthoring>()
                        .Select(u => (u as MonoBehaviour)?.gameObject)
                        .Where(x => x != null)
                        .Select(go => gcs.GetPrimaryEntity(go))

                    select (muzzleEntity, qUnitEntity.ToArray())
                    ;

                return qWaponEntities.ToArray();
            }

            static void initWapon_
                (GameObjectConversionSystem gcs,
                Entity main, Entity holder, IEnumerable<(Entity muzzle, IEnumerable<Entity> units)> wapons)
            {
                var em = gcs.DstEntityManager;

                var q =
                    from wapon in wapons.WithIndex()
                    from unit in wapon.src.units.WithIndex()
                    select (wapon.src.muzzle, unit.src, wapon.i, unit.i)
                    ;
                foreach (var (muzzle, unit, wid, uid) in q)
                {
                    addFunctionUnitComponents_(muzzle, unit, wid, uid);
                }

                void addFunctionUnitComponents_
                    (Entity muzzle, Entity unit, int waponId, int unitId)
                {
                    em.AddComponentData(unit,
                        new FunctionUnit.OwnerLinkData
                        {
                            OwnerMainEntity = main,
                            MuzzleBodyEntity = muzzle,
                        }
                    );
                    em.AddComponentData(unit,
                        new FunctionUnitWithWapon.TriggerSpecificData
                        {
                            Type = (FunctionUnitWithWapon.TriggerType)unitId,
                            WaponCarryId = waponId,
                        }
                    );
                }
                void Add()
                {
                    var holderBuf = em.GetBuffer<WaponHolder.LinkData>(holderEntity);
                    holderBuf.Add(new WaponHolder.LinkData
                    {
                        FunctionEntity0 = unitEntity0,
                        FunctionEntity1 = unitEntity1,
                    });
                }
            }

            foreach (var ((muzzleEntity, unitEntity0, unitEntity1), i) in qWaponInfos.WithIndex())
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
                    dstManager.AddComponentData(unitEntity0,
                        new FunctionUnitWithWapon.TriggerSpecificData
                        {
                            Type = FunctionUnitWithWapon.TriggerType.main,
                            WaponCarryId = i,
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
                    dstManager.AddComponentData(unitEntity1,
                        new FunctionUnitWithWapon.TriggerSpecificData
                        {
                            Type = FunctionUnitWithWapon.TriggerType.sub,
                            WaponCarryId = i,
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

            dstManager.AddComponentData(holderEntity,
                new WaponHolder.SelectorData
                { 
                    CurrentWaponIndex = 0,
                    Length = wapons.Length,
                }
            );

        }
    }
}