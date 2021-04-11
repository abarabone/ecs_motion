using System.Collections;
using System.Collections.Generic;
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

    public class TargetSensorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public class SensorUnit
        {
            public float distance;
            public float interval;
        }
        public SensorUnit[] Sensors;

        public CollisionFilter Filter;
        public CollisionFilter Group;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var holder = createSensorHolder_();
            var buf = dstManager.GetBuffer<TargetSensorHolder.SensorsLinkData>(holder);

            foreach (var s in this.Sensors)
            {
                var sent = createSensor_(s);
                addToHolder_(s, sent);
            }

            return;


            Entity createSensorHolder_()
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorResponse.SensorMainTag),
                    typeof(TargetSensorHolder.SensorsLinkData),
                    typeof(TargetSensorResponse.PositionData)
                });
                dstManager.AddComponents(ent, types);

                return ent;
            }

            Entity createSensor_(SensorUnit src)
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensor.PollingTag),
                    typeof(TargetSensor.LinkMainData),
                    typeof(TargetSensor.CollisionData),
                    typeof(TargetSensor.GroupFilterData),
                    typeof(TargetSensorResponse.PositionData)
                });
                dstManager.AddComponents(ent, types);

                dstManager.SetComponentData(ent, new TargetSensor.LinkMainData
                {
                    MainEntity = ent,

                });

                dstManager.SetComponentData(ent, new TargetSensor.CollisionData
                {
                    Distance = src.distance,
                    Filter = this.Filter,
                });

                dstManager.SetComponentData(ent, new TargetSensor.GroupFilterData
                {
                    CollidesWith = this.Group.CollidesWith,
                });

                return ent;
            }
            void addToHolder_(SensorUnit src, Entity ent)
            {
                buf.Add(new TargetSensorHolder.SensorsLinkData
                {
                    SensorEntity = ent,
                    Interval = src.interval,
                    LastTime = src.interval > 0 ? 0.0f : float.MaxValue,
                });
            }

            Entity createSingleSensor_()
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorResponse.SensorMainTag),
                    typeof(TargetSensor.PollingTag),
                    typeof(TargetSensor.LinkMainData),
                    typeof(TargetSensor.CollisionData),
                    typeof(TargetSensor.GroupFilterData),
                    typeof(TargetSensorResponse.PositionData)
                });
                dstManager.AddComponents(entity, types);

                return ent;
            }

        }
    }
}
