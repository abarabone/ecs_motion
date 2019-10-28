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
                Positions = this.GetComponentDataFromEntity<Translation>(),
                Rotations = this.GetComponentDataFromEntity<Rotation>(),
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
        struct PlayerMoveJob : IJobForEachWithEntity<PlayerCharacterTag, CharacterLinkData>
        {

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> Positions;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation> Rotations;

            public float3 StickDir;
            public quaternion CamRotWorld;
            public float DeltaTime;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref PlayerCharacterTag tag,
                [ReadOnly] ref CharacterLinkData linker
            )
            {

                var pos = this.Positions[ linker.PostureEntity ];
                var rot = this.Rotations[ linker.PostureEntity ];


                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 10;
                pos.Value += xyDir;

                this.Positions[ linker.PostureEntity ] = pos;
                this.Rotations[ linker.PostureEntity ] = rot;
                
            }
        }
    }
}
