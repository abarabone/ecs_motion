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
        public float3 LookDirection;

        public quaternion LookRotation =>
            quaternion.LookRotation( this.LookDirection, math.up() );

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
                        var gp = Gamepad.current;
                        var ls = gp.leftStick.ReadValue();
                        var ldir = new float3( ls.x, 0.0f, ls.y );
                        var jumpForce = gp.leftShoulder.wasPressedThisFrame ? 10.0f : 0.0f;

                        var rs = gp.rightStick.ReadValue();
                        var rdir = new float3( rs.x, 0.0f, rs.y );

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookDirection = rdir,
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
                        var kb = Keyboard.current;
                        var l = kb.dKey.isPressed ? 1.0f : 0.0f;
                        var r = kb.aKey.isPressed ? -1.0f : 0.0f;
                        var u = kb.wKey.isPressed ? 1.0f : 0.0f;
                        var d = kb.sKey.isPressed ? -1.0f : 0.0f;
                        var ldir = new float3( l + r, 0.0f, u + d );
                        var jumpForce = kb.spaceKey.wasPressedThisFrame ? 10.0f : 0.0f;

                        var ms = Mouse.current;
                        var rm = ms.delta.ReadValue() * 0.5f;
                        var rdir = new float3( rm.x, 0.0f, rm.y );

                        return new ControlActionUnit
                        {
                            MoveDirection = ldir,
                            LookDirection = rdir,
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

            inputDeps = new ContDevJob
            {
                Acts = acts,
            }
            .Schedule( this, inputDeps );

            var tfCamera = Camera.main.transform;
            var camRotWorld = (quaternion)tfCamera.rotation;

            //tfCamera.Rotate( Vector3.left, rs.y * 90.0f * Time.deltaTime );
            tfCamera.Rotate( Vector3.up, acts.LookDirection.x * 90.0f * Time.deltaTime );

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
