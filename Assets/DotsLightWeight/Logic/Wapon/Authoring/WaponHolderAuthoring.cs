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

    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class WaponHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public WaponAuthoring[] Wapons;
        public GameObject Muzzle;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var qWapon = this.Wapons
                //.Select(x => x.wapon)
                //.Cast<MonoBehaviour>()
                .Select(x => x.gameObject);

            referencedPrefabs.AddRange(qWapon);
        }



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject
                .Ancestors()
                .First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform
                .GetChild(0)
                .gameObject;


            var holderEntity = entity;
            var mainEntity = conversionSystem.GetPrimaryEntity(main);
            var muzzleEntity = conversionSystem.GetPrimaryEntity(this.Muzzle);

            //var unitEntities = listupMuzzleAndUnitsEntities_(conversionSystem, this.wapons);
            initHolder_(conversionSystem, holderEntity, mainEntity, muzzleEntity, this.Wapons.Length, startSelectedId: 0);
            //initAllWapons_(conversionSystem, holderEntity, unitEntities);
            //initAllUnits_(conversionSystem, mainEntity, unitEntities);
            //createInitUnit2Entities_(conversionSystem, holderEntity, this.Wapons);

            return;


            //static (Entity muzzle, Entity[] units)[] listupMuzzleAndUnitsEntities_
            //    (GameObjectConversionSystem gcs, IEnumerable<WaponUnits> wapons)
            //{

            //    var qWaponEntities =
            //        from w in wapons

            //        let qUnitEntity =
            //            from u in w.wapon.GetComponentsInChildren<IFunctionUnitAuthoring>()
            //            let ubehviour = u as MonoBehaviour//)?.gameObject
            //            where ubehviour != null
            //            select gcs.GetPrimaryEntity(ubehviour)

            //        let muzzle = gcs.GetPrimaryEntity(w.muzzle)

            //        select (muzzle, qUnitEntity.ToArray())
            //        ;

            //    return qWaponEntities.ToArray();
            //}

            static void initHolder_
                (GameObjectConversionSystem gcs, Entity holder, Entity main, Entity muzzle, int waponsLength, int startSelectedId)
            {
                var em = gcs.DstEntityManager;

                em.AddComponentData(holder,
                    new WaponHolder.OwnerLinkData
                    {
                        OwnerEntity = main,
                        MuzzleEntity = muzzle,
                    }
                );

                em.AddComponentData(holder,
                    new WaponHolder.SelectorData
                    {
                        CurrentWaponIndex = startSelectedId,
                        Length = waponsLength,
                    }
                );

                em.AddBuffer<WaponHolder.UnitLinkData>(holder);
            }
            //static void initAllWapons_
            //    (GameObjectConversionSystem gcs, Entity holder, (Entity muzzle, Entity[] units)[] unitsList)
            //{
            //    var em = gcs.DstEntityManager;

            //    foreach (var wapon in unitsList)
            //    {
            //        var unitPrefab0 = wapon.units.ElementAtOrDefault(0);
            //        var unitPrefab1 = wapon.units.ElementAtOrDefault(1);
            //        var unit0 = unitPrefab0;// != default ? em.Instantiate(unitPrefab0) : default;
            //        var unit1 = unitPrefab1;// != default ? em.Instantiate(unitPrefab1) : default;
            //        add2UnitsToHolder_(unit0, unit1);
            //    }

            //    void add2UnitsToHolder_(Entity unit0, Entity unit1)
            //    {
            //        var holderBuf = em.GetBuffer<WaponHolder.LinkData>(holder);

            //        holderBuf.Add(new WaponHolder.LinkData
            //        {
            //            FunctionEntity0 = unit0,
            //            FunctionEntity1 = unit1,
            //        });
            //    }
            //}
            //static void createInitUnit2Entities_
            //    (GameObjectConversionSystem gcs, Entity holder, IEnumerable<WaponAuthoring> wapons)
            //{
            //    var em = gcs.DstEntityManager;

            //    var wents = wapons
            //        .Select(x => gcs.GetPrimaryEntity(x))
            //        .ToArray();

            //    var initent = em.CreateEntity(new ComponentType(typeof(WaponTemplate.AddWaponData)));
            //    em.AddComponentData(initent, new WaponTemplate.AddWaponData
            //    {
            //        HolderEntity = holder,
            //        TemplateWaponEntity0 = wents.ElementAtOrDefault(0),
            //        TemplateWaponEntity1 = wents.ElementAtOrDefault(1),
            //        TemplateWaponEntity2 = wents.ElementAtOrDefault(2),
            //        TemplateWaponEntity3 = wents.ElementAtOrDefault(3),
            //    });
            //}
            //static void initAllUnits_
            //    (GameObjectConversionSystem gcs, Entity main, (Entity muzzle, Entity[] units)[] units)
            //{
            //    var em = gcs.DstEntityManager;

            //    var qUnit =
            //        from wapon in units.WithIndex()
            //        from unit in wapon.src.units.WithIndex()
            //        where unit.src != default
            //        select (wapon.src.muzzle, unit.src, wapon.i, unit.i)
            //        ;
            //    foreach (var (muzzle, unit, wid, uid) in qUnit)
            //    {
            //        addFunctionUnitComponents_(muzzle, unit, wid, uid);
            //    }

            //    void addFunctionUnitComponents_
            //        (Entity muzzle, Entity unit, int waponId, int unitId)
            //    {
            //        em.AddComponentData(unit,
            //            new FunctionUnit.OwnerLinkData
            //            {
            //                OwnerMainEntity = main,
            //                MuzzleBodyEntity = muzzle,
            //            }
            //        );
            //        em.AddComponentData(unit,
            //            new FunctionUnitInWapon.TriggerSpecificData
            //            {
            //                Type = (FunctionUnitInWapon.TriggerType)unitId,
            //                WaponCarryId = waponId,
            //            }
            //        );
            //    }
            //}

        }
    }
}