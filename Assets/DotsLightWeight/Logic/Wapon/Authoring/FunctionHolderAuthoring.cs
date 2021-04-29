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

    public class FunctionHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

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

                foreach (var unit in units)
                {
                    var src = unit as MonoBehaviour;

                    var unitEntity = gcs.GetPrimaryEntity(src);
                    var parentEntity = gcs.GetPrimaryEntity(src.transform.parent);

                    initUnit_(gcs, unitEntity, stateEntity, stateEntity, parentEntity);

                    var holderBuf = em.GetBuffer<FunctionHolder.LinkData>(holderEntity);
                    holderBuf.Add(new FunctionHolder.LinkData
                    {
                        FunctionEntity = unitEntity,
                    });
                }
            }

            static void initUnit_(GameObjectConversionSystem gcs, Entity unit, Entity state, Entity holder, Entity parent)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(
                    typeof(FunctionUnit.StateLinkData),
                    typeof(FunctionUnit.MuzzleLinkData),
                    typeof(FunctionUnitAiming.ParentBoneLinkData),
                    typeof(Rotation),
                    typeof(Translation)
                );
                em.AddComponents(unit, types);

                em.SetComponentData(unit,
                    new FunctionUnit.StateLinkData
                    {
                        StateEntity = state,
                    }
                );
                em.SetComponentData(unit,
                    new FunctionUnit.MuzzleLinkData
                    {
                        EmitterEntity = unit,
                        MuzzleEntity = unit,
                    }
                );
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