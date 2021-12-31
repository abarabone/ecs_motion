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
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Utilities;

    public class FunctionHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) return;


            var funits = this.GetComponentsInChildren<IFunctionUnitAuthoring>();

            var top = this.gameObject
                .AncestorsAndSelf()
                .First(go => go.GetComponent<CharacterModelAuthoring>());

            var posture = top.GetComponentInChildren<PostureAuthoring>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();

            //var postureEntity = conversionSystem.GetPrimaryEntity(posture);
            var stateEntity = conversionSystem.GetOrCreateEntity(state);


            initHolder_(conversionSystem, stateEntity);
            initAllUnits_(conversionSystem, stateEntity, stateEntity, entity, funits);

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


            static void initAllUnits_(GameObjectConversionSystem gcs,
                Entity holderEntity, Entity stateEntity, Entity emitter,
                IEnumerable<IFunctionUnitAuthoring> units)
            {
                var em = gcs.DstEntityManager;

                foreach (var unit in units.Cast<IMuzzleLocalPostion>())
                {
                    var src = unit as MonoBehaviour;

                    var unitEntity = gcs.GetPrimaryEntity(src);
                    var parentEntity = gcs.GetPrimaryEntity(src.transform.parent);

                    initUnit_(gcs, unit, unitEntity, stateEntity, stateEntity, parentEntity);

                    var holderBuf = em.GetBuffer<FunctionHolder.LinkData>(holderEntity);
                    holderBuf.Add(new FunctionHolder.LinkData
                    {
                        FunctionEntity = unitEntity,
                    });
                }
            }

            static void initUnit_(
                GameObjectConversionSystem gcs, IMuzzleLocalPostion unitObject,
                Entity unit, Entity state, Entity holder, Entity parent)
            {
                var em = gcs.DstEntityManager;

                var _types = new List<ComponentType>
                {
                    typeof(Emitter.OwnerLinkData),
                    //typeof(Emitter.BulletMuzzleLinkData),
                    typeof(Emitter.MuzzleTransformData),
                    typeof(FunctionUnitAiming.ParentBoneLinkData),
                    typeof(Rotation),
                    typeof(Translation),
                };
                //if (unitObject.UseEffect)
                //{
                //    _types.Add(typeof(Emitter.EffectMuzzleLinkData));
                //    _types.Add(typeof(Emitter.EffectMuzzlePositionData));
                //}
                var types = new ComponentTypes(_types.ToArray());
                em.AddComponents(unit, types);

                em.SetComponentData(unit,
                    new Emitter.OwnerLinkData
                    {
                        StateEntity = state,
                    }
                );
                em.SetComponentData(unit,
                    new Emitter.BulletMuzzleLinkData
                    {
                        MuzzleEntity = unit,
                    }
                );
                em.SetComponentData(unit,
                    new Emitter.MuzzleTransformData
                    {
                        MuzzlePositionLocal = unitObject.Local.As_float4(),
                        ParentEntity = parent,
                    }
                );
                //if (unitObject.UseEffect)
                //{
                //    em.SetComponentData(unit,
                //        new Emitter.EffectMuzzleLinkData
                //        {
                //            MuzzleEntity = unit,
                //        }
                //    );
                //    em.SetComponentData(unit,
                //        new Emitter.EffectMuzzlePositionData
                //        {
                //            MuzzlePositionLocal = unitObject.Local.As_float4(),
                //        }
                //    );
                //}
                //em.SetComponentData(unit,
                //    new FunctionUnit.MuzzleLinkData
                //    {
                //        EmitterEntity = unit,
                //        MuzzleEntity = unit,
                //    }
                //);
                em.SetComponentData(unit,
                    new FunctionUnitAiming.ParentBoneLinkData
                    {
                        ParentEntity = parent,
                    }
                );
            }

        }
    }
}
