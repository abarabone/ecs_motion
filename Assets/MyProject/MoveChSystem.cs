using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Physics;
using Unity.Scenes;
using UnityEngine.InputSystem;
using Unity.Burst;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace MyProject
{

    [DisableAutoCreation]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    //[UpdateAfter(typeof(StepPhysicsWorld))]
    public class MoveChSystem : JobComponentSystem
    {
        
        public Transform TfCamera;


        BuildPhysicsWorld buildPhysicsWorldSystem;
        StepPhysicsWorld stepPhysicsWorldSystem;
        
        EntityQuery ImpulseGroup;
        

        BlobAssetReference<Collider> movebodySphereCollider;


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            this.stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
            //this.ImpulseGroup = GetEntityQuery( new EntityQueryDesc
            //{
            //    All = new ComponentType[] { typeof( MoveChData ), }
            //} );

            var geom = new SphereGeometry()
            {
                Center  = float3.zero,
                Radius  = 0.15f,
            };
            var filter = new CollisionFilter
            {
                BelongsTo       = 1u<<23,
                CollidesWith    = 1u<<20 | 1u<<22,// | 1u<<23,
                GroupIndex      = 0,
            };
            this.movebodySphereCollider = SphereCollider.Create( geom, filter );
        }

        protected override void OnDestroy()
        {
            this.movebodySphereCollider.Dispose();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var gp = Gamepad.current;
            var input = gp != null ? getPadInput( gp ) : getKeyInput( Keyboard.current );

            var tfCamera = Camera.main.transform;
            var camRotWorld = (quaternion)tfCamera.rotation;//this.TfCamera.rotation;

            inputDeps = new MoveChJob
            {
                CamRotWorld = camRotWorld,
                DeltaTime = Time.deltaTime,
                StickDir = input.lStickDir,
                JumpForce = input.jumpForce,
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                MovebodyCollider = this.movebodySphereCollider,
            }
            .Schedule( this, inputDeps );

            var rs = gp != null ? gp.rightStick.ReadValue() : Mouse.current.delta.ReadValue() * 0.5f;

            tfCamera.Rotate( Vector3.up, rs.x * 90.0f * Time.deltaTime );
            //tfCamera.Rotate( Vector3.left, rs.y * 90.0f * Time.deltaTime );


            //inputDeps = new MoveChOnGroundJob
            //{
            //    ColliderEventImpulseGroup = GetComponentDataFromEntity<MoveChData>( isReadOnly: true ),
            //    PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
            //}
            //.Schedule(
            //    this.stepPhysicsWorldSystem.Simulation,
            //    ref this.buildPhysicsWorldSystem.PhysicsWorld,
            //    inputDeps
            //);


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
                var lStickDir = new float3( l+r, 0.0f, u+d );
                var jumpForce = kb.spaceKey.wasPressedThisFrame ? 10.0f : 0.0f;

                return (lStickDir, jumpForce);
            }
        }
        
        
        [BurstCompile]
        struct MoveChJob : IJobForEach<PhysicsVelocity, Translation>
        {
            public float3 StickDir;
            public quaternion CamRotWorld;
            public float DeltaTime;
            public float JumpForce;

            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public BlobAssetReference<Collider> MovebodyCollider;

            public unsafe void Execute
                ( ref PhysicsVelocity velocity, [ReadOnly] ref Translation position )
            {
                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 100;


                var upf = 0.0f;

                if( this.JumpForce > 0.0f )
                {
                    var hitInput = new ColliderCastInput
                    {
                        Collider    = (Collider*)this.MovebodyCollider.GetUnsafePtr(),
                        Orientation = quaternion.identity,
                        Start       = position.Value + math.up() * 0.15f,
                        End         = position.Value + math.up() * -0.05f,
                    };
                    var isHit = this.CollisionWorld.CastCollider( hitInput );
                    if( isHit )
                    {
                        upf = this.JumpForce * 0.5f;
                    }
                }

                var v = new float3( xyDir.x, velocity.Linear.y + upf, xyDir.z );
                
                velocity.Linear = v;
            }
        }
        
        [BurstCompile]
        struct MoveChOnGroundJob : ICollisionEventsJob
        {
            [ReadOnly] public ComponentDataFromEntity<MoveChData> ColliderEventImpulseGroup;
            public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

            public void Execute( CollisionEvent collisionEvent )
            {
                Entity entityA = collisionEvent.Entities.EntityA;
                Entity entityB = collisionEvent.Entities.EntityB;

                bool isBodyADynamic = PhysicsVelocityGroup.Exists(entityA);
                bool isBodyBDynamic = PhysicsVelocityGroup.Exists(entityB);

                //bool isBodyARepulser = ColliderEventImpulseGroup.Exists(entityA);
                //bool isBodyBRepulser = ColliderEventImpulseGroup.Exists(entityB);

                //if(isBodyARepulser && isBodyBDynamic)
                if(isBodyBDynamic)
                {
                    //var impulseComponent = ColliderEventImpulseGroup[entityA];
                    var velocityComponent = PhysicsVelocityGroup[entityB];
                    velocityComponent.Linear *= 1.5f;
                    PhysicsVelocityGroup[ entityB ] = velocityComponent;
                }
                //if (isBodyBRepulser && isBodyADynamic)
                if(isBodyADynamic)
                {
                    //var impulseComponent = ColliderEventImpulseGroup[entityB];
                    var velocityComponent = PhysicsVelocityGroup[entityA];
                    velocityComponent.Linear *= 1.5f;
                    PhysicsVelocityGroup[ entityA ] = velocityComponent;
                }

                void jump( ref PhysicsVelocity v_ )
                {

                }
            }
        }
    }


    public struct MoveChData : IComponentData
    {
        public float value;
    }
}
