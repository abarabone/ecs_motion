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
    using Abarabone.Dependency;
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
    public class WallingMoveSystem : DependencyAccessableSystemBase
    {

        PhysicsHitDependencySender phydep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.phydep = PhysicsHitDependencySender.Create(this);
        }

        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();


            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);
            var physicsWorld = this.phydep.PhysicsWorld;//.CollisionWorld;
            var deltaTime = this.Time.DeltaTime;//UnityEngine.Time.fixedDeltaTime,

            //inputDeps = new HorizontalMoveJob
            //{
            //    CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            //    DeltaTime = this.Time.DeltaTime,//UnityEngine.Time.fixedDeltaTime,
            //    MainEntities = mainEntities,
            //}
            //.Schedule( this, inputDeps );


            this.Entities
                .WithBurst()
                .WithAll<WallingTag>()
                .WithNone<WallHitResultData>()
                .WithReadOnly(physicsWorld)
                .WithReadOnly(mainEntities)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in MoveHandlingData handler,
                        in GroundHitWallingData walling,
                        ref WallHangingData hanging,
                        ref Translation pos,
                        ref Rotation rot,
                        //ref PhysicsGravityFactor g,
                        ref PhysicsVelocity v
                    )
                =>
                    {
                        //if (hanging.State > WallHangingData.WallingState.none_rotating) return;
                        if (hanging.State > WallHangingData.WallingState.front_45_rotating) return;


                        var dir = hanging.getDirection(pos.Value, rot.Value);
                        var bodysize = walling.CenterHeight;


                        //var gndRange = walling.HangerRange - bodysize;// ボディ中心からだと込み入った地形で
                        //var gndpos = pos.Value + dir.gnd * -0.05f + dir.mov * 0.05f;
                        var gndRange = walling.HangerRange;
                        var gndpos = pos.Value + dir.gnd * -bodysize + dir.mov * 0.05f;


                        var gi = Walling.makeCastInput(gndpos, dir.gnd, bodysize, gndRange, walling.Filter);
                        var hitgnd = physicsWorld.raycast(gi, entity, mainEntities);

                        if (!hitgnd.isHit)
                        {
                            hanging.State++;
                            return;
                        }
                        
                        hanging.State = WallHangingData.WallingState.none_rotating;

                        var n = hitgnd.n;
                        //var f = hitgnd.p - gi.Start;
                        //var w = f - math.dot(f, n) * n;

                        var moveRange = 4.0f * deltaTime;
                        var movepos = hitgnd.p + dir.gnd * -0.05f;//bodysize;// ボディだとワープした感じに見えてしまう
                        var movedir = math.cross(dir.right, n);//math.normalizesafe(w, dir.mov);

                        var mi = Walling.makeCastInput(movepos, movedir, bodysize, moveRange + bodysize, walling.Filter);
                        var hitmov = physicsWorld.raycast(mi, entity, mainEntities);

                        if (!hitmov.isHit)
                        {
                            var newposrot_ = new Walling.PosAndRot
                            {
                                pos = hitgnd.p + movedir * moveRange,// + hitgnd.n * 0.1f,
                                rot = quaternion.LookRotation(movedir, n),
                            };
                            newposrot_.setResultTo(ref pos, ref rot, ref v, deltaTime);

                            return;
                        }

                        //var newposrot = Walling.caluclateWallPosture
                        //    (pos.Value, hitmov.p, hitmov.n, hitgnd.n, dir.right, bodysize);
                        var walldir = math.cross(dir.right, hitmov.n);
                        var newposrot = new Walling.PosAndRot
                        {
                            pos = hitmov.p,// + movedir * moveRange,
                            rot = quaternion.LookRotation(walldir, hitmov.n),
                        };
                        newposrot.setResultTo(ref pos, ref rot, ref v, deltaTime);
                    }
                )
                .ScheduleParallel();
        }
    }

    static class Walling
    {
        // burst でタプルが使えるようになるまでの代用
        public struct PosAndRot
        {
            public float3 pos;
            public quaternion rot;
        }

        public struct HitFlagAndResult
        {
            public bool isHit;
            //public RaycastHit hit;
            public float3 n;
            public float3 p;
        }

        //public struct PostureResult
        //{
        //    public float3 pos;
        //    public quaternion rot;
        //    public float3 linear;
        //    public float3 angular;
        //}

        public struct DirectionUnit
        {
            public float3 gnd;
            public float3 mov;
            public float3 right;
        }


        public static DirectionUnit getDirection(this WallHangingData walling, float3 pos, quaternion rot) =>
            walling.State switch
            {
                WallHangingData.WallingState.none_rotating => new DirectionUnit
                {
                    gnd = math.mul(rot, new float3(0, -1, 0)),
                    mov = math.forward(rot),
                    right = math.mul(rot, new float3(1, 0, 0)),
                },
                WallHangingData.WallingState.front_45_rotating => new DirectionUnit
                {
                    gnd = math.mul(rot, new float3(0, 0, -1)),
                    mov = math.mul(rot, new float3(0, -1, 0)),
                    right = math.mul(rot, new float3(1, 0, 0)),
                },
                _ => default,
            };

        public static RaycastInput makeCastInput
            (float3 pos, float3 dir, float bodySize, float moveRange, CollisionFilter filter)
        {
            var gndst = pos;// + dir * -bodySize;
            var gnded = gndst + dir * moveRange;
            return new RaycastInput
            {
                Start = gndst,
                End = gnded,
                Filter = filter,
            };
        }

        public static void setResultTo
            (this PosAndRot newposrot, ref Translation pos, ref Rotation rot, ref PhysicsVelocity v, float dt)
        {

            var rdt = math.rcp(dt);
            var linear = (newposrot.pos - pos.Value) * rdt;

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

            //pos.Value = newposrot.pos;
            rot.Value = newposrot.rot;
            v.Linear = linear;
            v.Angular = 0;//
        }

        //( bool isHit, RaycastHit hit) raycast
        public static HitFlagAndResult raycast
            (ref this PhysicsWorld pw, RaycastInput hitInput, Entity self, ComponentDataFromEntity<Bone.MainEntityLinkData> mainlist)
        {
            var collector = new ClosestHitExcludeSelfCollector<RaycastHit>(maxFraction: 1.0f, self, mainlist);

            var isHit = pw.CastRay(hitInput, ref collector);

            return new HitFlagAndResult
            {
                isHit = collector.NumHits > 0,
                p = collector.ClosestHit.Position,
                n = collector.ClosestHit.SurfaceNormal,
                //hit = collector.ClosestHit
            };
        }


        //( float3 newpos, quaternion newrot) caluclateWallPosture
        public static PosAndRot caluclateWallPosture
            (float3 o, float3 p, float3 n, float3 up, float3 right, float r)
        {
            var f = p - o;
            var w = f - math.dot(f, n) * n;

            var newpos = p + w;// + n * r;

            var newfwd = math.normalizesafe(w, up);
            var newrot = quaternion.LookRotationSafe(newfwd, n);

            //return (newpos, newrot);
            return new PosAndRot { pos = newpos, rot = newrot };
        }
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

    //}
}
