using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;

namespace Abarabone.Character.Authoring
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Targeting;

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

        [SerializeField]
        public CollisionFilter Filter;
        //[SerializeField]
        //public CollisionFilter Group;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var holder = createSensorHolder_();
            dstManager.AddComponentData(entity, new TargetSensorHolderLink.HolderLinkData
            {
                HolderEntity = holder,
            });

            foreach (var s in this.Sensors)
            {
                var sent = createSensor_(s);
                addToHolder_(s, sent);
            }

            return;


            Entity createSensorHolder_()
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);
                dstManager.SetName_(ent, "ant sensor holder");

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorResponse.SensorMainTag),
                    typeof(TargetSensorHolder.SensorLinkData),
                    typeof(TargetSensorHolder.SensorNextTimeData),
                    typeof(TargetSensorResponse.PositionData)
                });
                dstManager.AddComponents(ent, types);

                return ent;
            }

            Entity createSensor_(SensorUnit src)
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);
                dstManager.SetName_(ent, $"ant sensor {src.ToString().Split('+').Last()}");

                var types = new ComponentTypes(new ComponentType[]
                {
                    //typeof(TargetSensor.WakeupFindTag),
                    typeof(TargetSensor.LinkTargetMainData),
                    typeof(TargetSensor.CollisionData),
                    typeof(TargetSensor.GroupFilterData),
                    typeof(TargetSensorResponse.PositionData),
                    typeof(Disabled)
                });
                dstManager.AddComponents(ent, types);

                //dstManager.SetComponentData(ent, new TargetSensor.LinkTargetMainData
                //{
                //    TargetMainEntity = ent,

                //});

                dstManager.SetComponentData(ent, new TargetSensor.CollisionData
                {
                    PostureEntity = entity,
                    Distance = src.distance,
                    Filter = this.Filter,
                });

                dstManager.SetComponentData(ent, new TargetSensor.GroupFilterData
                {
                    //CollidesWith = this.Group.CollidesWith,
                });

                return ent;
            }
            void addToHolder_(SensorUnit src, Entity ent)
            {
                var linkbuf = dstManager.GetBuffer<TargetSensorHolder.SensorLinkData>(holder);
                var nextbuf = dstManager.GetBuffer<TargetSensorHolder.SensorNextTimeData>(holder);
                linkbuf.Add(new TargetSensorHolder.SensorLinkData
                {
                    SensorEntity = ent,
                    Interval = src.interval,
                });
                nextbuf.Add(new TargetSensorHolder.SensorNextTimeData
                {
                    nextTime = src.interval > 0 ? 0.0f : float.MaxValue,
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
