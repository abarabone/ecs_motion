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
    using DotsLite.Targeting;
    using DotsLite.Particle.Aurthoring;

    /// <summary>
    /// 
    /// </summary>
    public class ExplosionAuthoring : ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public ParticleAuthoringBase Emitting;

        public float Radius;
        public int NumMaxHit;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initBullet_(conversionSystem, this.gameObject);

            return;


            void initBullet_(GameObjectConversionSystem gcs_, GameObject bullet_)
            {
                var em = gcs_.DstEntityManager;


                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);
                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(Explosion.HittableTag),
                    typeof(Bullet.PointDamageSpecData),
                    typeof(Explosion.SpecData),
                    typeof(CorpsGroup.TargetWithArmsData),
                    typeof(Translation)
                });
                em.AddComponents(bulletEntity, types);


                em.SetComponentData(bulletEntity,
                    new Explosion.SpecData
                    {
                        HitRadius = this.Radius,
                        NumMaxHitCollecting = this.NumMaxHit,
                    }
                );
            }
        }

    }
}
