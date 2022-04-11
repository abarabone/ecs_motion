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
using Unity.Physics.Authoring;

using CatId = PhysicsCategoryNamesId;
using CatFlag = PhysicsCategoryNamesFlag;

namespace DotsLite.Character
{

    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Character;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Model;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.Hit))]
    public partial class IsGroundAroundSystem : SystemBase
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();

            this.RegisterPhysicsRuntimeSystemReadOnly();
        }


        protected override void OnUpdate()
        {
            //this.Dependency = JobHandle.CombineDependencies(
            //    this.Dependency, this.buildPhysicsWorldSystem.GetOutputDependency());

            var cw = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;


            this.Entities
                .WithBurst()
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
                        var collector = new AnyHitExcludeSelfCollector<DistanceHit>(sphere.Distance, entity);
                        var isHit = cw.CalculateDistance(hitInput, ref collector);

                        ground.IsGround = isHit;//collector.NumHits > 0;
                    }
                )
                .ScheduleParallel();

            //this.buildPhysicsWorldSystem.AddInputDependencyToComplete(this.Dependency);
        }
    }

}
