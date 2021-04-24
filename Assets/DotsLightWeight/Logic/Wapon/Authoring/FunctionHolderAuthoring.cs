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
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;

    public class FunctionHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var funits = this.GetComponentsInChildren<IFunctionUnitAuthoring>();

            var top = this.gameObject
                .Ancestors()
                .First(go => go.GetComponent<CharacterModelAuthoring>());

            var posture = top.GetComponentInChildren<PostureAuthoring>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();

            var postureEntity = conversionSystem.GetPrimaryEntity(posture);
            var stateEntity = conversionSystem.GetOrCreateEntity(state);


            initHolder_(conversionSystem, stateEntity);

            return;



            static void initHolder_(GameObjectConversionSystem gcs, Entity holder)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(
                    typeof(FunctionHolder.LinkData)
                );
                em.AddComponents(holder, types);

                em.AddBuffer<FunctionHolder.LinkData>(holder);
            }


            static void initAllUnits_
                (GameObjectConversionSystem gcs, Entity holder, IEnumerable<IFunctionUnitAuthoring> units)
            {
                var em = gcs.DstEntityManager;
                var holderBuf = em.GetBuffer<FunctionHolder.LinkData>(holder);

                foreach (var unit in units)
                {
                    var u = unit as MonoBehaviour;

                    holderBuf.Add(new FunctionHolder.LinkData
                    {
                        FunctionEntity = gcs.GetPrimaryEntity(u),
                    });
                }
            }

            static void initAllUnits_(
                GameObjectConversionSystem gcs,
                Entity holder, Entity muzzle, IEnumerable<IFunctionUnitAuthoring> units)
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
                        typeof(FunctionUnit.OwnerLinkData),
                        typeof(FunctionUnit.StateLinkData),
                        typeof(FunctionUnitInWapon.TriggerSpecificData)
                    );
                    em.AddComponents(unit, types);

                    em.SetComponentData(unit,
                        new FunctionUnit.OwnerLinkData
                        {
                            OwnerMainEntity = main,
                            MuzzleEntity = muzzle,
                        }
                    );
                    em.SetComponentData(unit,
                        new FunctionUnit.StateLinkData
                        {
                            WaponHolderEntity = holder,
                            StateEntity = state,
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