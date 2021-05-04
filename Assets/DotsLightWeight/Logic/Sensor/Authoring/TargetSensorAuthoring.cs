using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Linq;

namespace DotsLite.Character.Authoring
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Model.Authoring;
    using DotsLite.Common.Extension;

    /// <summary>
    /// メインエンティティに付けておく
    /// </summary>
    public class TargetSensorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        [System.Serializable]
        public class SensorUnit
        {
            public float distance;
            public float interval;
        }
        [SerializeField]
        public SensorUnit[] Sensors;

        [System.Serializable]
        public struct Filter
        {
            public PhysicsCategoryTags BelongsTo;
            public PhysicsCategoryTags CollidesWith;
        }
        [SerializeField]
        public Filter Collision;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.FindParent<ModelGroupAuthoring.ModelAuthoringBase>();
            var state = top.GetComponentInChildren<ActionStateAuthoring>();
            var posture = top.GetComponentInChildren<PostureAuthoring>();
            var holder = this;


            var holderEntity = createSensorHolder_(conversionSystem, holder);
            
            var stateEntity = conversionSystem.GetOrCreateEntity(state);
            dstManager.AddComponentData(stateEntity,
                new TargetSensorHolderLink.HolderLinkData
                {
                    HolderEntity = holderEntity,
                }
            );

            var postureEntity = conversionSystem.GetPrimaryEntity(posture);
            dstManager.AddComponentData(postureEntity,
                new TargetSensorHolderLink.HolderLinkData
                {
                    HolderEntity = holderEntity,
                }
            );

            foreach (var src in this.Sensors)
            {
                var sent = createSensor_(conversionSystem, src, holder, posture);
                addToHolder_(conversionSystem, src, sent, holderEntity);
            }

            return;


            static Entity createSensorHolder_(GameObjectConversionSystem gcs, TargetSensorAuthoring holder)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.CreateAdditionalEntity(holder);
                em.SetName_(ent, "ant sensor holder");

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorResponse.SensorMainTag),
                    typeof(TargetSensorHolder.SensorLinkData),
                    typeof(TargetSensorHolder.SensorNextTimeData),
                    typeof(TargetSensorResponse.PositionData)
                });
                em.AddComponents(ent, types);

                return ent;
            }

            static Entity createSensor_(
                GameObjectConversionSystem gcs, SensorUnit src, TargetSensorAuthoring holder, PostureAuthoring posture, Corps corps)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.CreateAdditionalEntity(holder);
                em.SetName_(ent, $"ant sensor {src.ToString().Split('+').Last()}");

                var types = new ComponentTypes(new ComponentType[]
                {
                    //typeof(TargetSensor.WakeupFindTag),
                    typeof(TargetSensor.LinkTargetMainData),
                    typeof(TargetSensor.CollisionData),
                    typeof(TargetSensor.GroupFilterData),
                    typeof(TargetSensorResponse.PositionData)
                });
                em.AddComponents(ent, types);

                //dstManager.SetComponentData(ent, new TargetSensor.LinkTargetMainData
                //{
                //    TargetMainEntity = ent,

                //});

                em.SetComponentData(ent, new TargetSensor.CollisionData
                {
                    PostureEntity = gcs.GetPrimaryEntity(posture),
                    Distance = src.distance,
                    Corps = corps,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = holder.Collision.BelongsTo.Value,
                        CollidesWith = holder.Collision.CollidesWith.Value,
                    },
                });

                //dstManager.SetComponentData(ent, new TargetSensor.GroupFilterData
                //{
                //    CollidesWith = this.Group.Value,
                //});

                return ent;
            }
            static void addToHolder_(GameObjectConversionSystem gcs, SensorUnit src, Entity sent, Entity holderEntity)
            {
                var em = gcs.DstEntityManager;

                var linkbuf = em.GetBuffer<TargetSensorHolder.SensorLinkData>(holderEntity);
                var nextbuf = em.GetBuffer<TargetSensorHolder.SensorNextTimeData>(holderEntity);
                linkbuf.Add(new TargetSensorHolder.SensorLinkData
                {
                    SensorEntity = sent,
                    Interval = src.interval,
                });
                nextbuf.Add(new TargetSensorHolder.SensorNextTimeData
                {
                    NextTime = src.interval > 0 ? 0.0f : float.MaxValue,
                });
            }

            //Entity createSingleSensor_()
            //{
            //    var ent = conversionSystem.CreateAdditionalEntity(this);

            //    var types = new ComponentTypes(new ComponentType[]
            //    {
            //        typeof(TargetSensorResponse.SensorMainTag),
            //        typeof(TargetSensor.WakeupFindTag),
            //        typeof(TargetSensor.LinkTargetMainData),
            //        typeof(TargetSensor.CollisionData),
            //        typeof(TargetSensor.GroupFilterData),
            //        typeof(TargetSensorResponse.PositionData)
            //    });
            //    dstManager.AddComponents(entity, types);

            //    return ent;
            //}

        }
    }
}
