//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.Linq;
//using UnityEngine;
//using Unity.Linq;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Transforms;
//using Unity.Mathematics;

//namespace DotsLite.Arms.Authoring
//{
//    using DotsLite.Character;
//    using DotsLite.Draw.Authoring;
//    using DotsLite.Common.Extension;
//    using DotsLite.Draw;
//    using DotsLite.CharacterMotion;
//    using DotsLite.Arms;
//    using DotsLite.ParticleSystem;
//    using Unity.Physics.Authoring;
//    using DotsLite.ParticleSystem.Aurthoring;
//    using DotsLite.Targeting;

//    /// <summary>
//    /// 
//    /// </summary>
//    public class EmitterBulletAuthoring : BulletAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
//    {

//        public float RangeDistance;

//        public float BulletSpeed;

//        public float GravityFactor;
//        public float AimFactor;

//        public ParticleAuthoringBase EmittingPrefab;
//        public int NumEmitting;


//        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
//        {
//            var emitting = this.EmittingPrefab;
//            referencedPrefabs.Add(emitting.gameObject);
//        }

//        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
//        {

//            initBullet_(conversionSystem, this.gameObject);

//            return;


//            void initBullet_( GameObjectConversionSystem gcs_, GameObject bullet_ )
//            {
//                var em = gcs_.DstEntityManager;


//                var bulletEntity = gcs_.GetPrimaryEntity(bullet_);
                
//                var _types = new List<ComponentType>
//                {
//                    typeof(Bullet.LinkData),
//                    typeof(Bullet.MoveSpecData),
//                    typeof(Bullet.VelocityData),
//                    typeof(Bullet.DistanceData),
//                    typeof(Bullet.LifeTimeData),
//                    typeof(Bullet.EmitterTag),
//                    typeof(Bullet.EmitData),
//                    typeof(Bullet.InitializeFromEmitterData),
//                    typeof(CorpsGroup.TargetWithArmsData)
//                };
//                if (this.GravityFactor != 0.0f || this.AimFactor != 0.0f || true)
//                {
//                    _types.Add(typeof(Bullet.AccelerationData));
//                }

//                var types = new ComponentTypes(_types.ToArray());
//                em.AddComponents(bulletEntity, types);


//                em.SetComponentData(bulletEntity,
//                    new Bullet.MoveSpecData
//                    {
//                        BulletSpeed = this.BulletSpeed,
//                        RangeDistanceFactor = this.RangeDistance,
//                        GravityFactor = this.GravityFactor,
//                        AimFactor = this.AimFactor,
//                    }
//                );
//                em.SetComponentData(bulletEntity,
//                    new Bullet.DistanceData
//                    {
//                        RestRangeDistance = this.RangeDistance,
//                    }
//                );
//                em.SetComponentData(bulletEntity,
//                    new Bullet.EmitData
//                    {
//                        EmittingPrefab = gcs_.GetPrimaryEntity(this.EmittingPrefab),
//                        numEmitting = this.NumEmitting,
//                    }
//                );
//                //em.SetComponentData(bulletEntity,
//                //    new Bullet.LifeTimeData
//                //    {
//                //        LifeTime = this.LifeTime,
//                //        InvTotalTime = 1.0f / this.LifeTime,
//                //    }
//                //);

//                //// phyllium authoring で
//                //em.SetComponentData(bulletEntity,
//                //    new Particle.AdditionalData
//                //    {
//                //        Color = this.BulletColor,
//                //        Size = this.BulletSize,
//                //    }
//                //);
//            }
//        }

//    }
//}
