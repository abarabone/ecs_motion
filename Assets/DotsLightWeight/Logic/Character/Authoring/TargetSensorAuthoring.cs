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
        }
        public SensorUnit[] Sensors;

        public CollisionFilter Filter;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createSensorHolder_();

            foreach (var s in this.Sensors)
            {
                createSensor_(s);
            }

            return;


            void createSensorHolder_()
            {
                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorHolder.MainHolderTag),
                    typeof(TargetSensorHolder.SensorLinkData),
                    typeof(TargetSensorHolder.PositionData)
                });
                dstManager.AddComponents(entity, types);
            }

            void createSensor_(SensorUnit s)
            {
                var ent = conversionSystem.CreateAdditionalEntity(this);

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensor.MainLinkData),
                    typeof(TargetSensor.FindCollisionData),
                    typeof(TargetSensor.CurrentData)
                });
                dstManager.AddComponents(ent, types);
            }

            void createSingleSensor_()
            {
                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(TargetSensorHolder.MainHolderTag),
                    typeof(TargetSensorHolder.SensorLinkData),
                    typeof(TargetSensorHolder.PositionData)
                });
                dstManager.AddComponents(entity, types);
            }

        }
    }
}
