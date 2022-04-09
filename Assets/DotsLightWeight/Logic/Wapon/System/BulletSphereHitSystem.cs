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
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.Hit))]
    [UpdateAfter(typeof(BulletRayHitSystem))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public partial class BulletSphereHitSystem : DependencyAccessableSystemBase//, BarrierDependency.IRecievable
    {

        //public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();

        //    this.Reciever.Dispose();
        //}


        CommandBufferDependency.Sender cmddep;

        PhysicsHitDependency.Sender phydep;

        HitMessage<Structure.PartHitMessage>.Sender ptSender;
        HitMessage<Character.HitMessage>.Sender chSender;


        //DependencyAccessableSystemBase prevHitSystem;



        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);

            this.ptSender = HitMessage<Structure.PartHitMessage>.Sender.Create<StructurePartMessageAllocationSystem>(this);
            this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);

            //this.prevHitSystem = World.GetExistingSystem<BulletRayHitSystem>();
        }


        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            using var phyScope = this.phydep.WithDependencyScope();
            using var pthitScope = this.ptSender.WithDependencyScope();
            using var chhitScope = this.chSender.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            var cw = phyScope.PhysicsWorld.CollisionWorld;
            var pthit = pthitScope.MessagerAsParallelWriter;
            var chhit = chhitScope.MessagerAsParallelWriter;


            var damages = this.GetComponentDataFromEntity<Bullet.PointDamageSpecData>(isReadOnly: true);
            var emits = this.GetComponentDataFromEntity<Bullet.EmitData>(isReadOnly: true);

            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);

            var dt = this.Time.DeltaTime;
            var predtrcp = dt * TimeEx.PrevDeltaTimeRcp;

            this.Entities
                .WithBurst()
                .WithAll<Bullet.SphereTag>()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .WithReadOnly(damages)
                .WithReadOnly(emits)
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                .WithReadOnly(corpss)
                .WithNativeDisableParallelForRestriction(pthit)
                .WithNativeDisableContainerSafetyRestriction(pthit)
                .WithNativeDisableParallelForRestriction(chhit)
                .WithNativeDisableContainerSafetyRestriction(chhit)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Translation pos,
                        in Particle.VelocityFactorData vfact,
                        in Bullet.LinkData link,
                        in Particle.OptionalData additional,
                        //in Bullet.PointDamageSpecData damage,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;

                        var hit_ = cw.BulletHitSphere
                            (link.OwnerStateEntity, pos.Value, additional.Radius, targets);

                        if (!hit_.isHit) return;


                        var v = (pos.Value - vfact.PrePosition.xyz) * predtrcp;
                        var hit = hit_.core;

                        if (damages.HasComponent(entity))
                        {
                            var damage = damages[entity].Damage;
                            hit.Hit(chhit, pthit, default, parts, corpss, v, damage, corps);
                        }

                        if (emits.HasComponent(entity))
                        {
                            var emit = emits[entity];
                            hit.Emit(cmd, eqi, emit, link, corps);
                        }


                        cmd.DestroyEntity(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();

            //this.AddInputDependency(this.prevHitSystem.GetOutputDependency());
        }

    }

}