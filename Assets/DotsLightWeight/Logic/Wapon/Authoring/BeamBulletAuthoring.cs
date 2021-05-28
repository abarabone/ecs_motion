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
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;
    using DotsLite.Particle;
    using Unity.Physics.Authoring;

    /// <summary>
    /// 
    /// </summary>
    public class BeamBulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float RangeDistance;// 瞬時に着弾するので、射出時に決定する。そのため、弾丸には不要。
        //public float LifeTime;

        //public Color32 BulletColor;
        //public float BulletSize;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initBullet_(conversionSystem, this.gameObject);

            return;


            void initBullet_( GameObjectConversionSystem gcs_, GameObject bullet_ )
            {
                var em = gcs_.DstEntityManager;


                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);
                var types = new ComponentTypes
                (
                    typeof(Bullet.LifeTimeData),
                    typeof(Bullet.DistanceData),
                    typeof(Bullet.VelocityData),
                    typeof(Bullet.LinkData),
                    BulletType.Beam.ToComponentType()
                );
                em.AddComponents(bulletEntity, types);

                //em.SetComponentData(bulletEntity,
                //    new Bullet.LifeTimeData
                //    {
                //        LifeTime = this.LifeTime,
                //        InvTotalTime = 1.0f / this.LifeTime,
                //    }
                //);
                //em.SetComponentData(bulletEntity,
                //    new Particle.AdditionalData
                //    {
                //        Color = this.BulletColor,
                //        Size = this.BulletSize,
                //    }
                //);
                em.SetComponentData(bulletEntity,
                    new Bullet.DistanceData
                    {
                        RestRangeDistance = this.RangeDistance,
                    }
                );
            }
        }

    }
}
