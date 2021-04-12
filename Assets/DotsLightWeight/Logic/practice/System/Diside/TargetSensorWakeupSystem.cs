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


    // ホルダーからセンサーを起動する
    // センサーはインターバルごとに PollingTag をつけて起動する
    // 

    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class TargetSensorWakeSystem : SystemBase
    {



        protected override void OnCreate()
        { }


        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var currentFrame = UnityEngine.Time.frameCount;

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref TargetSensorResponse.CurrentData current,
                        in TargetSensor.LinkTargetMainData mainlink
                    )
                =>
                    {




                    }
                )
                .ScheduleParallel();
        }


    }

}

