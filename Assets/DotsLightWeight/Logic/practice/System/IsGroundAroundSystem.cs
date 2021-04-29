﻿using System.Collections;
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

namespace DotsLite.Character
{

    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Character;
    using DotsLite.Physics;
    using DotsLite.SystemGroup;
    using DotsLite.Model;
    using DotsLite.Collision;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    public class IsGroundAroundSystem : SystemBase
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override void OnUpdate()
        {
            this.Dependency = JobHandle.CombineDependencies
                (this.Dependency, this.buildPhysicsWorldSystem.GetOutputDependency());

            var cw = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;
            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithReadOnly(targets)
                .WithReadOnly(cw)
                .ForEach(
                    (
                        Entity entity,
                        ref GroundHitResultData ground,
                        in GroundHitSphereData sphere,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        var rtf = new RigidTransform(rot.Value, pos.Value);

                        var hitInput = new PointDistanceInput
                        {
                            Position = math.transform(rtf, sphere.Center),
                            MaxDistance = sphere.Distance,
                            Filter = sphere.Filter,
                        };
                        var collector = new AnyHitExcludeSelfCollector<DistanceHit>(sphere.Distance, entity, targets);
                        var isHit = cw.CalculateDistance(hitInput, ref collector);

                        ground.IsGround = collector.NumHits > 0;
                    }
                )
                .ScheduleParallel();

            this.buildPhysicsWorldSystem.AddInputDependencyToComplete(this.Dependency);
        }
    }

}
