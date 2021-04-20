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
    public class WaponHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        //public WaponAuthoring[] Wapons;
        public GameObject Muzzle;


        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    var qWapon = this.Wapons
        //        //.Select(x => x.wapon)
        //        //.Cast<MonoBehaviour>()
        //        .Select(x => x.gameObject);

        //    referencedPrefabs.AddRange(qWapon);
        //}



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject
                .Ancestors()
                .First(go => go.GetComponent<CharacterModelAuthoring>());
            var main = top.transform
                .GetChild(0)
                .gameObject;

            var wapons = this.GetComponentsInChildren<WaponAuthoring>();
            var state = this.gameObject.Ancestors()
                .Where(x => x.GetComponent<ActionStateAuthoring>() != null)
                .First()
                .GetComponent<ActionStateAuthoring>();
                //this.GetComponentInParent<ActionStateAuthoring>();// wapon holder を独立させるべきか…？
            dstManager.DestroyEntity(entity);//
            Debug.Log(state.name);

            var holderEntity = conversionSystem.GetEntityDictionary()[state];//
            //var holderEntity = entity;
            var mainEntity = conversionSystem.GetPrimaryEntity(main);
            var muzzleEntity = conversionSystem.GetPrimaryEntity(this.Muzzle);

            var unitsEntities = listupMuzzleAndUnitsEntities_(conversionSystem, wapons);
            initHolder_(conversionSystem, holderEntity, mainEntity, muzzleEntity, wapons.Length, startSelectedId: 0);
            initAllWapons_(conversionSystem, holderEntity, unitsEntities);
            initAllUnits_(conversionSystem, holderEntity, mainEntity, muzzleEntity, unitsEntities);
            //createInitUnit2Entities_(conversionSystem, holderEntity, this.Wapons);

            return;


            static Entity[][] listupMuzzleAndUnitsEntities_
                (GameObjectConversionSystem gcs, IEnumerable<WaponAuthoring> wapons)
            {

                var qWaponEntities =
                    from w in wapons

                    let qUnitEntity =
                        from u in w.GetComponentsInChildren<IFunctionUnitAuthoring>()
                        let ubehviour = u as MonoBehaviour//)?.gameObject
                        where ubehviour != null
                        select gcs.GetPrimaryEntity(ubehviour)

                    select qUnitEntity.ToArray()
                    ;

                return qWaponEntities.ToArray();
            }

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
            static void initAllWapons_
                (GameObjectConversionSystem gcs, Entity holder, IEnumerable<IEnumerable<Entity>> unitss)
            {
                var em = gcs.DstEntityManager;

                foreach (var w in unitss)
                {
                    var unit0 = w.ElementAtOrDefault(0);
                    var unit1 = w.ElementAtOrDefault(1);
                    add2UnitsToHolder_(unit0, unit1);
                }

                void add2UnitsToHolder_(Entity unit0, Entity unit1)
                {
                    var holderBuf = em.GetBuffer<WaponHolder.UnitLinkData>(holder);

                    holderBuf.Add(new WaponHolder.UnitLinkData
                    {
                        FunctionEntity0 = unit0,
                        FunctionEntity1 = unit1,
                    });
                }
            }
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
            static void initAllUnits_
                (GameObjectConversionSystem gcs, Entity holder, Entity main, Entity muzzle, IEnumerable<IEnumerable<Entity>> unitss)
            {
                var em = gcs.DstEntityManager;

                var qUnit =
                    from w in unitss.WithIndex()
                    from unit in w.src.WithIndex()
                    where unit.src != default
                    select (unit.src, w.i, unit.i)
                    ;
                foreach (var (unit, wid, uid) in qUnit)
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
                            WaponHolderEntity = holder,
                            MuzzleEntity = muzzle,
                        }
                    );
                    em.AddComponentData(unit,
                        new FunctionUnitInWapon.TriggerSpecificData
                        {
                            Type = (FunctionUnitInWapon.TriggerType)unitId,
                            WaponCarryId = waponId,
                        }
                    );
                }
            }

        }
    }
}