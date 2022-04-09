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

using DotsLite.Misc;
using DotsLite.Utilities;
using DotsLite.SystemGroup;

namespace DotsLite.Character
{

    //[UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    ////[UpdateInGroup(typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    //[UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public partial class CameraMoveSystem : SystemBase//ComponentSystem
    {


        //EntityQuery eq;


        protected override void OnCreate()
        {
            base.OnCreate();
            this.RequireSingletonForUpdate<CharacterFollowCameraPositionData>();


            //if (Gamepad.current == null)
            //{
            //    Cursor.lockState = CursorLockMode.Locked;
            //    Cursor.visible = false;
            //}

            //this.eq = this.Entities
            //    .WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData>()
            //    //.WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData, GroundHitSphereData>()
            //    //.WithAll<GroundHitResultData>()
            //    .ToEntityQuery();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }


        protected override void OnUpdate()
        {

            //if (Keyboard.current.wasUpdatedThisFrame)
            //{
            //    Cursor.lockState ^= CursorLockMode.Locked;
            //    Cursor.visible ^= false;
            //}


            var tfCam = Camera.main.transform;

            var campos = this.GetSingleton<CharacterFollowCameraPositionData>();
            var poss = this.GetComponentDataFromEntity<Translation>();
            var rots = this.GetComponentDataFromEntity<Rotation>();
            var cament = this.GetSingletonEntity<CharacterFollowCameraPositionData>();


            this.Entities//.With( this.eq )
                .WithoutBurst()
                .WithAll<PlayerTag>()
                .WithNativeDisableParallelForRestriction(poss)
                .WithNativeDisableParallelForRestriction(rots)
                .ForEach(
                    ( in Translation pos, in Rotation rot, in Control.MoveData moves ) =>
                    {

                        //var forwardpos = campos.lookForwardPosition;
                        //var downpos = campos.lookDownPosition - campos.lookForwardPosition;
                        //var uppos = campos.lookUpPosition - campos.lookForwardPosition;
                        //var rotcenter = campos.RotationCenter;

                        //var vdeg = math.degrees(moves.VerticalAngle);
                        //const float invDeg = 1.0f / 90.0f;
                        //var rate_forwardToDown = math.max(vdeg, 0.0f) * invDeg;
                        //var rate_forwardToUp = -math.min(vdeg, 0.0f) * invDeg;
                        //var camOffset = forwardpos + downpos * rate_forwardToDown + uppos * rate_forwardToUp;

                        //tfCam.position = pos.Value +
                        //    math.mul(moves.BodyRotation, rotcenter) + math.mul(moves.LookRotation, camOffset);

                        //tfCam.rotation = moves.LookRotation;

                        var newpos = calc_cam(campos, moves, pos.Value);

                        tfCam.position = newpos;
                        tfCam.rotation = moves.LookRotation;

                        poss[cament] = new Translation { Value = newpos };
                        rots[cament] = new Rotation { Value = moves.LookRotation };
                    }
                )
                .Run();

        }

        //[BurstCompile]
        static float3 calc_cam
            (CharacterFollowCameraPositionData campos, Control.MoveData moves, float3 pos)
        {
            var forwardpos = campos.lookForwardPosition;
            var downpos = campos.lookDownPosition - campos.lookForwardPosition;
            var uppos = campos.lookUpPosition - campos.lookForwardPosition;
            var rotcenter = campos.RotationCenter;

            var vdeg = math.degrees(moves.VerticalAngle);
            const float invDeg = 1.0f / 90.0f;
            var rate_forwardToDown = math.max(vdeg, 0.0f) * invDeg;
            var rate_forwardToUp = -math.min(vdeg, 0.0f) * invDeg;
            var camOffset = forwardpos + downpos * rate_forwardToDown + uppos * rate_forwardToUp;

            return pos + math.mul(moves.BodyRotation, rotcenter) + math.mul(moves.LookRotation, camOffset);
        }
    }
}
