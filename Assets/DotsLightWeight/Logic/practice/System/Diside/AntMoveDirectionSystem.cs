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

            var targets = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(targets)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref MoveHandlingData handle,
                        in Move.TurnParamaterData param,
                        in Rotation rot,
                        in Translation pos,
                        in TargetSensorHolderLink.HolderLinkData link
                    )
                =>
                    {

                        var target = targets[link.HolderEntity];

                        var line = target.Position - pos.Value;
                        var lensq = math.lengthsq(line);

                        if (lensq < float.MinValue)
                        {
                            handle.ControlAction.MoveDirection = math.forward(rot.Value);
                            handle.ControlAction.LookRotation = rot.Value;
                            handle.ControlAction.BodyRotation = rot.Value;
                            handle.ControlAction.VerticalAngle = 0.0f;
                            return;
                        }


                        var dir = line / math.sqrt(lensq);
                        var up = math.mul(rot.Value, math.up());

                        if (math.abs(math.dot(dir, up)) > 0.3f)
                        {
                            handle.ControlAction.MoveDirection = math.forward(rot.Value);
                            handle.ControlAction.LookRotation = rot.Value;
                            handle.ControlAction.BodyRotation = rot.Value;
                            handle.ControlAction.VerticalAngle = 0.0f;
                            return;
                        }



                        var right = math.normalize(math.cross(up, dir));
                        var newforward = math.normalize(math.cross(right, up));
                        var oldforward = math.forward(rot.Value);
                        var oldrot = rot.Value;

                        var maxrad = param.TurnRadPerSec * deltaTime;
                        var rad = math.acos(math.dot(oldforward, newforward));
                        var r = math.min(rad, maxrad);

                        var c = oldforward.zx * newforward.xz;
                        var drot = quaternion.AxisAngle(up, r * math.sign(c.x - c.y));
                        var newrot = math.mul(oldrot, drot);


                        handle.ControlAction.MoveDirection = newforward;
                        handle.ControlAction.LookRotation = newrot;
                        handle.ControlAction.BodyRotation = newrot;
                        handle.ControlAction.VerticalAngle = 0.0f;
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
