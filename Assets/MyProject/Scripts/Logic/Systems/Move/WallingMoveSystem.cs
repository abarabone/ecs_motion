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
using Unity.Physics.Extensions;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Geometry;

namespace Abss.Character
{

    /// <summary>
    /// 与えられた方向を向き、与えられた水平移動をする。
    /// ジャンプが必要なら、地面と接触していればジャンプする。←暫定
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class WallingMoveSystem : JobComponentSystem
    {


        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            inputDeps = new HorizontalMoveJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                DeltaTime = Time.deltaTime,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        struct HorizontalMoveJob : IJobForEachWithEntity
            <WallHunggingData, MoveHandlingData, GroundHitSphereData, Translation, Rotation>//, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [ReadOnly] ref WallHunggingData walling,
                [ReadOnly] ref MoveHandlingData handler,
                [ReadOnly] ref GroundHitSphereData sphere,
                //[ReadOnly] ref Translation pos,
                //[ReadOnly] ref Rotation rot,
                ref Translation pos,
                ref Rotation rot//,
                //ref PhysicsVelocity v
            )
            {
                var rtf = new RigidTransform( rot.Value, pos.Value );

                var dir = math.forward( rot.Value );
                var move = dir * (this.DeltaTime * 3.0f);


                var st = math.transform( rtf, sphere.Center ) + dir * sphere.Distance * 1.01f;
                var ed = st + move;
                var hitInput = new RaycastInput
                {
                    Start = st,
                    End = ed,
                    Filter = sphere.Filter,
                };
                var collector = new ClosestRayHitExcludeSelfCollector( 1.0f, entity, this.CollisionWorld.Bodies );
                var isHit = this.CollisionWorld.CastRay( hitInput, ref collector );
                //var a = new NativeList<RaycastHit>( Allocator.Temp );
                //var isHit = this.CollisionWorld.CastRay( hitInput, out var a );

                if( collector.NumHits == 0 )
                {
                    //v.Linear = move / this.DeltaTime;
                    pos.Value += move;
                    //a.Dispose();
                    return;
                }

                //var movetowall = collector.ClosestHit.Position - pos.Value;

                var f = ed - st;
                var n = collector.ClosestHit.SurfaceNormal;
                pos.Value = collector.ClosestHit.Position;
                rot.Value = quaternion.LookRotation( math.normalize(f-math.dot(f,n)*n), n);
                //a.Dispose();
            }
        }


    }


    public struct ClosestRayHitExcludeSelfCollector : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        private RaycastHit m_ClosestHit;
        public RaycastHit ClosestHit => m_ClosestHit;

        public ClosestRayHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            m_ClosestHit = default( RaycastHit );
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( RaycastHit hit )
        {
            //if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            //this.MaxFraction = hit.Fraction;
            //this.NumHits++;
            //MaxFraction = hit.Fraction;
            m_ClosestHit = hit;
            return true;
        }

        public void TransformNewHits
            ( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey )
        {
            //if( m_ClosestHit.Fraction < oldFraction )
            //{
            //    m_ClosestHit.Transform( transform, numSubKeyBits, subKey );
            //}
        }
        public void TransformNewHits
            ( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        {
            //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
            if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;

            if( m_ClosestHit.Fraction < oldFraction )
            {
                m_ClosestHit.Transform( transform, rigidBodyIndex );
                MaxFraction = m_ClosestHit.Fraction;
                NumHits = 1;
            }
        }
    }
}
