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
using Abss.Motion;

namespace Abss.Instance
{

    [DisableAutoCreation]
    //[UpdateAfter( typeof( EndFramePhysicsSystem ) )]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class PlayerMoveSystem : JobComponentSystem
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないとエラーになるみたい
        //StepPhysicsWorld stepPhysicsWorldSystem;


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
            //this.stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

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

            //tfCamera.Rotate( Vector3.left, rs.y * 90.0f * Time.deltaTime );
            tfCamera.Rotate( Vector3.up, rs.x * 90.0f * Time.deltaTime );

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
            <PlayerTag, /*GroundHitColliderData,*/ Translation, PhysicsVelocity>
        {
            
            [ReadOnly] public float3 StickDir;
            [ReadOnly] public quaternion CamRotWorld;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float JumpForce;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [ReadOnly] ref PlayerTag tag,
                //[ReadOnly] ref GroundHitColliderData hit,
                [ReadOnly] ref Translation pos,
                ref PhysicsVelocity v
            )
            {

                var upf = 0.0f;

                if( this.JumpForce > 0.0f )
                {
                    //var hitInput = new ColliderCastInput
                    //{
                    //    Collider = (Collider*)hit.Collider.GetUnsafePtr(),
                    //    Orientation = quaternion.identity,
                    //    Start = pos.Value + math.up() * 0.05f,
                    //    End = pos.Value + math.up() * -0.1f,
                    //};
                    var hitInput = new PointDistanceInput
                    {
                        Position = pos.Value,
                        MaxDistance = 0.1f,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ( 1 << 20 ) | ( 1 << 22 ) | ( 1 << 23 ),
                            CollidesWith = (1<<20) | (1<<22) | (1<<23),
                            GroupIndex = 0,
                        },
                    };
                    //var collector = new ExcludeEntityCollector
                    //{
                    //    IgnoreEntity = entity,
                    //    Rigidbodies = this.CollisionWorld.Bodies,
                    //};
                    var a = new NativeList<DistanceHit>( Allocator.Temp );
                    var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );
                    if( isHit && a.Length > 1 )
                    {
                        upf = this.JumpForce * 0.5f;
                    }
                    a.Dispose();
                }

                var vlinear = v.Linear;
                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 170;
                
                xyDir.y = vlinear.y + upf;
                
                v.Linear = math.min( xyDir, new float3(10,1000,10) );

            }
        }

        struct ExcludeEntityCollector : ICollector<Unity.Physics.RaycastHit>
        {

            public Entity IgnoreEntity;
            public NativeSlice<Unity.Physics.RigidBody> Rigidbodies;


            public bool EarlyOutOnFirstHit => IgnoreEntity == Entity.Null;

            public float MaxFraction => 0.0f;

            public int NumHits => 0;


            public bool AddHit( Unity.Physics.RaycastHit hit )
            {
                if( this.Rigidbodies[ hit.RigidBodyIndex ].Entity == this.IgnoreEntity )
                {
                    this.IgnoreEntity = Entity.Null;
                    return true;
                }
                return false;
            }

            public void TransformNewHits
                ( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey )
            {

            }

            public void TransformNewHits
                ( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
            {

            }
        }
    }
}
