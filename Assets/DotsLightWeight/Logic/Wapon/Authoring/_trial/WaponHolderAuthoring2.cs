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
    using DotsLite.Utilities;

    public interface IMuzzleLocalPostion
    {
        float3 Local { get; }
    }

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
            var muzzle = this.Muzzle.gameObject;
            var emitter = this.UseCameraSite ? Camera.main.gameObject : muzzle;
            var muzzleEntity = conversionSystem.GetPrimaryEntity(muzzle);
            var emitterEntity = conversionSystem.GetPrimaryEntity(emitter);

            var units = listupMuzzleAndUnitsEntities_(conversionSystem, wapons);
            initHolder_(conversionSystem,
                holderEntity, postureEntity,
                muzzleEntity, stateEntity, wapons.Length, startSelectedId: 0);
            initAllWapons_(conversionSystem,
                holderEntity, units);
            initAllUnits_(conversionSystem,
                holderEntity, postureEntity, stateEntity,
                muzzleEntity, emitterEntity, units, this.UseCameraSite ? Camera.main : null);

            return;


            /// 
            static IFunctionUnitAuthoring[][] listupMuzzleAndUnitsEntities_
                (GameObjectConversionSystem gcs, IEnumerable<WaponAuthoring> wapons)
            {

                var qWaponEntities =
                    from w in wapons

                    let qUnitEntity =
                        from u in w.GetComponentsInChildren<IFunctionUnitAuthoring>()
                        let ubehviour = u as MonoBehaviour//)?.gameObject
                        where ubehviour != null
                        select u//gcs.GetPrimaryEntity(ubehviour)

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
                (GameObjectConversionSystem gcs, Entity holder, IEnumerable<IEnumerable<IFunctionUnitAuthoring>> unitss)
            {
                var em = gcs.DstEntityManager;
                var holderBuf = em.GetBuffer<WaponHolder.UnitLinkData>(holder);

                foreach (var w in unitss)
                {
                    var unit0 = gcs.GetPrimaryEntity(w.ElementAtOrDefault(0) as MonoBehaviour);
                    var unit1 = gcs.GetPrimaryEntity(w.ElementAtOrDefault(1) as MonoBehaviour);
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


            static void initAllUnits_(
                GameObjectConversionSystem gcs,
                Entity holder, Entity main, Entity state, Entity muzzle, Entity emitter,
                IEnumerable<IEnumerable<IFunctionUnitAuthoring>> unitss, Camera cam)
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
                    (Entity muzzle, IFunctionUnitAuthoring unit_, int waponId, int unitId)
                {
                    var unit = gcs.GetPrimaryEntity(unit_ as MonoBehaviour);

                    var types = new ComponentTypes(new ComponentType[] {
                        typeof(Emitter.BulletMuzzleLinkData),
                        typeof(Emitter.BulletMuzzlePositionData),
                        typeof(Emitter.EffectMuzzleLinkData),
                        typeof(Emitter.EffectMuzzlePositionData),
                        typeof(Emitter.OwnerLinkData),
                        typeof(FunctionUnit.holderLinkData),
                        typeof(FunctionUnitInWapon.TriggerSpecificData),
                    });
                    em.AddComponents(unit, types);

                    em.SetComponentData(unit,
                        new Emitter.BulletMuzzleLinkData
                        {
                            MuzzleEntity = emitter,
                        }
                    );
                    em.SetComponentData(unit,
                        new Emitter.BulletMuzzlePositionData
                        {
                            MuzzlePositionLocal = cam != null
                                ? cam.GetComponent<IMuzzleLocalPostion>().Local.As_float4()
                                : (unit_ as IMuzzleLocalPostion).Local.As_float4(),
                        }
                    );
                    em.SetComponentData(unit,
                        new Emitter.EffectMuzzleLinkData
                        {
                            MuzzleEntity = muzzle,
                        }
                    );
                    em.SetComponentData(unit,
                        new Emitter.EffectMuzzlePositionData
                        {
                            MuzzlePositionLocal = (unit_ as IMuzzleLocalPostion).Local.As_float4(),
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