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
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Particle;
    using DotsLite.Particle.Aurthoring;
    using DotsLite.Targeting;

    /// <summary>
    /// 
    /// </summary>
    public class ShotBulletAuthoring : BulletAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public float RangeDistance;

        public float BulletSpeed;

        public float GravityFactor;
        public float AimFactor;
        public float DamagePoint;

        public BulletHitType HitType;

        public ParticleAuthoringBase EmittingPrefab;
        public int NumEmitting;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (this.EmittingPrefab != null)
            {
                var emitting = this.EmittingPrefab;
                referencedPrefabs.Add(emitting.gameObject);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            addBulletComponents_(conversionSystem, this.gameObject);

            return;


            void addBulletComponents_( GameObjectConversionSystem gcs_, GameObject bullet_ )
            {
                var em = gcs_.DstEntityManager;


                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);

                var _types = new List<ComponentType>
                {
                    typeof(Bullet.LinkData),
                    typeof(Bullet.MoveSpecData),
                    typeof(Bullet.VelocityData),
                    typeof(Bullet.DistanceData),
                    typeof(Bullet.LifeTimeData),
                    typeof(Bullet.InitializeFromEmitterData),
                    typeof(CorpsGroup.TargetWithArmsData)
                };
                if (this.GravityFactor != 0.0f || this.AimFactor != 0.0f || true)
                {
                    _types.Add(typeof(Bullet.AccelerationData));
                }
                if (this.HitType == BulletHitType.Ray)
                {
                    _types.Add(typeof(Bullet.RayTag));
                    _types.Add(typeof(Psyllium.TranslationTailData));
                }
                if (this.HitType == BulletHitType.Sphere)
                {
                    _types.Add(typeof(Bullet.SphereTag));
                }
                if (DamagePoint != 0)
                {
                    _types.Add(typeof(Bullet.PointDamageSpecData));
                }
                if (EmittingPrefab != null)
                {
                    _types.Add(typeof(Bullet.EmitterTag));
                    _types.Add(typeof(Bullet.EmitData));
                }

                var types = new ComponentTypes(_types.ToArray());
                em.AddComponents(bulletEntity, types);


                em.SetComponentData(bulletEntity,
                    new Bullet.MoveSpecData
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

                if (DamagePoint != 0)
                {
                    em.SetComponentData(bulletEntity,
                        new Bullet.PointDamageSpecData
                        {
                            Damage = this.DamagePoint,
                        }
                    );
                }

                if (EmittingPrefab != null)
                {
                    em.SetComponentData(bulletEntity,
                        new Bullet.EmitData
                        {
                            EmittingPrefab = gcs_.GetPrimaryEntity(this.EmittingPrefab),
                            numEmitting = this.NumEmitting,
                        }
                    );
                }
            }
        }

    }
}
