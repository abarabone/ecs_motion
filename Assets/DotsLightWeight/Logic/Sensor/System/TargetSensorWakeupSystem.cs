using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

namespace Abarabone.Character
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Targeting;
    using Abarabone.Dependency;


    // ホルダーからセンサーを起動する
    // センサーはインターバルごとに WakeupFindTag をつけて起動する
    // 

    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class TargetSensorWakeSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }


        protected override void OnUpdate()
        {
            var cmdScope = this.cmddep.WithDependencyScope();


            var sensorLinks = this.GetComponentDataFromEntity<TargetSensor.LinkTargetMainData>(isReadOnly: true);
            var sensorPoss = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);

            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var currentTime = this.Time.ElapsedTime;

            this.Entities
                .WithBurst()
                .WithReadOnly(sensorLinks)
                .WithReadOnly(sensorPoss)
                .WithNativeDisableContainerSafetyRestriction(sensorPoss)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref TargetSensorResponse.PositionData response,
                        ref DynamicBuffer<TargetSensorHolder.SensorNextTimeData> nexts,
                        in DynamicBuffer<TargetSensorHolder.SensorLinkData> links
                    )
                =>
                    {

                        for (var i = 0; i < links.Length; i++)
                        {
                            var link = links[i];
                            var sensor = link.SensorEntity;

                            if (sensorLinks[sensor].TargetMainEntity != default)
                            {
                                response.Position = sensorPoss[sensor].Position;
                            }


                            var nexttime = nexts[i].nextTime;
                            if (currentTime >= nexttime)
                            {

                                cmd.AddComponent(entityInQueryIndex,
                                    link.SensorEntity,
                                    new TargetSensor.WakeupFindTag { }
                                );

                                cmd.RemoveComponent<Disabled>(entityInQueryIndex, entity);


                                nexts[i] = new TargetSensorHolder.SensorNextTimeData
                                {
                                    nextTime = nexttime + link.Interval,
                                };
                            }
                            
                        }

                    }
                )
                .ScheduleParallel();
        }


    }

}

