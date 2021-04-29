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

namespace DotsLite.Character
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Dependency;


    // ホルダーからセンサーを起動する
    // センサーはインターバルごとに WakeupFindTag をつけて起動する
    // 

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class TargetSensorWakeupAndCopyPositionSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            //var sensorLinks = this.GetComponentDataFromEntity<TargetSensor.LinkTargetMainData>(isReadOnly: true);
            var sensorPoss = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var disables = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);

            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var currentTime = this.Time.ElapsedTime;

            this.Entities
                .WithBurst()
                //.WithReadOnly(sensorLinks)
                .WithReadOnly(sensorPoss)
                .WithReadOnly(disables)
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

                            var nexttime = nexts[i].NextTime;
                            if (currentTime >= nexttime)
                            {

                                cmd.AddComponent<TargetSensor.WakeupFindTag>(entityInQueryIndex, sensor);

                                cmd.AddComponent<TargetSensor.AcqurireTag>(entityInQueryIndex, sensor);


                                nexts[i] = new TargetSensorHolder.SensorNextTimeData
                                {
                                    NextTime = (float)(currentTime + link.Interval),
                                };
                            }
                        }

                        for (var i = 0; i < links.Length; i++)
                        {
                            var link = links[i];
                            var sensor = link.SensorEntity;

                            if (!disables.HasComponent(sensor))
                            {
                                response.Position = sensorPoss[sensor].Position;

                                break;
                            }
                        }

                    }
                )
                .ScheduleParallel();
        }


    }

}

