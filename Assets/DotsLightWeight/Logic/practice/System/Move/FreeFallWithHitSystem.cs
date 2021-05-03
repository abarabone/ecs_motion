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
using System.Runtime.CompilerServices;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsLite.Character
{
    using DotsLite.Dependency;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Geometry;
    using DotsLite.Physics;
    using DotsLite.Model;
    using DotsLite.Collision;


    static class FreeFallWithHitSystemBringYourOwnDelegate
    {
        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8>
            (
                T0 t0, T1 t1,
                in T2 t2, in T3 t3, in T4 t4,
                ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8
            );

        // Declare the function overload
        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8>
            (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8> codeToRun)
            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
        =>
            LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    }

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    public class FreeFallWithHitSystem : DependencyAccessableSystemBase
    {

        PhysicsHitDependency.Sender phydep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.phydep = PhysicsHitDependency.Sender.Create(this);
        }


        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();


            var rdt = 1.0f / this.Time.DeltaTime;
            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var collisionWorld = phyScope.PhysicsWorld.CollisionWorld;

            this.Entities
                .WithBurst()
                .WithReadOnly(targets)
                .WithReadOnly(collisionWorld)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in GroundHitWallingData walling,
                        in Control.StateLinkData slink,
                        in PhysicsMass mass,
                        ref WallHitResultData result,
                        ref Translation pos,
                        ref Rotation rot,
                        ref PhysicsVelocity v
                    )
                =>
                    {
                        var up = math.mul(rot.Value, math.up());
                        var center = pos.Value + walling.CenterHeight * up;

                        var hitInput = new PointDistanceInput
                        {
                            Position = center,
                            MaxDistance = walling.HangerRange,
                            Filter = walling.Filter,
                        };
                        //var collector = new ClosestTargetedHitExcludeSelfCollector<DistanceHit>(1.0f, entity, targets);
                        var collector = new ClosestTargetedHitExcludeSelfCollector<DistanceHit>(walling.HangerRange, slink.StateEntity, targets);
                        var isHit = collisionWorld.CalculateDistance(hitInput, ref collector);

                        if (!isHit) return;


                        result.IsHit = true;

                        var n = collector.ClosestHit.SurfaceNormal;
                        var p = collector.ClosestHit.Position;
                        pos.Value = p;

                        var right = math.mul(rot.Value, new float3(1.0f, 0.0f, 0.0f));
                        var forward = math.cross(right, n);
                        var safe_forward = math.select(math.forward(rot.Value), forward, math.abs(math.dot(right, n)) > math.FLT_MIN_NORMAL);
                        rot.Value = quaternion.LookRotation(safe_forward, n);

                        v.Linear *= 0.3f;
                        v.Angular *= 0.3f;

                        //var par = caluclateWallPosture(pos.Value, p, n, up);
                        //v = setResultTo(par.pos, par.rot, mass, pos, rot, rdt);
                        ////var (newpos, newrot) = caluclateWallPosture(pos.Value, p, n, up);
                        ////v = setResultTo(newpos, newrot, mass, pos, rot, rdt);
                    }
                )
                .ScheduleParallel();
        }


        //// burst でタプルが使えるようになるまでの代用
        //public struct PosAndRot
        //{
        //    public float3 pos;
        //    public quaternion rot;
        //}

        //[BurstCompile]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        ////static (float3 newpos, quaternion newrot) caluclateWallPosture
        //public static PosAndRot caluclateWallPosture
        //    (float3 o, float3 p, float3 n, float3 up)
        //{
        //    var f = p - o;
        //    var w = f - math.dot(f, n) * n;

        //    var newpos = p + w;// + n * r;

        //    var newfwd = math.normalizesafe(w, up);
        //    var newrot = quaternion.LookRotationSafe(newfwd, n);

        //    //return (newpos, newrot);
        //    return new PosAndRot { pos = newpos, rot = newrot };
        //}

        //[BurstCompile]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static PhysicsVelocity setResultTo
        //    (float3 newpos, quaternion newrot, PhysicsMass m, Translation pos, Rotation rot, float rdt)
        //{

        //    var rgtf = new RigidTransform(newrot, newpos);

        //    return PhysicsVelocity.CalculateVelocityToTarget(in m, in pos, in rot, in rgtf, rdt);
        //}


    }
}
