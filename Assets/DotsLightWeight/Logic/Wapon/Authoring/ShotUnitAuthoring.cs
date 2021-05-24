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
using Unity.Physics.Authoring;

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Model;
    using DotsLite.Particle.Aurthoring;

    /// <summary>
    /// 
    /// </summary>
    public class ShotUnitAuthoring : MonoBehaviour, IFunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public int Id;

        //public IBulletAuthoring aaa;
        public ShotBulletAuthoring BulletPrefab;
        public ParticleAuthoringBase MuzzleEffectPrefab;

        //public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;

        public int NumEmitMultiple;
        public float EmittingInterval;
        public float DirectionAccuracyDegree;
        public float RangeDistanceFactor;// 弾丸のと掛け合わせて射程とする


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var bullet = this.BulletPrefab;
            referencedPrefabs.Add(bullet.gameObject);

            if (this.MuzzleEffectPrefab != null)
            {
                var effect = this.MuzzleEffectPrefab;// as MonoBehaviour;
                referencedPrefabs.Add(effect.gameObject);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEmitter_(conversionSystem, entity);

            return;


            void initEmitter_(GameObjectConversionSystem gcs, Entity emitter)
            {

                var em = gcs.DstEntityManager;

                var bulletPrefab = gcs.GetPrimaryEntity(this.BulletPrefab);
                var effectPrefab = this.MuzzleEffectPrefab != null
                    ? gcs.GetPrimaryEntity(this.MuzzleEffectPrefab)
                    : Entity.Null;

                var ent = emitter;


                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(FunctionUnit.BulletEmittingData),
                    typeof(FunctionUnit.EmittingStateData),
                    typeof(FunctionUnit.TriggerData),
                    //typeof(FunctionUnit.ActivateData),
                    typeof(Targeting.CorpsGroup.TargetWithArmsData)
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new FunctionUnit.BulletEmittingData
                    {
                        BulletPrefab = bulletPrefab,
                        EffectPrefab = effectPrefab,
                        MuzzlePositionLocal = this.MuzzleLocalPosition,
                        EmittingInterval = this.EmittingInterval,
                        NumEmitMultiple = this.NumEmitMultiple,
                        AccuracyRad = math.radians(this.DirectionAccuracyDegree),
                        RangeDistanceFactor = this.RangeDistanceFactor,
                    }
                );
                em.SetComponentData(ent,
                    new FunctionUnit.EmittingStateData
                    {
                        NextEmitableTime = this.EmittingInterval,
                    }
                );
                //em.SetComponentData(ent,
                //    new FunctionUnit.TriggerData
                //    {
                //        IsTriggered = true,
                //    }
                //);
            }


        }

    }
}
