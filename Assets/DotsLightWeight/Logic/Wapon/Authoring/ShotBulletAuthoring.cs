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
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Abarabone.Particle;
    using Unity.Physics.Authoring;

    /// <summary>
    /// 
    /// </summary>
    public class ShotBulletAuthoring : MonoBehaviour, IBulletAuthoring, IConvertGameObjectToEntity
    {

        public float RangeDistance;
        public float LifeTime;

        public Color32 BulletColor;
        public float BulletSize;
        public float BulletSpeed;

        public float GravityFactor;
        public float AimFactor;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initBullet_(conversionSystem, this.gameObject);

            return;


            void initBullet_( GameObjectConversionSystem gcs_, GameObject bullet_ )
            {
                var em = gcs_.DstEntityManager;


                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);
                var types = (this.GravityFactor != 0.0f || this.AimFactor != 0.0f || true)//
                    ? new ComponentTypes
                    (new ComponentType[] {
                        typeof(Bullet.SpecData),
                        typeof(Bullet.VelocityData),
                        typeof(Bullet.AccelerationData),
                        typeof(Bullet.DistanceData),
                        typeof(Bullet.LifeTimeData),
                        typeof(Particle.AdditionalData)
                    })
                    : new ComponentTypes
                    (
                        typeof(Bullet.SpecData),
                        typeof(Bullet.VelocityData),
                        typeof(Bullet.DistanceData),
                        typeof(Bullet.LifeTimeData),
                        typeof(Particle.AdditionalData)
                    );
                em.AddComponents(bulletEntity, types);

                em.SetComponentData(bulletEntity,
                    new Bullet.SpecData
                    {
                        BulletSpeed = this.BulletSpeed,
                        RangeDistanceFactor = this.RangeDistance,
                        GravityFactor = this.GravityFactor,
                        AimFactor = this.AimFactor,
                    }
                );
                em.SetComponentData(bulletEntity,
                    new Bullet.DistanceData
                    {
                        RestRangeDistance = this.RangeDistance,
                    }
                );
                em.SetComponentData(bulletEntity,
                    new Bullet.LifeTimeData
                    {
                        LifeTime = this.LifeTime,
                        InvTotalTime = 1.0f / this.LifeTime,
                    }
                );

                // phyllium authoring で
                em.SetComponentData(bulletEntity,
                    new Particle.AdditionalData
                    {
                        Color = this.BulletColor,
                        Size = this.BulletSize,
                    }
                );
            }
        }

    }
}
