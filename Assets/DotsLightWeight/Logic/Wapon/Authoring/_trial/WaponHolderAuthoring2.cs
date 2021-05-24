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

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;

    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class WaponHolderAuthoring2 : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        public GameObject Muzzle;

        public bool UseCameraSite;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject
                .AncestorsAndSelf()
                .First(go => go.GetComponent<CharacterModelAuthoring>());

            var wapons = this.GetComponentsInChildren<WaponAuthoring>();
            var posture = top.GetComponentInChildren<PostureAuthoring>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();

            var postureEntity = conversionSystem.GetPrimaryEntity(posture);
            var stateEntity = conversionSystem.GetOrCreateEntity(state);

            var holderEntity = entity;
            var muzzleEntity = conversionSystem.GetPrimaryEntity(this.Muzzle);
            var emitterEntity = this.UseCameraSite ? conversionSystem.GetPrimaryEntity(Camera.main) : muzzleEntity;

            var unitsEntities = listupMuzzleAndUnitsEntities_(conversionSystem, wapons);
            initHolder_(conversionSystem, holderEntity, postureEntity, muzzleEntity, stateEntity, wapons.Length, startSelectedId: 0);
            initAllWapons_(conversionSystem, holderEntity, unitsEntities);
            initAllUnits_(conversionSystem, holderEntity, postureEntity, stateEntity, muzzleEntity, emitterEntity, unitsEntities);
            //createInitUnit2Entities_(conversionSystem, holderEntity, this.Wapons);

            return;


            /// 
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


            static void initHolder_(
                GameObjectConversionSystem gcs,
                Entity holder, Entity main, Entity muzzle, Entity state,
                int waponsLength, int startSelectedId)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(
                    typeof(WaponHolder.SelectorData),
                    typeof(WaponHolder.UnitLinkData),
                    typeof(WaponHolder.OwnerLinkData),
                    typeof(WaponHolder.StateLinkData)
                );
                em.AddComponents(holder, types);

                em.SetComponentData(holder,
                    new WaponHolder.SelectorData
                    {
                        CurrentWaponIndex = startSelectedId,
                        Length = waponsLength,
                    }
                );
                em.AddBuffer<WaponHolder.UnitLinkData>(holder);

                em.SetComponentData(holder,
                    new WaponHolder.OwnerLinkData
                    {
                        //OwnerEntity = main,
                        MuzzleEntity = muzzle,
                    }
                );
                em.SetComponentData(holder,
                    new WaponHolder.StateLinkData
                    {
                        StateEntity = state,
                    }
                );
            }


            static void initAllWapons_
                (GameObjectConversionSystem gcs, Entity holder, IEnumerable<IEnumerable<Entity>> unitss)
            {
                var em = gcs.DstEntityManager;
                var holderBuf = em.GetBuffer<WaponHolder.UnitLinkData>(holder);

                foreach (var w in unitss)
                {
                    var unit0 = w.ElementAtOrDefault(0);
                    var unit1 = w.ElementAtOrDefault(1);
                    add2UnitsToHolder_(unit0, unit1);
                }

                void add2UnitsToHolder_(Entity unit0, Entity unit1)
                {
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


            static void initAllUnits_(
                GameObjectConversionSystem gcs,
                Entity holder, Entity main, Entity state, Entity muzzle, Entity emitter,
                IEnumerable<IEnumerable<Entity>> unitss)
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
                    var types = new ComponentTypes(
                        typeof(Emitter.BulletMuzzleLinkData),
                        typeof(Emitter.EffectMuzzleLinkData),
                        typeof(Emitter.OwnerLinkData),
                        typeof(FunctionUnit.holderLinkData),
                        typeof(FunctionUnitInWapon.TriggerSpecificData)
                    );
                    em.AddComponents(unit, types);

                    em.SetComponentData(unit,
                        new Emitter.BulletMuzzleLinkData
                        {
                            MuzzleEntity = muzzle,
                        }
                    );
                    em.SetComponentData(unit,
                        new Emitter.EffectMuzzleLinkData
                        {
                            MuzzleEntity = muzzle,
                        }
                    );
                    em.SetComponentData(unit,
                        new Emitter.OwnerLinkData
                        {
                            StateEntity = state,
                        }
                    );
                    em.SetComponentData(unit,
                        new FunctionUnit.holderLinkData
                        {
                            WaponHolderEntity = holder,
                        }
                    );
                    em.SetComponentData(unit,
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