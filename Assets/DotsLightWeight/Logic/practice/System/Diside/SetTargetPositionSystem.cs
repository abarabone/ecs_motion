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


    // メイン位置を持つ物体を、いったん単なる位置になおす
    // 移動処理に汎用性をもたせられる

    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class SetTargetPosiionSystem : SystemBase
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
                        ref TargetSensor.CurrentData current,
                        in TargetSensor.MainLinkData mainlink
                    )
                =>
                    {

                        var targetPos = poss[mainlink.MainEntity];

                        current.Position = targetPos.Value;
                        current.LastFrame = currentFrame;
                    }
                )
                .ScheduleParallel();
        }


    }

}

