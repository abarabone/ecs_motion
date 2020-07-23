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
using Unity.Physics.Systems;

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;

namespace Abarabone.Character
{

    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class CameraMoveSystem : ComponentSystem
    {


        EntityQuery eq;


        protected override void OnCreate()
        {
            this.eq = this.Entities
                .WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData>()
                //.WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData, GroundHitSphereData>()
                //.WithAll<GroundHitResultData>()
                .ToEntityQuery();
        }


        protected override void OnUpdate()
        {

            var tfCam = Camera.main.transform;

            var campos = this.GetSingleton<CharacterFollowCameraPositionData>();


            this.Entities.With( this.eq )
                .ForEach(
                    ( ref Translation pos, ref Rotation rot, ref MoveHandlingData handler ) =>
                    {

                        ref var acts = ref handler.ControlAction;


                        var forwardpos = campos.lookForwardPosition;
                        var downpos = campos.lookDownPosition - campos.lookForwardPosition;
                        var uppos = campos.lookUpPosition - campos.lookForwardPosition;
                        var rotcenter = campos.RotationCenter;

                        var vdeg = math.degrees(acts.VerticalAngle);
                        const float invDeg = 1.0f / 90.0f;
                        var rate_forwardToDown = math.max(vdeg, 0.0f) * invDeg;
                        var rate_forwardToUp = -math.min(vdeg, 0.0f) * invDeg;
                        var camOffset = forwardpos + downpos * rate_forwardToDown + uppos * rate_forwardToUp;

                        tfCam.position = pos.Value +
                            math.mul(acts.HorizontalRotation, rotcenter) + math.mul(acts.LookRotation, camOffset);

                        tfCam.rotation = acts.LookRotation;

                    }
                );

        }

    }
}
