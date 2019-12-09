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

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Motion;

namespace Abss.Character
{



    [DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class AntMoveDirectionSystem : JobComponentSystem
    {



        protected override void OnCreate()
        { }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new AiJob
            { }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile, RequireComponentTag(typeof(AntTag))]
        struct AiJob : IJobForEachWithEntity
            <MoveHandlingData, Rotation>//
        {


            public void Execute(
                Entity entity, int index,
                [WriteOnly] ref MoveHandlingData handler,
                [ReadOnly] ref Rotation rot
            )
            {

                //handler.ControlAction.HorizontalRotation = quaternion.identity;
                //handler.ControlAction.LookRotation = quaternion.identity;
                //handler.ControlAction.MoveDirection = math.forward(rot.Value);

            }
        }


    }

}
