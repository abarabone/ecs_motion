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

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;
using Abarabone.Character;
using Abarabone.CharacterMotion;

namespace Abarabone.Character
{



    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class AntMoveDirectionSystem : SystemBase
    {



        protected override void OnCreate()
        { }


        protected override void OnUpdate()
        {
            //inputDeps = new AiJob
            //{ }
            //.Schedule( this, inputDeps );

            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref MoveHandlingData handler,
                        in Rotation rot
                    )
                =>
                    {
                        //handler.ControlAction.HorizontalRotation = quaternion.identity;
                        //handler.ControlAction.LookRotation = quaternion.identity;
                        //handler.ControlAction.MoveDirection = math.forward(rot.Value);
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
