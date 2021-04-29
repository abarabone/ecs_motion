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


    using DotsLite.CharacterMotion;

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


            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var collisionWorld = this.phydep.PhysicsWorld.CollisionWorld;

            this.Entities
                .WithBurst()
                .WithReadOnly(targets)
                .WithReadOnly(collisionWorld)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref WallHitResultData result,
                        ref Translation pos,
                        ref Rotation rot,
                        ref PhysicsVelocity v,
                        in GroundHitWallingData walling
                    )
                =>
                    {
                        //var rtf = new RigidTransform( rot.Value, pos.Value );
                        var center = pos.Value + walling.CenterHeight * math.mul(rot.Value, math.up());

                        var hitInput = new PointDistanceInput
                        {
                            Position = center,
                            MaxDistance = walling.HangerRange,//.CenterHeight,//.HangerRange,
                            Filter = walling.Filter,
                        };
                        var collector = new ClosestTargetedHitExcludeSelfCollector<DistanceHit>(1.0f, entity, targets);
                        var isHit = collisionWorld.CalculateDistance(hitInput, ref collector);

                        if (collector.NumHits <= 0) return;
                        //if (!isHit) return;


                        result.IsHit = true;

                        var n = collector.ClosestHit.SurfaceNormal;
                        var p = collector.ClosestHit.Position;
                        pos.Value = p;

                        var right = math.mul(rot.Value, new float3(1.0f, 0.0f, 0.0f));
                        var forward = math.cross(right, n);
                        var safe_forward = math.select(math.forward(rot.Value), forward, math.abs(math.dot(right, n)) > math.FLT_MIN_NORMAL);
                        rot.Value = quaternion.LookRotation(safe_forward, n);
                        //rot.Value = quaternion.LookRotationSafe(forward, n);

                        v.Linear *= 0.3f;
                        v.Angular *= 0.3f;
                    }
                )
                .ScheduleParallel();
        }

    }
}
