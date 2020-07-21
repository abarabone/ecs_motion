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

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;
using Abarabone.Character;
using Abarabone.CharacterMotion;

namespace Abarabone.Character
{



    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class PlayerMoveDirectionSystem : JobComponentSystem
    {


        Func<ControlActionUnit> getControlUnitFunc;

        //quaternion vrot = quaternion.identity;
        quaternion hrot = quaternion.identity;
        float vangle = 0.0f;

        protected override void OnCreate()
        {

            setControllerFunc_();

            return;


            void setControllerFunc_()
            {
                if( Gamepad.current != null )
                {
                    this.getControlUnitFunc = () =>
                    {
                        var gp = Gamepad.current;

                        var rdir = gp.rightStick.ReadValue() * 5.0f * Time.DeltaTime;
                        //var rdir = new float3( rs.x, rs.y, 0.0f );

                        this.vangle -= rdir.y;
                        this.vangle = math.min( this.vangle, math.radians( 90.0f ) );
                        this.vangle = math.max( this.vangle, math.radians( -90.0f ) );

                        this.hrot = math.mul( quaternion.RotateY( rdir.x ), this.hrot );

                        var rRot = math.mul( this.hrot, quaternion.RotateX(this.vangle) );

                        var ls = gp.leftStick.ReadValue();
                        var ldir = math.mul( this.hrot, new float3( ls.x, 0.0f, ls.y ) );

                        var jumpForce = gp.leftShoulder.wasPressedThisFrame ? 5.0f : 0.0f;

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookRotation = rRot,
                            HorizontalRotation = this.hrot,
                            VerticalAngle = this.vangle,
                            JumpForce = jumpForce,
                            IsChangeMotion = gp.rightShoulder.wasPressedThisFrame,
                            IsShooting = gp.rightShoulder.isPressed
                        };
                    };
                    return;
                }
                
                if( Mouse.current != null && Keyboard.current != null )
                {
                    this.getControlUnitFunc = () =>
                    {
                        
                        var ms = Mouse.current;
                        var rdir = ms.delta.ReadValue() * Time.DeltaTime;

                        this.vangle -= rdir.y;
                        this.vangle = math.min( this.vangle, math.radians( 90.0f ) );
                        this.vangle = math.max( this.vangle, math.radians( -90.0f ) );

                        this.hrot = math.mul( quaternion.RotateY( rdir.x ), this.hrot );

                        var rRot = math.mul( this.hrot, quaternion.RotateX( this.vangle ) );

                        var kb = Keyboard.current;
                        var l = kb.sKey.isPressed ? -1.0f : 0.0f;
                        var r = kb.fKey.isPressed ? 1.0f : 0.0f;
                        var u = kb.eKey.isPressed ? 1.0f : 0.0f;
                        var d = kb.dKey.isPressed ? -1.0f : 0.0f;
                        var ldir = math.mul( this.hrot, new float3( l + r, 0.0f, u + d ) );
                        
                        var jumpForce = kb.spaceKey.wasPressedThisFrame ? 5.0f : 0.0f;

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookRotation = rRot,
                            HorizontalRotation = this.hrot,
                            VerticalAngle = this.vangle,
                            JumpForce = jumpForce,
                            IsChangeMotion = ms.rightButton.wasPressedThisFrame,
                            IsShooting = ms.rightButton.isPressed,
                        };
                    };
                    return;
                }
            }
            
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var acts = this.getControlUnitFunc();

            inputDeps = new ContDevJob
            {
                Acts = acts,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile, RequireComponentTag(typeof(PlayerTag))]
        struct ContDevJob : IJobForEachWithEntity
            <MoveHandlingData>
        {

            //[ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ControlActionUnit Acts;


            public void Execute(
                Entity entity, int index,
                [WriteOnly] ref MoveHandlingData handler
            )
            {
                //if( this.Acts.IsChangeMotion )
                //{
                //    var motionInfo = this.MotionInfos[ linker.MotionEntity ];
                //    this.Commands.AddComponent( index, linker.MotionEntity, new Motion.InitializeData { MotionIndex = (motionInfo.MotionIndex+1) % 10 } );
                //}

                handler.ControlAction = this.Acts;

            }
        }
    }

}
