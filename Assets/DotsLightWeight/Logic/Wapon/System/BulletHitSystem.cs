﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.XR;
using Unity.Physics.Systems;

namespace DotsLite.Arms
{
    using DotsLite.Dependency;
    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using Unity.Physics;
    using DotsLite.Structure;
    using DotsLite.Character.Action;
    using DotsLite.Hit;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class BulletHitSystem : DependencyAccessableSystemBase
    {


        HitMessage<Structure.HitMessage>.Sender stSender;
        HitMessage<Character.HitMessage>.Sender chSender;

        PhysicsHitDependency.Sender phydep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.stSender = HitMessage<Structure.HitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);
            this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);
        }


        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();
            using var sthitScope = this.stSender.WithDependencyScope();
            using var chhitScope = this.chSender.WithDependencyScope();


            var sthit = this.stSender.AsParallelWriter();
            var chhit = this.chSender.AsParallelWriter();
            var cw = this.phydep.PhysicsWorld.CollisionWorld;

            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);
            var hitTargets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithReadOnly(hitTargets)
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .WithNativeDisableParallelForRestriction(chhit)
                .WithNativeDisableContainerSafetyRestriction(chhit)
                .ForEach(
                    (
                        Entity fireEntity,// int entityInQueryIndex,
                        in Particle.TranslationPtoPData ptop,
                        //in Bullet.SpecData bullet,
                        in Bullet.LinkData link,
                        //in Bullet.DistanceData dist
                        in Bullet.VelocityData v
                    ) =>
                    {
                        //var hit = cw.BulletHitRay
                        //    (bullet.MainEntity, ptop.Start, ptop.End, dist.RestRangeDistance, mainLinks);
                        var hit = cw.BulletHitRay
                            (link.StateEntity, ptop.Start, ptop.End, 1.0f, targets);

                        if (!hit.isHit) return;


                        var ht = hitTargets[hit.hitEntity];

                        switch (ht.HitType)
                        {
                            case Hit.HitType.part:
                                hit.PostStructureHitMessage(sthit, parts);
                                break;
                            case Hit.HitType.charactor:
                                hit.PostCharacterHitMessage(chhit, 1.0f, v.Velocity.xyz);
                                break;
                        }
                    }
                )
                .ScheduleParallel();
        }

    }

}