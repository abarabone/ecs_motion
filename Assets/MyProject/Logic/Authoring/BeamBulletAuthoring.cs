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

namespace Abarabone.Model.Authoring
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
                    typeof(Bullet.BeamTag),
                    typeof(Bullet.BulletData)
                );
                em.AddComponents(bulletEntity, beemTypes);

                em.SetComponentData(bulletEntity,
                    new Bullet.BulletData
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
            }
        }

    }
}
