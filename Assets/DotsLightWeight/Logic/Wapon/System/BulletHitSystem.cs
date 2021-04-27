using System.Collections;
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

namespace Abarabone.Arms
{
    using Abarabone.Dependency;
    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Unity.Physics;
    using Abarabone.Structure;
    using Abarabone.Character.Action;
    using Abarabone.Hit;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class BulletHitSystem : DependencyAccessableSystemBase
    {


        HitMessage<Abarabone.Structure.HitMessage>.Sender sender;

        PhysicsHitDependency.Sender phydep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.sender = HitMessage<Abarabone.Structure.HitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);
        }


        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();
            using var hitScope = this.sender.WithDependencyScope();


            var sthit = this.sender.AsParallelWriter();
            var cw = this.phydep.PhysicsWorld.CollisionWorld;

            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);
            var hitTargets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .ForEach(
                    (
                        Entity fireEntity,// int entityInQueryIndex,
                        in Particle.TranslationPtoPData ptop,
                        in Bullet.SpecData bullet,
                        //in Bullet.DistanceData dist
                        in Bullet.VelocityData v
                    ) =>
                    {
                        //var hit = cw.BulletHitRay
                        //    (bullet.MainEntity, ptop.Start, ptop.End, dist.RestRangeDistance, mainLinks);
                        var hit = cw.BulletHitRay
                            (bullet.StateEntity, ptop.Start, ptop.End, 1.0f, targets);

                        if (!hit.isHit) return;


                        //var ht = hitTargets[hit.]

                        hit.postMessageToHitTarget(sthit, parts);
                    }
                )
                .ScheduleParallel();
        }

    }

}