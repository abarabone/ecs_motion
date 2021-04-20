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
    [UpdateInGroup( typeof(InitializationSystemGroup) )]
    public class PlayerMoveDirectionSystem : SystemBase
    {



        delegate (Control.MoveData move, Control.ActionData action) PlayerControlFunction(ref Control.WorkData work, float dt);

        PlayerControlFunction playerControlFunction;



        // OnCreate() だと、ビルド版で失敗した。エディタ上との初期化タイミングの違いだろうか？
        protected override void OnStartRunning()
        {

            base.OnCreate();

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (ref Control.WorkData work) =>
                    {
                        work.hrot = quaternion.identity;
                        work.vangle = 0.0f;
                    }
                )
                .Run();

            this.playerControlFunction = setControllerFunc_();

            return;


            PlayerControlFunction setControllerFunc_()
            {
                //Debug.Log("Gamepad.current " + Gamepad.current);
                if( Gamepad.current != null )
                {
                    return (ref Control.WorkData work, float dt) =>
                    {
                        var gp = Gamepad.current;

                        var rdir = gp.rightStick.ReadValue() * 5.0f * dt;
                        //var rdir = new float3( rs.x, rs.y, 0.0f );

                        work.vangle -= rdir.y;
                        work.vangle = math.min(work.vangle, math.radians( 90.0f ) );
                        work.vangle = math.max(work.vangle, math.radians( -90.0f ) );

                        work.hrot = math.mul( quaternion.RotateY( rdir.x ), work.hrot );

                        var rRot = math.mul(work.hrot, quaternion.RotateX(work.vangle) );

                        var ls = gp.leftStick.ReadValue();
                        var ldir = math.mul(work.hrot, new float3( ls.x, 0.0f, ls.y ) );

                        var jumpForce = gp.leftShoulder.wasPressedThisFrame ? 5.0f : 0.0f;

                        return (
                            new Control.MoveData
                            {
                                MoveDirection = ldir,
                                LookRotation = rRot,
                                BodyRotation = work.hrot,
                                VerticalAngle = work.vangle,
                            },
                            new Control.ActionData
                            {
                                JumpForce = jumpForce,
                                IsChangeMotion = gp.rightShoulder.wasPressedThisFrame,
                                IsShooting = gp.rightShoulder.isPressed,
                                IsTriggerdSub = gp.leftTrigger.isPressed,
                                IsChangingWapon = gp.rightTrigger.wasPressedThisFrame,
                            }
                        );
                    };
                }

                //Debug.Log("Mouse.current " + Mouse.current);
                //Debug.Log("Keyboard.current " + Keyboard.current);
                if ( Mouse.current != null && Keyboard.current != null )
                {
                    return (ref Control.WorkData work, float dt) =>
                    {
                        
                        var ms = Mouse.current;
                        var rdir = ms.delta.ReadValue() * dt;

                        work.vangle -= rdir.y;
                        work.vangle = math.min(work.vangle, math.radians( 90.0f ) );
                        work.vangle = math.max(work.vangle, math.radians( -90.0f ) );

                        work.hrot = math.mul( quaternion.RotateY( rdir.x ), work.hrot );

                        var rRot = math.mul( work.hrot, quaternion.RotateX(work.vangle ) );

                        var kb = Keyboard.current;
                        var l = kb.sKey.isPressed ? -1.0f : 0.0f;
                        var r = kb.fKey.isPressed ? 1.0f : 0.0f;
                        var u = kb.eKey.isPressed ? 1.0f : 0.0f;
                        var d = kb.dKey.isPressed ? -1.0f : 0.0f;
                        var ldir = math.mul(work.hrot, new float3( l + r, 0.0f, u + d ) );
                        
                        var jumpForce = kb.spaceKey.wasPressedThisFrame ? 5.0f : 0.0f;

                        return (
                            new Control.MoveData
                            {
                                MoveDirection = ldir,
                                LookRotation = rRot,
                                BodyRotation = work.hrot,
                                VerticalAngle = work.vangle,
                            },
                            new Control.ActionData
                            {
                                JumpForce = jumpForce,
                                IsChangeMotion = ms.rightButton.wasPressedThisFrame,
                                IsShooting = ms.rightButton.isPressed,
                                IsTriggerdSub = ms.middleButton.isPressed,
                                IsChangingWapon = kb.tabKey.wasPressedThisFrame,
                            }
                        );
                    };
                }

                return default;
            }
            
        }


        protected override void OnUpdate()
        {
            
            var actions = this.GetComponentDataFromEntity<Control.ActionData>();

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithoutBurst()
                .WithAll<PlayerTag>()
                .WithNativeDisableParallelForRestriction(actions)
                .ForEach(
                    (ref Control.MoveData move, ref Control.WorkData work, in Control.ActionLinkData link) =>
                    {

                        var c = this.playerControlFunction(ref work, dt);

                        move = c.move;

                        actions[link.ActionEntity] = c.action;

                    }
                )
                .Run();
        }

    }

}
