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

namespace Abss.Instance
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class PlayerMoveSystem : JobComponentSystem
    {
        

        //public Transform TfCamera;




        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //return inputDeps;
            
            var gp = Gamepad.current;
            var input = gp != null ? getPadInput( gp ) : getKeyInput( Keyboard.current );

            var tfCamera = Camera.main.transform;
            var camRotWorld = (quaternion)tfCamera.rotation;//this.TfCamera.rotation;


            inputDeps = new PlayerMoveJob
            {
                CamRotWorld = camRotWorld,
                DeltaTime = Time.deltaTime,
                StickDir = input.lStickDir,
                JumpForce = input.jumpForce,
            }
            .Schedule( this, inputDeps );

            var rs = gp != null ? gp.rightStick.ReadValue() : Mouse.current.delta.ReadValue() * 0.5f;

            tfCamera.Rotate( Vector3.up, rs.x * 90.0f * Time.deltaTime );
            //tfCamera.Rotate( Vector3.left, rs.y * 90.0f * Time.deltaTime );

            return inputDeps;


            (float3 lStickDir, float jumpForce) getPadInput( Gamepad gp_ )
            {
                var ls = gp_.leftStick.ReadValue();
                var lStickDir = new float3( ls.x, 0.0f, ls.y );
                var jumpForce = gp_.leftShoulder.wasPressedThisFrame ? 10.0f : 0.0f;

                return (lStickDir, jumpForce);
            }
            (float3 lStickDir, float jumpForce) getKeyInput( Keyboard kb )
            {
                var l = kb.dKey.isPressed ? 1.0f : 0.0f;
                var r = kb.aKey.isPressed ? -1.0f : 0.0f;
                var u = kb.wKey.isPressed ? 1.0f : 0.0f;
                var d = kb.sKey.isPressed ? -1.0f : 0.0f;
                var lStickDir = new float3( l + r, 0.0f, u + d );
                var jumpForce = kb.spaceKey.wasPressedThisFrame ? 10.0f : 0.0f;

                return (lStickDir, jumpForce);
            }
        }



        [BurstCompile]
        struct PlayerMoveJob : IJobForEachWithEntity
            <PlayerCharacterTag, PhysicsVelocity>
        {

            public float3 StickDir;
            public quaternion CamRotWorld;
            public float DeltaTime;
            public float JumpForce;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref PlayerCharacterTag tag,
                ref PhysicsVelocity v
            )
            {

                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 20;
                xyDir.y = this.JumpForce * 0.5f;
                v.Linear = math.min( v.Linear + xyDir, new float3(10,10000,10) );

            }
        }
    }
}
