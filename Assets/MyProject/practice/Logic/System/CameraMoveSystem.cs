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

            this.Entities.With( this.eq )
                .ForEach(
                    ( ref Translation pos, ref Rotation rot, ref MoveHandlingData handler ) =>
                    {

                        ref var acts = ref handler.ControlAction;


                        tfCam.rotation = acts.LookRotation;


                        //var cam_lookup = new float3(0.0f, 0.4f, -2.5f);
                        //var cam_lookdown = new float3(0.0f, 0.4f, -4.0f);
                        //var rot_center = new float3(0.0f, 0.8f, 0.0f);

                        var cam_lookup = new float3(0.0f, 0.0f, -3.0f);
                        var cam_lookdown = new float3(0.0f, 0.0f, -2.0f);
                        var rot_center = new float3(0.0f, 0.8f, 0.0f);

                        var rateUpToDown = math.min(0.0f, acts.VerticalAngle) / math.radians(90.0f);
                        var camOffset = math.lerp( cam_lookup, cam_lookdown, rateUpToDown );

                        tfCam.position = pos.Value + rot_center + math.mul(acts.LookRotation, camOffset);

                        //var camz = 2.5f + math.min( 0.0f, acts.VerticalAngle ) / math.radians( 90.0f ) * 1.5f;
                        //var camOffset = new float3( 0.0f, 0.4f, -camz );

                        //tfCam.position =
                        //    pos.Value + new float3( 0.0f, 0.8f, 0.0f ) + math.mul( acts.LookRotation, camOffset );


                        //var camz = 2.5f - math.abs(acts.VerticalAngle) / math.radians(90.0f) * 1.5f;
                        //var camOffset = new float3(0.0f, 0.4f, -camz);

                        //tfCam.position =
                        //    pos.Value + new float3(0.0f, 0.8f - 0.43f, 0.0f) + math.mul(acts.LookRotation, camOffset);
                    }
                );

        }

    }
}
