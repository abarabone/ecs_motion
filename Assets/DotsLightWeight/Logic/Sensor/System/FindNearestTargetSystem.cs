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

namespace Abarabone.Character
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Dependency;
    using Abarabone.Targeting;
    using Abarabone.Physics;
    using Abarabone.Model;


    //[DisableAutoCreation]
    [UpdateAfter(typeof(TargetSensorWakeSystem))]
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
            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<TargetSensor.WakeupFindTag>()
                .WithReadOnly(mainEntities)
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

                        var startpos = poss[collision.PostureEntity].Value;

                        // 暫定：あとでグループ対応コレクターにすること
                        var collector = new ClosestHitExcludeSelfCollector<DistanceHit>(1.0f, entity, mainEntities);
                        

                        var isHit = cw.OverlapSphereCustom(startpos, collision.Distance, ref collector, collision.Filter);
                        target.TargetMainEntity = collector.ClosestHit.Entity;
                        // ヒットしなければ Entity.Null という前提

                        if (!isHit) cmd.AddComponent<Disabled>(entityInQueryIndex, entity);

                        cmd.RemoveComponent<TargetSensor.WakeupFindTag>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }


    }

}

