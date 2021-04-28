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

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Abarabone.Model;

    /// <summary>
    /// 
    /// </summary>
    public class ShotUnitAuthoring : MonoBehaviour, IFunctionUnitAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public int Id;

        //public IBulletAuthoring aaa;
        public ShotBulletAuthoring BulletPrefab;

        //public GameObject MuzzleObject;
        public float3 MuzzleLocalPosition;

        public int NumEmitMultiple;
        public float EmittingInterval;
        public float DirectionAccuracyDegree;
        public float RangeDistanceFactor;// 弾丸のと掛け合わせて射程とする


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.BulletPrefab.gameObject);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initEmitter_(conversionSystem, entity);

            return;


            void initEmitter_(GameObjectConversionSystem gcs_, Entity emitter_)
            {

                var em = gcs_.DstEntityManager;

                var bulletPrefab = gcs_.GetPrimaryEntity(this.BulletPrefab.gameObject);

                var ent = emitter_;


                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(FunctionUnit.BulletEmittingData),
                    typeof(FunctionUnit.EmittingStateData),
                    typeof(FunctionUnit.TriggerData),
                    typeof(FunctionUnit.ActivateData)
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent,
                    new FunctionUnit.BulletEmittingData
                    {
                        BulletPrefab = bulletPrefab,
                        EffectPrefab = Entity.Null,// 
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
