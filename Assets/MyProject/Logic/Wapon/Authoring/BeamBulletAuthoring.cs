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
    public class BeamBulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float RangeDistance;// 瞬時に着弾するので、射出時に決定する。そのため、弾丸には不要。
        public float LifeTime;

        public Color32 BulletColor;
        public float BulletSize;



        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initBullet_(conversionSystem, this.gameObject);

            return;


            void initBullet_( GameObjectConversionSystem gcs_, GameObject bullet_ )
            {
                var em = gcs_.DstEntityManager;


                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);
                var beemTypes = new ComponentTypes
                (
                    typeof(Bullet.LifeTimeData),
                    typeof(Bullet.DistanceData),
                    typeof(Bullet.DirectionData)
                );
                em.AddComponents(bulletEntity, beemTypes);

                em.SetComponentData(bulletEntity,
                    new Bullet.LifeTimeData
                    {
                        LifeTime = this.LifeTime,
                        InvTotalTime = 1.0f / this.LifeTime,
                    }
                );
                em.SetComponentData(bulletEntity,
                    new Particle.AdditionalData
                    {
                        Color = this.BulletColor,
                        Size = this.BulletSize,
                    }
                );
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
