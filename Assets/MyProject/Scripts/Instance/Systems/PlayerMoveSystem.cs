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
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

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

        BuildPhysicsWorld buildPhysicsWorldSystem;
        StepPhysicsWorld stepPhysicsWorldSystem;



        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            this.stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        }


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
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
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

            [ReadOnly] public float3 StickDir;
            [ReadOnly] public quaternion CamRotWorld;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float JumpForce;

            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public BlobAssetReference<Collider> MovebodyCollider;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref PlayerCharacterTag tag,
                ref PhysicsVelocity v
            )
            {

                var upf = 0.0f;

                if( this.JumpForce > 0.0f )
                {
                    var hitInput = new ColliderCastInput
                    {
                        Collider = (Collider*)this.MovebodyCollider.GetUnsafePtr(),
                        Orientation = quaternion.identity,
                        Start = position.Value + math.up() * 0.15f,
                        End = position.Value + math.up() * -0.05f,
                    };
                    var isHit = this.CollisionWorld.CastCollider( hitInput );
                    if( isHit )
                    {
                        upf = this.JumpForce * 0.5f;
                    }
                }

                var vlinear = v.Linear;
                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 170;
                
                xyDir.y = vlinear.y + this.JumpForce * 0.5f;

                v.Linear = math.min( xyDir, new float3(10,1000,10) );

            }
        }
    }
}
