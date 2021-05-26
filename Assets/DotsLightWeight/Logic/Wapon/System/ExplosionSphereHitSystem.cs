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
    using DotsLite.Collision;
    using DotsLite.Targeting;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class ExplosionSphereHitSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;

        PhysicsHitDependency.Sender phydep;

        HitMessage<Structure.HitMessage>.Sender stSender;
        HitMessage<Character.HitMessage>.Sender chSender;



        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);

            this.stSender = HitMessage<Structure.HitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);
            this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);
        }


        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            using var phyScope = this.phydep.WithDependencyScope();
            using var sthitScope = this.stSender.WithDependencyScope();
            using var chhitScope = this.chSender.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            var cw = phyScope.PhysicsWorld.CollisionWorld;
            var sthit = sthitScope.MessagerAsParallelWriter;
            var chhit = chhitScope.MessagerAsParallelWriter;


            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

            var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<Bullet.SphereTag>()
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                .WithReadOnly(corpss)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .WithNativeDisableParallelForRestriction(chhit)
                .WithNativeDisableContainerSafetyRestriction(chhit)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Translation pos,
                        in Particle.AdditionalData additional,
                        in Explosion.SpecData spec,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {


                        var filter = new CollisionFilter
                        {
                            BelongsTo = CollisionFilter.Default.BelongsTo,
                            CollidesWith = CatFlag.datail | CatFlag.field | CatFlag.detenv,
                        };

                        var results = new NativeList<DistanceHit>(30, Allocator.Temp);
                        var isHit = cw.OverlapSphereCustom((pos.Value, spec.Radius, ref results, filter);

                        if (!isHit) return;


                        switch (hit.hitType)
                        {
                            case HitType.part:

                                hit.PostStructureHitMessage(sthit, parts);
                                break;


                            case HitType.charactor:

                                var otherCorpts = corpss[hit.hitEntity];
                                if ((otherCorpts.BelongTo & corps.TargetCorps) == 0) return;

                                hit.PostCharacterHitMessage(chhit, 1.0f, v.Velocity.xyz);
                                break;


                            default:
                                break;
                        }

                        cmd.DestroyEntity(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }

    }

}