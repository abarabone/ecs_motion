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
using UnityEngine.Assertions;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abss.Misc;
using Abss.Utilities;
using Abss.Character;
using Abss.SystemGroup;

namespace Abss.Character
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class IsGroundAroundSystem : JobComponentSystem
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }
        

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new IsGroundAroundJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }




        [BurstCompile]
        struct IsGroundAroundJob : IJobForEachWithEntity
            <GroundHitResultData, GroundHitSphereData, Translation, Rotation>
        {

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [WriteOnly] ref GroundHitResultData ground,
                [ReadOnly] ref GroundHitSphereData sphere,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {
                //var a = new NativeList<DistanceHit>( Allocator.Temp );
                var rtf = new RigidTransform( rot.Value, pos.Value );

                var hitInput = new PointDistanceInput
                {
                    Position = math.transform( rtf, sphere.Center ),
                    MaxDistance = sphere.Distance,
                    Filter = sphere.Filter,
                };
                //var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );// 自身のコライダを除外できればシンプルになるんだが…

                var collector = new AnyDistanceHitExcludeSelfCollector( sphere.Distance, entity, CollisionWorld.Bodies );
                var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref collector );

                //var castInput = new RaycastInput
                //{
                //    Start = math.transform( rtf, sphere.Center ),
                //    End = math.transform( rtf, sphere.Center ) + ( math.up() * -sphere.Distance ),
                //    Filter = sphere.filter,
                //};
                //var collector = new AnyHitExcludeSelfCollector2( 1.0f, entity, this.CollisionWorld.Bodies );
                //var isHit = this.CollisionWorld.CastRay( castInput, ref collector );

                //ground = new GroundHitResultData { IsGround = ( isHit && a.Length > 1 ) };
                //ground.IsGround = a.Length > 1;
                ground.IsGround = collector.NumHits > 0;
                //ground.IsGround = isHit;

                //a.Dispose();
            }
        }


    }

    public struct AnyDistanceHitExcludeSelfCollector : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        public AnyDistanceHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( DistanceHit hit )
        {
            //this.MaxFraction = hit.Fraction;
            //if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return true;
            //this.NumHits++;
            return true;
        }

        public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey ) { }
        public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        {
            //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
            if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;
            this.NumHits++;
        }
    }
    public struct AnyRayHitExcludeSelfCollector : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => true;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        public AnyRayHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( RaycastHit hit )
        {
            //if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            //this.MaxFraction = hit.Fraction;
            //this.NumHits++;
            return true;
        }

        public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey ) { }
        public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        {
            //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
            if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;
            this.NumHits++;
        }
    }
}
