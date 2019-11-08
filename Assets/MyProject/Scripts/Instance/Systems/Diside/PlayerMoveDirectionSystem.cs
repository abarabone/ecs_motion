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

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Instance;
using Abss.Motion;

namespace Abss.Instance
{


    public struct ControlActionUnit
    {
        public float3 MoveDirection;
        public quaternion LookRotation;

        public float3 LookDirection => math.forward( this.LookRotation );

        public float JumpForce;
        public bool IsChangeMotion;
    }


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class PlayerMoveDirectionSystem : JobComponentSystem
    {


        Func<ControlActionUnit> getControlUnitFunc;
        

        protected override void OnCreate()
        {

            setControlFunc_();

            return;


            void setControlFunc_()
            {
                if( Gamepad.current != null )
                {
                    this.getControlUnitFunc = () =>
                    {
                        var tfCamera = Camera.main.transform;
                        var gp = Gamepad.current;

                        var rs = gp.rightStick.ReadValue() * 100.0f;
                        var rdir = new float3( rs.x, rs.y, 0.0f );
                        var rRot = math.mul( quaternion.RotateX(rdir.x), quaternion.RotateY(rdir.y) );

                        var ls = gp.leftStick.ReadValue();
                        var ldir = math.mul( rRot, new float3( ls.x, 0.0f, ls.y ) );

                        var jumpForce = gp.leftShoulder.wasPressedThisFrame ? 10.0f : 0.0f;

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookRotation = rRot,
                            JumpForce = jumpForce,
                            IsChangeMotion = gp.bButton.wasPressedThisFrame,
                        };
                    };
                    return;
                }
                
                if( Mouse.current != null && Keyboard.current != null )
                {
                    this.getControlUnitFunc = () =>
                    {
                        var rotCam = Camera.main.transform.rotation;
                        var euler = rotCam.eulerAngles;

                        var ms = Mouse.current;
                        var rdir = ms.delta.ReadValue() * 0.005f;
                        //var rdir = new float3( rm.x, 0.0f, rm.y );

                        var rRot = quaternion.RotateX( euler.y + -rdir.y );//math.mul( quaternion.RotateX( euler.y + -rdir.y ), quaternion.RotateY( euler.x+ -rdir.x ) );//math.mul( quaternion.RotateY( rdir.y ), quaternion.RotateX( rdir.x ) );
                        //var rRot = rRot_ * rotCam;

                        var hRot = quaternion.RotateY( euler.x + -rdir.x );
                        var kb = Keyboard.current;
                        var l = kb.dKey.isPressed ? 1.0f : 0.0f;
                        var r = kb.aKey.isPressed ? -1.0f : 0.0f;
                        var u = kb.wKey.isPressed ? 1.0f : 0.0f;
                        var d = kb.sKey.isPressed ? -1.0f : 0.0f;
                        var ldir = math.mul( hRot, new float3( l + r, 0.0f, u + d ) );

                        var jumpForce = kb.spaceKey.wasPressedThisFrame ? 10.0f : 0.0f;

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookRotation = rRot,
                            JumpForce = jumpForce,
                            IsChangeMotion = ms.rightButton.wasPressedThisFrame,
                        };
                    };
                    return;
                }
            }
            
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var acts = this.getControlUnitFunc();
            Camera.main.transform.rotation = acts.LookRotation;
            
            inputDeps = new ContDevJob
            {
                Acts = acts,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        struct ContDevJob : IJobForEachWithEntity
            <PlayerTag, MoveHandlingData>
        {

            //[ReadOnly] public EntityCommandBuffer.Concurrent Commands;
            [ReadOnly] public ControlActionUnit Acts;

            //[ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref PlayerTag tag,
                ref MoveHandlingData handler
            )
            {
                //if( this.Acts.IsChangeMotion )
                //{
                //    var motionInfo = this.MotionInfos[ linker.MotionEntity ];
                //    this.Commands.AddComponent( index, linker.MotionEntity, new MotionInitializeData { MotionIndex = (motionInfo.MotionIndex+1) % 10 } );
                //}


                handler.ControlAction = this.Acts;

            }
        }
    }

}
