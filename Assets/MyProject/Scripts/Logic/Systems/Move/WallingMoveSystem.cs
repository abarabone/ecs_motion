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



        //[BurstCompile]
        struct HorizontalMoveJob : IJobForEachWithEntity
            <WallHunggingData, MoveHandlingData, GroundHitSphereData, Translation, Rotation>//, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                ref WallHunggingData walling,
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
                
                var up = math.mul( rot.Value, math.up() );
                var fwd = math.mul( rot.Value, Vector3.forward );//math.forward( rot.Value );


                var (origin, ray, forwardWhenVirtical) = getRayParams( walling.State, pos.Value, rot.Value );
                
                var (isHit, hit) = raycast( origin, ray, entity, sphere.Filter );

                if( isHit )
                {
                    var (newpos, newrot) = caluclateWallPosture
                        ( origin, hit.Position, hit.SurfaceNormal, forwardWhenVirtical, sphere.Distance );

                    pos.Value = newpos;
                    rot.Value = newrot;
                    walling.State = ++walling.State & 3;
                    return;
                }

                return;

                switch(walling.State)
                {
                    case 0:
                    {
                        var move = fwd * ( this.DeltaTime * 3.0f );
                        var fwdRay = move + fwd * sphere.Distance;
                        var (isHit, hit) = raycast( pos.Value, fwdRay, entity, sphere.Filter );

                        if( isHit )
                        {
                            var (newpos, newrot) = caluclateWallPosture
                                ( pos.Value, hit.Position, hit.SurfaceNormal, up, sphere.Distance );

                            pos.Value = newpos;
                            rot.Value = newrot;
                            break;
                        }
                    }
                    {
                        var move = fwd * ( this.DeltaTime * 3.0f );
                        var movedPos = pos.Value + move;
                        var underRay = up * -( sphere.Distance * 1.5f );
                        var (isHit, hit) = raycast( movedPos, underRay, entity, sphere.Filter );

                        if( isHit )
                        {
                            var (newpos, newrot) = caluclateGroundPosture
                                ( movedPos, hit.Position, hit.SurfaceNormal, fwd, sphere.Distance );

                            pos.Value = newpos;
                            rot.Value = newrot;
                            break;
                        }

                        pos.Value = movedPos;
                        walling.State++;
                    }
                    break;
                    case 1:
                    {
                        var move = up * -sphere.Distance;
                        var movedPos = pos.Value + move;
                        var backRay = fwd * -( sphere.Distance * 1.5f );
                        var (isHit, hit) = raycast( movedPos, backRay, entity, sphere.Filter );

                        if( isHit )
                        {
                            var (newpos, newrot) = caluclateGroundPosture
                                ( movedPos, hit.Position, hit.SurfaceNormal, -up, sphere.Distance );

                            pos.Value = newpos;
                            rot.Value = newrot;
                            walling.State = 0;
                            break;
                        }

                        pos.Value = movedPos;
                        //walling.State++;
                        walling.State = 0;
                    }
                    break;
                }
            }


            (float3 origin, float3 ray, float3 forwardWhenVirtical) getRayParams
                (int state, float3 pos, quaternion rot)
            {

                var up = math.mul( rot, math.up() );
                var fwd = math.mul( rot, Vector3.forward );//math.forward( rot.Value );


                switch( state )
                {
                    case 0:
                    {
                        var move = fwd * ( this.DeltaTime * 3.0f );
                        var fwdRay = move + fwd * sphere.Distance;

                        return (pos, fwdRay, up);
                    }
                    case 1:
                    {
                        var move = fwd * ( this.DeltaTime * 3.0f );
                        var movedPos = pos.Value + move;
                        var underRay = up * -( sphere.Distance * 1.5f );

                        return (movedPos, underRay, fwd);
                    }
                    case 2:
                    {
                        var move = up * -sphere.Distance;
                        var movedPos = pos.Value + move;
                        var backRay = fwd * -( sphere.Distance * 1.5f );

                        return (movedPos, backRay, -up);
                    }
                    case 3:
                    {
                        var move = fwd * -sphere.Distance;
                        var movedPos = pos.Value + move;
                        var upRay = up * ( sphere.Distance * 1.5f );

                        return (movedPos, upRay, -fwd);
                    }
                }

                return (float3.zero, float3.zero, float3.zero);
            }


            bool isHitWallAndSetPosture(
                ref float3 pos, ref quaternion rot,
                float3 origin, float3 ray, float3 forwardWhenVirtical,
                Entity ent, CollisionFilter filter, float margin
            )
            {
                var (isHit, hit) = raycast( origin, ray, ent, filter );

                if( isHit )
                {
                    var (newpos, newrot) = caluclateGroundPosture
                        ( origin, hit.Position, hit.SurfaceNormal, forwardWhenVirtical, margin );

                    pos = newpos;
                    rot = newrot;
                }

                return isHit;
            }


            (bool isHit, RaycastHit hit) raycast
                ( float3 origin, float3 ray, Entity ent, CollisionFilter filter )
            {
                var hitInput = new RaycastInput
                {
                    Start = origin,
                    End = origin + ray,
                    Filter = filter,
                };
                //var collector = new ClosestRayHitExcludeSelfCollector( 1.0f, ent, this.CollisionWorld.Bodies );
                var collector = new ClosestHitCollector<RaycastHit>( 1.0f );
                /*var isHit = */
                this.CollisionWorld.CastRay( hitInput, ref collector );

                return (collector.NumHits > 0, collector.ClosestHit);
            }

            (float3 newpos, quaternion newrot) caluclateWallPosture
                ( float3 o, float3 p, float3 n, float3 up, float r )
            {
                var f = p - o;
                var w = f - math.dot( f, n ) * n;

                var newfwd = math.select( up, math.normalize( w * math.sign( math.dot( up, w ) ) ), math.lengthsq( w ) > 0.001 );
                //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize( w * math.sign( math.dot( up, w ) ) ) : up;
                var newpos = p + n * r;
                var newrot = quaternion.LookRotation( newfwd, n );

                return (newpos, newrot);
            }
            (float3 newpos, quaternion newrot) caluclateGroundPosture
                ( float3 o, float3 p, float3 n, float3 up, float r )
            {
                var f = p - o;
                var w = f - math.dot( f, n ) * n;

                var newfwd = math.select( up, math.normalize( w ), math.lengthsq( w ) > 0.001 );
                //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize(w) : up;
                var newpos = p + n * r;
                var newrot = quaternion.LookRotation( newfwd, n );

                return (newpos, newrot);
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

        RaycastHit currentHit;
        RaycastHit m_ClosestHit;
        public RaycastHit ClosestHit => m_ClosestHit;

        public ClosestRayHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            m_ClosestHit = default( RaycastHit );
            this.currentHit = default( RaycastHit );
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
            this.currentHit = hit;
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

            if( this.currentHit.Fraction < oldFraction )
            {
                m_ClosestHit = this.currentHit;
                m_ClosestHit.Transform( transform, rigidBodyIndex );
                MaxFraction = m_ClosestHit.Fraction;
                NumHits = 1;
            }
        }
    }
}
