using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsLite.Character
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Dependency;
    using DotsLite.Targeting;
    using DotsLite.Physics;
    using DotsLite.Model;
    using DotsLite.Collision;


    //[DisableAutoCreation]
    [UpdateAfter(typeof(TargetSensorWakeupAndCopyPositionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class FindNearestTargeSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;
        PhysicsHitDependency.Sender phydep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.phydep = PhysicsHitDependency.Sender.Create(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            using var phyScope = this.phydep.WithDependencyScope();


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();
            var cw = this.phydep.PhysicsWorld.CollisionWorld;

            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<TargetSensor.WakeupFindTag>()
                .WithReadOnly(targets)
                .WithReadOnly(poss)
                .WithReadOnly(cw)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref TargetSensor.LinkTargetMainData target,
                        in TargetSensor.CollisionData collision
                    )
                =>
                    {

                        // 暫定：あとでグループ対応コレクターにすること
                        var collector = new ClosestTargetedHitExcludeSelfCollector<DistanceHit>(collision.Distance, collision.PostureEntity, targets);


                        var startpos = poss[collision.PostureEntity].Value;
                        cw.OverlapSphereCustom(startpos, collision.Distance, ref collector, collision.Filter);

                        // ヒットしなければ Entity.Null という前提
                        target.TargetMainEntity = collector.ClosestHit.Entity;


                        // 一回実行したらやめる
                        cmd.RemoveComponent<TargetSensor.WakeupFindTag>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }


    }

}

