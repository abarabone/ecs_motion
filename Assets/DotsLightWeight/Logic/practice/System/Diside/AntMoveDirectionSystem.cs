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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class AntMoveDirectionSystem : SystemBase
    {



        protected override void OnCreate()
        { }


        protected override void OnUpdate()
        {

            var targets = this.GetComponentDataFromEntity<TargetSensor.PositionData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(targets)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Rotation rot,
                        in Translation pos,
                        in TargetSensor.MoveTargetLinkData link
                    )
                =>
                    {

                        var target = targets[link.MoveTargetEnity];

                        var dir = target.Position - pos.Value;
                        var up = math.mul(rot.Value, math.up());

                        if (math.dot(dir, up) < float.MinValue) return;


                        rot.Value = quaternion.LookRotation(dir, up);

                    }
                )
                .ScheduleParallel();
        }



        //[BurstCompile, RequireComponentTag(typeof(AntTag))]
        //struct AiJob : IJobForEachWithEntity
        //    <MoveHandlingData, Rotation>//
        //{


        //    public void Execute(
        //        Entity entity, int index,
        //        [WriteOnly] ref MoveHandlingData handler,
        //        [ReadOnly] ref Rotation rot
        //    )
        //    {

        //        //handler.ControlAction.HorizontalRotation = quaternion.identity;
        //        //handler.ControlAction.LookRotation = quaternion.identity;
        //        //handler.ControlAction.MoveDirection = math.forward(rot.Value);

        //    }
        //}


    }

}
