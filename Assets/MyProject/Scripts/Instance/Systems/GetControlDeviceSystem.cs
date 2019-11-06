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

namespace Abss.Character
{


    struct ControlActionUnit
    {
        public float3 MoveDirection;
        public float3 LookDirection;

        public quaternion LookRotation;

        public float JumpForce;
        public bool IsChangeMotion;
    }


    [DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class GetControlDeviceSystem : JobComponentSystem
    {


        Func<ControlActionUnit> getControlUnitFunc;
        
        EntityCommandBufferSystem ecb;
        

        protected override void OnCreate()
        {

            this.ecb = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();


            if( Gamepad.current != null )
                this.getControlUnitFunc = () =>
                {
                    var gp = Gamepad.current;
                    var ls = gp.leftStick.ReadValue();
                    var lStickDir = new float3( ls.x, 0.0f, ls.y );
                    var jumpForce = gp.leftShoulder.wasPressedThisFrame ? 10.0f : 0.0f;

                    return new ControlActionUnit
                    {
                        MoveDirection = lStickDir,
                        JumpForce = jumpForce,
                        IsChangeMotion = gp.bButton.wasPressedThisFrame,
                    };
                };

            if( Mouse.current != null && Keyboard.current != null )
                this.getControlUnitFunc = () =>
                {
                    var kb = Keyboard.current;
                    var l = kb.dKey.isPressed ? 1.0f : 0.0f;
                    var r = kb.aKey.isPressed ? -1.0f : 0.0f;
                    var u = kb.wKey.isPressed ? 1.0f : 0.0f;
                    var d = kb.sKey.isPressed ? -1.0f : 0.0f;
                    var lStickDir = new float3( l + r, 0.0f, u + d );
                    var jumpForce = kb.spaceKey.wasPressedThisFrame ? 10.0f : 0.0f;

                    var ms = Mouse.current;

                    return new ControlActionUnit
                    {
                        MoveDirection = lStickDir,
                        JumpForce = jumpForce,
                        IsChangeMotion = ms.rightButton.wasPressedThisFrame,
                    };
                };
            
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            
            inputDeps = new ContDevJob
            {
                Commands = this.ecb.CreateCommandBuffer().ToConcurrent(),
                Acts = this.getControlUnitFunc(),
                MotionInfos = this.GetComponentDataFromEntity<MotionInfoData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }


        struct ContDevJob : IJobForEachWithEntity<CharacterLinkData, PlayerCharacterTag>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
            [ReadOnly] public ControlActionUnit Acts;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref CharacterLinkData linker,
                [ReadOnly] ref PlayerCharacterTag tag
            )
            {
                if( this.Acts.IsChangeMotion )
                {
                    var motionInfo = this.MotionInfos[ linker.MotionEntity ];
                    this.Commands.AddComponent( index, linker.MotionEntity, new MotionInitializeData { MotionIndex = (motionInfo.MotionIndex+1) % 10 } );
                }

            }
        }
    }

}
