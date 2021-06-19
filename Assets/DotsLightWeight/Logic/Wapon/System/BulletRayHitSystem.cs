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
    using DotsLite.Misc;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class BulletRayHitSystem : DependencyAccessableSystemBase
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
            var chhit =chhitScope.MessagerAsParallelWriter;


            var damages = this.GetComponentDataFromEntity<Bullet.PointDamageSpecData>(isReadOnly: true);
            var emits = this.GetComponentDataFromEntity<Bullet.EmitData>(isReadOnly: true);
            var springs = this.GetComponentDataFromEntity<Spring.StickyStateData>(isReadOnly: true);

            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

            var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);

            var dt = this.Time.DeltaTime;
            var predtrcp = dt * TimeEx.PrevDeltaTimeRcp;

            this.Entities
                .WithBurst()
                .WithAll<Bullet.RayTag>()
                //.WithNone<Bullet.EmitterTag>()// 
                .WithReadOnly(damages)
                .WithReadOnly(emits)
                .WithReadOnly(springs)
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(corpss)
                .WithReadOnly(cw)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .WithNativeDisableParallelForRestriction(chhit)
                .WithNativeDisableContainerSafetyRestriction(chhit)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Translation pos,
                        in Psyllium.TranslationTailData tail,
                        in Particle.VelocityFactorData vfact,
                        in Bullet.LinkData link,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        
                        var hit_ = cw.BulletHitRay
                            (link.OwnerStateEntity, pos.Value, tail.Position, 1.0f, targets);

                        if (!hit_.isHit) return;


                        var v = (pos.Value - vfact.PrePosition.xyz) * predtrcp;
                        var hit = hit_.core;

                        if (damages.HasComponent(entity))
                        {
                            var damage = damages[entity].Damage;
                            hit.Hit(chhit, sthit, parts, corpss, v, damage, corps);
                        }

                        if (emits.HasComponent(entity))
                        {
                            var emit = emits[entity];
                            hit.Emit(cmd, eqi, emit, link, corps);
                        }

                        if (springs.HasComponent(entity))
                        {
                            var state = springs[entity];
                            hit.Sticky(cmd, eqi, entity, state);
                        }

                        cmd.DestroyEntity(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }

    }

}