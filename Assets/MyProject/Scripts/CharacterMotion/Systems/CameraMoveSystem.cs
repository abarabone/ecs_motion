using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine.InputSystem;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Character
{

    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup(typeof(ObjectLogicSystemGroup))]
    public class CameraMoveSystem : ComponentSystem
    {


        EntityQuery eq;


        protected override void OnCreate()
        {
            this.eq = this.Entities
                .WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData>()
                .ToEntityQuery();
        }


        protected override void OnUpdate()
        {
            var tfCam = Camera.main.transform;

            this.Entities.With( this.eq )
                .ForEach(
                    ( ref Translation pos, ref Rotation rot, ref MoveHandlingData handler ) =>
                    {

                        ref var acts = ref handler.ControlAction;


                        tfCam.rotation = acts.LookRotation;

                        var camz = 2.5f - math.abs( acts.VerticalAngle ) / math.radians(90.0f) * 1.5f;
                        //var camz = 2.5f + math.min( 0.0f, acts.VerticalAngle ) / math.radians( 90.0f ) * 1.5f;
                        var camOffset = new float3( 0.0f, 0.4f, -camz );

                        tfCam.position =
                            pos.Value + new float3(0.0f,0.8f-0.43f,0.0f) + math.mul( acts.LookRotation, camOffset );

                    }
                );
        }

    }
}
