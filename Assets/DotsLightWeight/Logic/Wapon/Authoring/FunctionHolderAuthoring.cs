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


        public bool UseCameraSite;



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
            var emitterEntity = this.UseCameraSite ? conversionSystem.GetPrimaryEntity(Camera.main) : Entity.Null;


            initHolder_(conversionSystem, stateEntity);
            initAllUnits_(conversionSystem, stateEntity, stateEntity, emitterEntity, funits);

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
                    var muzzleEntity = gcs.GetPrimaryEntity(src.transform.parent);
                    var emitterEntity = emitter != Entity.Null ? emitter : muzzleEntity;

                    initUnit_(gcs, unitEntity, stateEntity, muzzleEntity, emitterEntity);

                    var holderBuf = em.GetBuffer<FunctionHolder.LinkData>(holderEntity);

                    holderBuf.Add(new FunctionHolder.LinkData
                    {
                        FunctionEntity = unitEntity,
                    });
                }
            }

            static void initUnit_(GameObjectConversionSystem gcs, Entity unit, Entity state, Entity muzzle, Entity emitter)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(
                    typeof(FunctionUnit.StateLinkData),
                    typeof(FunctionUnit.MuzzleLinkData)
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
                        EmitterEntity = emitter,
                        MuzzleEntity = muzzle,
                    }
                );
            }

        }
    }
}