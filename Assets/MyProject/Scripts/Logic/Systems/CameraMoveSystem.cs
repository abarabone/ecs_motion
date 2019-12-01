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
using Unity.Physics.Systems;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Character
{

    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup(typeof(ObjectLogicSystemGroup))]
    public class CameraMoveSystem : ComponentSystem
    {


        EntityQuery eq;


        protected override void OnCreate()
        {
            this.eq = this.Entities
                .WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData>()
                //.WithAllReadOnly<Translation, Rotation, PlayerTag, MoveHandlingData, GroundHitSphereData>()
                //.WithAll<GroundHitResultData>()
                .ToEntityQuery();
        }


        protected override void OnUpdate()
        {
            var tfCam = Camera.main.transform;

            this.Entities.With( this.eq )
                .ForEach(
                    ( ref Translation pos, ref Rotation rot, ref MoveHandlingData handler ) =>
                    {

                        ref var acts = ref handler.ControlAction;


                        tfCam.rotation = acts.LookRotation;

                        //var camz = 2.5f - math.abs( acts.VerticalAngle ) / math.radians( 90.0f ) * 1.5f;
                        var camz = 2.5f + math.min( 0.0f, acts.VerticalAngle ) / math.radians( 90.0f ) * 1.5f;
                        var camOffset = new float3( 0.0f, 0.4f, -camz );

                        tfCam.position =
                            //pos.Value + new float3( 0.0f, 0.8f - 0.43f, 0.0f ) + math.mul( acts.LookRotation, camOffset );
                            pos.Value + new float3( 0.0f, 0.8f, 0.0f ) + math.mul( acts.LookRotation, camOffset );
                    }
                );

            //var buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();//
            //var CollisionWorld = buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;//
            //this.Entities.With( this.eq )
            //    .ForEach(
            //        ( Entity entity, ref Translation pos, ref Rotation rot, ref GroundHitSphereData sphere, ref GroundHitResultData ground ) =>
            //        {
            //            var rtf = new RigidTransform( rot.Value, pos.Value );

            //            var hitInput = new PointDistanceInput
            //            {
            //                Position = pos.Value,//math.transform( rtf, sphere.Center ),
            //                MaxDistance = sphere.Distance,
            //                Filter = sphere.filter,
            //            };
            //        var collector = new AnyHitExcludeSelfCollector3( sphere.Distance, entity, CollisionWorld.Bodies );
            //            var isHit = CollisionWorld.CalculateDistance( hitInput, ref collector );
            //            Debug.Log( collector.NumHits );
            //            ground.IsGround = collector.NumHits > 0;//isHit;
            //        }
            //    );
        }


        public struct AnyHitExcludeSelfCollector3 : ICollector<DistanceHit>
        {
            public bool EarlyOutOnFirstHit => false;//{ get; private set; }
            public float MaxFraction { get; private set; }
            public int NumHits { get; private set; }

            NativeSlice<RigidBody> rigidbodies;
            Entity self;

            public AnyHitExcludeSelfCollector3
                ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
            {
                MaxFraction = maxFraction;
                this.rigidbodies = rigidbodies;
                this.self = selfEntity;
                this.NumHits = 0;
            }

            public bool AddHit( DistanceHit hit )
            {
                //Debug.Log( $"{hit.RigidBodyIndex} {this.rigidbodies[ hit.RigidBodyIndex ].Entity}" );
                //this.MaxFraction = hit.Fraction;
                //if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return true;
                //this.NumHits++;
                return true;
            }

            public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey ) { Debug.Log( $"a" ); }
            public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
            {
                //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
                if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;
                this.NumHits++;
            }
        }
    }
}
