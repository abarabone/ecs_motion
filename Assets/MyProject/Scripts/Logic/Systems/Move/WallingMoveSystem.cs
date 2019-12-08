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
    /// 
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

                var up = math.mul( rot.Value, math.up() );
                var fwd = math.forward( rot.Value );

                switch( walling.State )
                {
                    case 0:
                    {
                        var move = fwd * ( this.DeltaTime * 13.0f );
                        var fwdRay = move + fwd * sphere.Distance*2;

                        var isHit = raycastHitToWall_(
                            ref pos.Value, ref rot.Value,
                            pos.Value, fwdRay, sphere.Distance, up, entity, sphere.Filter );

                        if( isHit ) return;
                        pos.Value += move;
                    }
                    {
                        //var move = fwd * ( this.DeltaTime * 13.0f );
                        var movedPos = pos.Value;// + move;
                        var underRay = up * -( sphere.Distance * 1.5f );

                        var isHit = raycastHitToWall_(
                            ref pos.Value, ref rot.Value,
                            movedPos, underRay, sphere.Distance, fwd, entity, sphere.Filter );

                        if( isHit ) return;
                        walling.State++;
                    }
                    break;
                    case 1:
                    {
                        var move = up * -sphere.Distance;
                        var movedPos = pos.Value + move;
                        var backRay = fwd * -( sphere.Distance * 1.5f );

                        var isHit = raycastHitToWall_(
                            ref pos.Value, ref rot.Value,
                            movedPos, backRay, sphere.Distance, -up, entity, sphere.Filter );

                        if( isHit ) { walling.State = 0; return; }
                        walling.State++;
                    }
                    break;
                }

            }

            bool raycastHitToWall_(
                ref float3 pos, ref quaternion rot,
                float3 origin, float3 gndray, float bodySize, float3 fwddir,
                Entity ent, CollisionFilter filter
            )
            {

                var (isHit, hit) = raycast( ref this.CollisionWorld, origin, gndray, ent, filter );

                if( isHit )
                {
                    var (newpos, newrot) = caluclateWallPosture
                        ( origin, hit.Position, hit.SurfaceNormal, fwddir, bodySize );

                    pos = newpos;
                    rot = newrot;
                }

                return isHit;


                (bool isHit, RaycastHit hit) raycast
                    ( ref CollisionWorld cw, float3 origin_, float3 ray_, Entity ent_, CollisionFilter filter_ )
                {
                    var hitInput = new RaycastInput
                    {
                        Start = origin_,
                        End = origin_ + ray_,
                        Filter = filter_,
                    };
                    //var collector = new ClosestRayHitExcludeSelfCollector( 1.0f, ent, this.CollisionWorld.Bodies );
                    var collector = new ClosestHitCollector<RaycastHit>( 1.0f );
                    /*var isHit = */
                    cw.CastRay( hitInput, ref collector );

                    return (collector.NumHits > 0, collector.ClosestHit);
                }

                (float3 newpos, quaternion newrot) caluclateWallPosture
                    ( float3 o, float3 p, float3 n, float3 up, float r )
                {
                    var f = p - o;
                    var w = f - math.dot( f, n ) * n;

                    var upsign = math.sign( math.dot( up, w ) );
                    var newfwd = math.select( up, math.normalize( w * upsign ), math.lengthsq( w ) > 0.001f );
                    //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize( w * math.sign( math.dot( up, w ) ) ) : up;
                    var newpos = p + n * r;
                    var newrot = quaternion.LookRotation( newfwd, n );

                    return (newpos, newrot);
                }
            }
        }


    }
}
