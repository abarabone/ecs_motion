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

namespace Abarabone.Character
{

    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Geometry;
    using Abarabone.Physics;
    using Abarabone.Motion;
    using Abarabone.Model;


    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    //[UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class WallingMoveSystem : JobComponentSystem
    {


        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);

            inputDeps = new HorizontalMoveJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
                DeltaTime = this.Time.DeltaTime,//UnityEngine.Time.fixedDeltaTime,
                MainEntities = mainEntities,
            }
            .Schedule( this, inputDeps );
            
            return inputDeps;
        }



        [BurstCompile]
        [RequireComponentTag(typeof(WallingTag)), ExcludeComponent(typeof(WallHitResultData))]
        struct HorizontalMoveJob : IJobForEachWithEntity
            <WallHunggingData, MoveHandlingData, GroundHitSphereData, Translation, Rotation, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public PhysicsWorld CollisionWorld;

            [ReadOnly] public ComponentDataFromEntity<Bone.MainEntityLinkData> MainEntities;


            public unsafe void Execute(
                Entity entity, int index,
                ref WallHunggingData walling,
                [ReadOnly] ref MoveHandlingData handler,
                [ReadOnly] ref GroundHitSphereData sphere,
                //[ReadOnly] ref Translation pos,
                //[ReadOnly] ref Rotation rot,
                ref Translation pos,
                ref Rotation rot,
                                ref PhysicsVelocity v//, ref PhysicsGravityFactor g
            )
            {
                //var v = default(PhysicsVelocity);
                var up = math.mul( rot.Value, math.up() );
                var fwd = math.forward( rot.Value );

                switch( walling.State )
                {
                    case 0:
                    {
                        var move = fwd * ( this.DeltaTime * 13.0f );
                        var fwdRay = move + fwd * sphere.Distance * 1.5f;

                        var isHit = raycastHitToWall_(
                            ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
                            pos.Value, fwdRay, sphere.Distance, up, entity, sphere.Filter, this.MainEntities);

                        if( isHit ) break;
                        //pos.Value += move;
                    }
                    {
                        var move = fwd * ( this.DeltaTime * 13.0f );
                        var movedPos = pos.Value + move;
                        var underRay = up * -( sphere.Distance * 1.5f );

                        var isHit = raycastHitToWall_(
                            ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
                            movedPos, underRay, sphere.Distance, fwd, entity, sphere.Filter, this.MainEntities);

                        if( isHit ) break;
                        v.Linear = move * math.rcp( this.DeltaTime );
                        walling.State++;
                    }
                    break;
                    case 1:
                    {
                        var move = up * -sphere.Distance;
                        var movedPos = pos.Value + move;
                        var backRay = fwd * -( sphere.Distance * 1.5f );

                        var isHit = raycastHitToWall_(
                            ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
                            movedPos, backRay, sphere.Distance, -up, entity, sphere.Filter, this.MainEntities );

                        if( isHit ) { walling.State = 0; return; }
                        walling.State++;
                    }
                    break;
                }

                //v.Linear = 0;//
                //v.Angular = float3.zero;//
            }

            [BurstCompile]
            bool raycastHitToWall_
                (
                    ref PhysicsVelocity v, ref float3 pos, ref quaternion rot, float dt,
                    float3 origin, float3 gndray, float bodySize, float3 fwddir,
                    Entity ent, CollisionFilter filter,
                    ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntities
                )
            {

                var h = raycast( ref this.CollisionWorld, origin, gndray, ent, filter, mainEntities );
                //var (isHit, hit) = raycast( ref this.CollisionWorld, origin, gndray, ent, filter );

                if( h.isHit )
                {
                    var newposrot = caluclateWallPosture
                    //var (newpos, newrot) = caluclateWallPosture
                        ( origin, h.hit.Position, h.hit.SurfaceNormal, fwddir, bodySize );

                    var rdt = math.rcp( dt );
                    v.Linear = ( newposrot.pos - pos ) * rdt;
                    //pos = newposrot.pos;

                    //var invprev = math.inverse( newposrot.rot );
                    //var drot = math.mul( invprev, rot );
                    //var angle = math.acos( drot.value.w ) * 2.0f;
                    //var sin = math.sin( angle );
                    //var axis = drot.value.As_float3() * math.rcp( sin );
                    //var invprev = math.inverse( newposrot.rot );
                    //var drot = math.mul( invprev, rot );
                    //var axis = drot.value.As_float3();
                    //var angle = math.lengthsq( drot );
                    //v.Angular = axis * ( angle * rdt );
                    v.Angular = float3.zero;
                    rot = newposrot.rot;
                }

                return h.isHit;


                HitFlagAndResult raycast
                //( bool isHit, RaycastHit hit) raycast
                    (
                        ref PhysicsWorld cw, float3 origin_, float3 ray_, Entity ent_, CollisionFilter filter_,
                        ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntities_
                    )
                {
                    var hitInput = new RaycastInput
                    {
                        Start = origin_,
                        End = origin_ + ray_,
                        Filter = filter_,
                    };
                    var collector = new ClosestHitExcludeSelfCollector<RaycastHit>( 1.0f, ent, mainEntities_ );
                    //var collector = new ClosestHitCollector<RaycastHit>( 1.0f );
                    /*var isHit = */
                    cw.CastRay( hitInput, ref collector );

                    return new HitFlagAndResult { isHit=collector.NumHits > 0, hit=collector.ClosestHit };
                    //return (collector.NumHits > 0, collector.ClosestHit);
                }

                PosAndRot caluclateWallPosture
                //( float3 newpos, quaternion newrot) caluclateWallPosture
                    ( float3 o, float3 p, float3 n, float3 up, float r )
                {
                    var f = p - o;
                    var w = f - math.dot( f, n ) * n;

                    var upsign = math.sign( math.dot( up, w ) );
                    var newfwd = math.select( up, math.normalize( w * upsign ), math.lengthsq( w ) > 0.001f );
                    //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize( w * math.sign( math.dot( up, w ) ) ) : up;
                    var newpos = p + n * r;
                    var newrot = quaternion.LookRotation( newfwd, n );

                    return new PosAndRot { pos = newpos, rot = newrot };
                    //return (newpos, newrot);
                }
            }
            // burst でタプルが使えるようになるまでの代用
            struct PosAndRot
            {
                public float3 pos;
                public quaternion rot;
            }
            struct HitFlagAndResult
            {
                public bool isHit;
                public RaycastHit hit;
            }
        }


    }
}
