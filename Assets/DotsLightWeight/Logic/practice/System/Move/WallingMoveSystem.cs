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
    using Abarabone.Common;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Geometry;
    using Abarabone.Physics;
    using Abarabone.CharacterMotion;
    using Abarabone.Model;


    static class BringYourOwnDelegate
    {
        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7>
            (
                T0 t0, T1 t1,
                in T2 t2, in T3 t3,
                ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7
            );

        // Declare the function overload
        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7>
            (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7> codeToRun)
            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
        =>
            LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    }

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class WallingMoveSystem : PhysicsHitSystemBase
    {

        protected override void OnUpdateWith(BuildPhysicsWorld physicsBuilder)
        {
            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);
            var collisionWorld = physicsBuilder.PhysicsWorld;//.CollisionWorld;
            var deltaTime = this.Time.DeltaTime;//UnityEngine.Time.fixedDeltaTime,

            //inputDeps = new HorizontalMoveJob
            //{
            //    CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            //    DeltaTime = this.Time.DeltaTime,//UnityEngine.Time.fixedDeltaTime,
            //    MainEntities = mainEntities,
            //}
            //.Schedule( this, inputDeps );


            this.Entities
                .WithoutBurst()
                .WithAll<WallingTag>()
                .WithNone<WallHitResultData>()
                .WithReadOnly(collisionWorld)
                .WithReadOnly(mainEntities)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in MoveHandlingData handler,
                        in GroundHitSphereData sphere,
                        ref WallHunggingData walling,
                        ref Translation pos,
                        ref Rotation rot,
                        //ref PhysicsGravityFactor g,
                        ref PhysicsVelocity v
                    )
                =>
                    {
                        //var v = default(PhysicsVelocity);
                        var up = math.mul(rot.Value, math.up());
                        var fwd = math.forward(rot.Value);


                        


                        switch (walling.State)
                        {
                            case 0:
                                {
                                    var move = fwd * (deltaTime * 13.0f);
                                    var fwdRay = move + fwd * sphere.Distance * 1.5f;

                                    var isHit = raycastHitToWall_(
                                        ref v, ref pos.Value, ref rot.Value, deltaTime,
                                        pos.Value, fwdRay, sphere.Distance, up, entity, sphere.Filter, mainEntities);

                                    if (isHit) break;
                                    //pos.Value += move;
                                }
                                {
                                    var move = fwd * (deltaTime * 13.0f);
                                    var movedPos = pos.Value + move;
                                    var underRay = up * -(sphere.Distance * 1.5f);

                                    var isHit = raycastHitToWall_(
                                        ref v, ref pos.Value, ref rot.Value, deltaTime,
                                        movedPos, underRay, sphere.Distance, fwd, entity, sphere.Filter, mainEntities);

                                    if (isHit) break;
                                    v.Linear = move * math.rcp(deltaTime);
                                    walling.State++;
                                }
                                break;
                            case 1:
                                {
                                    var move = up * -sphere.Distance;
                                    var movedPos = pos.Value + move;
                                    var backRay = fwd * -(sphere.Distance * 1.5f);

                                    var isHit = raycastHitToWall_(
                                        ref v, ref pos.Value, ref rot.Value, deltaTime,
                                        movedPos, backRay, sphere.Distance, -up, entity, sphere.Filter, mainEntities);

                                    if (isHit) { walling.State = 0; return; }
                                    walling.State++;
                                }
                                break;
                        }

                        //v.Linear = 0;//
                        //v.Angular = float3.zero;//
                    }
                )
                .ScheduleParallel();
                

            bool raycastHitToWall_
                (
                    ref PhysicsVelocity v, ref float3 pos, ref quaternion rot, float dt,
                    float3 origin, float3 gndray, float bodySize, float3 fwddir,
                    Entity ent, CollisionFilter filter,
                    ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntities
                )
            {
                //var (isHit, hit) = raycast( ref this.CollisionWorld, origin, gndray, ent, filter );
                var h = raycast(ref collisionWorld, origin, gndray, ent, filter, mainEntities);

                if (h.isHit)
                {
                    //var (newpos, newrot) = caluclateWallPosture
                    var newposrot = caluclateWallPosture
                        (origin, h.hit.Position, h.hit.SurfaceNormal, fwddir, bodySize);

                    var rdt = math.rcp(dt);
                    v.Linear = (newposrot.pos - pos) * rdt;
                    pos = newposrot.pos;

                    //var invprev = math.inverse(newposrot.rot);
                    //var drot = math.mul(invprev, rot);
                    //var angle = math.acos(drot.value.w) * 2.0f;
                    //var sin = math.sin(angle);
                    //var axis = drot.value.As_float3() * math.rcp(sin);
                    var invprev = math.inverse(newposrot.rot);
                    var drot = math.mul(invprev, rot);
                    var axis = drot.value.As_float3();
                    var angle = math.lengthsq(drot);
                    v.Angular = axis * (angle * rdt);
                    //v.Angular = //float3.zero;
                    rot = newposrot.rot;
                }

                return h.isHit;


                //( bool isHit, RaycastHit hit) raycast
                HitFlagAndResult raycast
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
                    var collector = new ClosestHitExcludeSelfCollector<RaycastHit>(1.0f, ent, mainEntities_);
                    //var collector = new ClosestHitCollector<RaycastHit>( 1.0f );
                    /*var isHit = */
                    cw.CastRay(hitInput, ref collector);

                    return new HitFlagAndResult { isHit = collector.NumHits > 0, hit = collector.ClosestHit };
                    //return (collector.NumHits > 0, collector.ClosestHit);
                }

                //( float3 newpos, quaternion newrot) caluclateWallPosture
                PosAndRot caluclateWallPosture
                    (float3 o, float3 p, float3 n, float3 up, float r)
                {
                    var f = p - o;
                    var w = f - math.dot(f, n) * n;

                    var upsign = math.sign(math.dot(up, w));
                    var newfwd = math.select(up, math.normalize(w * upsign), math.lengthsq(w) > 0.001f);
                    //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize( w * math.sign( math.dot( up, w ) ) ) : up;
                    var newpos = p + n * r;
                    var newrot = quaternion.LookRotation(newfwd, n);

                    return new PosAndRot { pos = newpos, rot = newrot };
                    //return (newpos, newrot);
                }
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
            public float3 n;
            public float3 p;
        }

        struct HitFlagAndResult2
        {
            public bool isHit;
            public float3 pos;
            public quaternion rot;
            public float3 linear;
            public float3 angular;
        }


        public enum WallState
        {
            stand,
            front,
        }
        static RaycastInput makeGroundInput(WallState state, float3 pos, quaternion rot) =>
            state switch
            {
                WallState.stand => new RaycastInput
                {
                    //Start = origin,
                    //End = origin + ray,
                    //Filter = filter,
                },
                WallState.front => new RaycastInput
                {
                    //Start = origin,
                    //End = origin + ray,
                    //Filter = filter,
                },
            };
        static RaycastInput makeMoveInput(WallState state, float3 pos, quaternion rot) =>
            state switch
            {
                WallState.stand => new RaycastInput
                {
                    //Start = origin,
                    //End = origin + ray,
                    //Filter = filter,
                },
                WallState.front => new RaycastInput
                {
                    //Start = origin,
                    //End = origin + ray,
                    //Filter = filter,
                },
            };


        HitFlagAndResult2 raycastHitToWall
            (
                ref PhysicsWorld pw, WallState state,
                float3 pos, quaternion rot, Entity self, ComponentDataFromEntity<Bone.MainEntityLinkData> mainlist, float bodySize, float dt
            )
        {

            var gi = makeGroundInput(state, pos, rot);
            var hitgnd = raycast(ref pw, gi, self, mainlist);

            if (!hitgnd.isHit) return new HitFlagAndResult2 { isHit = false };


            var mi = makeMoveInput(state, hitgnd.p, rot);
            var hitmov = raycast(ref pw, mi, self, mainlist);

            if (!hitmov.isHit) return new HitFlagAndResult2 { isHit = false };

            var up = ;
            var newposrot = caluclateWallPosture
                (mi.Start, hitmov.p, hitmov.n, up, bodySize);

            var rdt = math.rcp(dt);
            var linear = (newposrot.pos - pos) * rdt;

            //var invprev = math.inverse(newposrot.rot);
            //var drot = math.mul(invprev, rot);
            //var angle = math.acos(drot.value.w) * 2.0f;
            //var sin = math.sin(angle);
            //var axis = drot.value.As_float3() * math.rcp(sin);
            //var invprev = math.inverse(newposrot.rot);
            //var drot = math.mul(invprev, rot);
            //var axis = drot.value.As_float3();
            //var angle = math.lengthsq(drot);
            //var angular = axis * (angle * rdt);

            return new HitFlagAndResult2
            {
                isHit = true,
                pos = newposrot.pos,
                rot = newposrot.rot,
                linear = linear,
            };
        }

        //( bool isHit, RaycastHit hit) raycast
        static HitFlagAndResult raycast
            (ref PhysicsWorld pw, RaycastInput hitInput, Entity self, ComponentDataFromEntity<Bone.MainEntityLinkData> mainlist)
        {
            var collector = new ClosestHitExcludeSelfCollector<RaycastHit>(maxFraction: 1.0f, self, mainlist);

            var isHit = pw.CastRay(hitInput, ref collector);

            return new HitFlagAndResult { isHit = collector.NumHits > 0, hit = collector.ClosestHit };
        }


        //( float3 newpos, quaternion newrot) caluclateWallPosture
        PosAndRot caluclateWallPosture
            (float3 o, float3 p, float3 n, float3 up, float r)
        {
            var f = p - o;
            var w = f - math.dot(f, n) * n;

            var newpos = w + n * r;

            var newfwd = math.select(up, math.normalize(w), math.dot(n, up) > math.FLT_MIN_NORMAL);// 壁に垂直侵入した場合、up となる
            var newrot = quaternion.LookRotation(newfwd, n);

            //return (newpos, newrot);
            return new PosAndRot { pos = newpos, rot = newrot };
        }


        //[BurstCompile]
        //[RequireComponentTag(typeof(WallingTag)), ExcludeComponent(typeof(WallHitResultData))]
        //struct HorizontalMoveJob : IJobForEachWithEntity
        //    <WallHunggingData, MoveHandlingData, GroundHitSphereData, Translation, Rotation, PhysicsVelocity>
        //{

        //    [ReadOnly] public float DeltaTime;

        //    [ReadOnly] public PhysicsWorld CollisionWorld;

        //    [ReadOnly] public ComponentDataFromEntity<Bone.MainEntityLinkData> MainEntities;


        //    public unsafe void Execute(
        //        Entity entity, int index,
        //        ref WallHunggingData walling,
        //        [ReadOnly] ref MoveHandlingData handler,
        //        [ReadOnly] ref GroundHitSphereData sphere,
        //        //[ReadOnly] ref Translation pos,
        //        //[ReadOnly] ref Rotation rot,
        //        ref Translation pos,
        //        ref Rotation rot,
        //                        ref PhysicsVelocity v//, ref PhysicsGravityFactor g
        //    )
        //    {
        //        //var v = default(PhysicsVelocity);
        //        var up = math.mul( rot.Value, math.up() );
        //        var fwd = math.forward( rot.Value );

        //        switch( walling.State )
        //        {
        //            case 0:
        //            {
        //                var move = fwd * ( this.DeltaTime * 13.0f );
        //                var fwdRay = move + fwd * sphere.Distance * 1.5f;

        //                var isHit = raycastHitToWall_(
        //                    ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
        //                    pos.Value, fwdRay, sphere.Distance, up, entity, sphere.Filter, this.MainEntities);

        //                if( isHit ) break;
        //                //pos.Value += move;
        //            }
        //            {
        //                var move = fwd * ( this.DeltaTime * 13.0f );
        //                var movedPos = pos.Value + move;
        //                var underRay = up * -( sphere.Distance * 1.5f );

        //                var isHit = raycastHitToWall_(
        //                    ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
        //                    movedPos, underRay, sphere.Distance, fwd, entity, sphere.Filter, this.MainEntities);

        //                if( isHit ) break;
        //                v.Linear = move * math.rcp( this.DeltaTime );
        //                walling.State++;
        //            }
        //            break;
        //            case 1:
        //            {
        //                var move = up * -sphere.Distance;
        //                var movedPos = pos.Value + move;
        //                var backRay = fwd * -( sphere.Distance * 1.5f );

        //                var isHit = raycastHitToWall_(
        //                    ref v, ref pos.Value, ref rot.Value, this.DeltaTime,
        //                    movedPos, backRay, sphere.Distance, -up, entity, sphere.Filter, this.MainEntities );

        //                if( isHit ) { walling.State = 0; return; }
        //                walling.State++;
        //            }
        //            break;
        //        }

        //        //v.Linear = 0;//
        //        //v.Angular = float3.zero;//
        //    }

        //    [BurstCompile]
        //    bool raycastHitToWall_
        //        (
        //            ref PhysicsVelocity v, ref float3 pos, ref quaternion rot, float dt,
        //            float3 origin, float3 gndray, float bodySize, float3 fwddir,
        //            Entity ent, CollisionFilter filter,
        //            ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntities
        //        )
        //    {

        //        var h = raycast( ref this.CollisionWorld, origin, gndray, ent, filter, mainEntities );
        //        //var (isHit, hit) = raycast( ref this.CollisionWorld, origin, gndray, ent, filter );

        //        if( h.isHit )
        //        {
        //            var newposrot = caluclateWallPosture
        //            //var (newpos, newrot) = caluclateWallPosture
        //                ( origin, h.hit.Position, h.hit.SurfaceNormal, fwddir, bodySize );

        //            var rdt = math.rcp( dt );
        //            v.Linear = ( newposrot.pos - pos ) * rdt;
        //            //pos = newposrot.pos;

        //            //var invprev = math.inverse( newposrot.rot );
        //            //var drot = math.mul( invprev, rot );
        //            //var angle = math.acos( drot.value.w ) * 2.0f;
        //            //var sin = math.sin( angle );
        //            //var axis = drot.value.As_float3() * math.rcp( sin );
        //            //var invprev = math.inverse( newposrot.rot );
        //            //var drot = math.mul( invprev, rot );
        //            //var axis = drot.value.As_float3();
        //            //var angle = math.lengthsq( drot );
        //            //v.Angular = axis * ( angle * rdt );
        //            v.Angular = float3.zero;
        //            rot = newposrot.rot;
        //        }

        //        return h.isHit;


        //        HitFlagAndResult raycast
        //        //( bool isHit, RaycastHit hit) raycast
        //            (
        //                ref PhysicsWorld cw, float3 origin_, float3 ray_, Entity ent_, CollisionFilter filter_,
        //                ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntities_
        //            )
        //        {
        //            var hitInput = new RaycastInput
        //            {
        //                Start = origin_,
        //                End = origin_ + ray_,
        //                Filter = filter_,
        //            };
        //            var collector = new ClosestHitExcludeSelfCollector<RaycastHit>( 1.0f, ent, mainEntities_ );
        //            //var collector = new ClosestHitCollector<RaycastHit>( 1.0f );
        //            /*var isHit = */
        //            cw.CastRay( hitInput, ref collector );

        //            return new HitFlagAndResult { isHit=collector.NumHits > 0, hit=collector.ClosestHit };
        //            //return (collector.NumHits > 0, collector.ClosestHit);
        //        }

        //        PosAndRot caluclateWallPosture
        //        //( float3 newpos, quaternion newrot) caluclateWallPosture
        //            ( float3 o, float3 p, float3 n, float3 up, float r )
        //        {
        //            var f = p - o;
        //            var w = f - math.dot( f, n ) * n;

        //            var upsign = math.sign( math.dot( up, w ) );
        //            var newfwd = math.select( up, math.normalize( w * upsign ), math.lengthsq( w ) > 0.001f );
        //            //var newfwd = math.lengthsq(w) > 0.001f ? math.normalize( w * math.sign( math.dot( up, w ) ) ) : up;
        //            var newpos = p + n * r;
        //            var newrot = quaternion.LookRotation( newfwd, n );

        //            return new PosAndRot { pos = newpos, rot = newrot };
        //            //return (newpos, newrot);
        //        }
        //    }
        //    // burst でタプルが使えるようになるまでの代用
        //    struct PosAndRot
        //    {
        //        public float3 pos;
        //        public quaternion rot;
        //    }
        //    struct HitFlagAndResult
        //    {
        //        public bool isHit;
        //        public RaycastHit hit;
        //    }
        //}


    }
}
