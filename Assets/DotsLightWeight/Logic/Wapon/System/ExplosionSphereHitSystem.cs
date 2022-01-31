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
using Unity.Collections.LowLevel.Unsafe;

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
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.Hit))]
    [UpdateAfter(typeof(BulletRayHitSystem))]
    [UpdateAfter(typeof(BulletSphereHitSystem))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class ExplosionSphereHitSystem : DependencyAccessableSystemBase//, BarrierDependency.IRecievable
    {

        //public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();

        //    this.Reciever.Dispose();
        //}


        CommandBufferDependency.Sender cmddep;

        PhysicsHitDependency.Sender phydep;

        HitMessage<Structure.EnvelopeHitMessage>.Sender stSender;
        HitMessage<Structure.PartHitMessage>.Sender ptSender;
        HitMessage<Character.HitMessage>.Sender chSender;

        //DependencyAccessableSystemBase prevHitSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);

            this.stSender = HitMessage<Structure.EnvelopeHitMessage>.Sender.Create<StructureEnvelopeMessageAllocationSystem>(this);
            this.ptSender = HitMessage<Structure.PartHitMessage>.Sender.Create<StructurePartMessageAllocationSystem>(this);
            this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);

            //this.prevHitSystem = World.GetExistingSystem<BulletSphereHitSystem>();
        }


        protected unsafe override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();
            using var phyScope = this.phydep.WithDependencyScope();
            using var sthitScope = this.stSender.WithDependencyScope();
            using var pthitScope = this.ptSender.WithDependencyScope();
            using var chhitScope = this.chSender.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            var cw = phyScope.PhysicsWorld.CollisionWorld;
            var sthit = sthitScope.MessagerAsParallelWriter;
            var pthit = pthitScope.MessagerAsParallelWriter;
            var chhit = chhitScope.MessagerAsParallelWriter;


            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<Explosion.HittableTag>()
                //.WithAll<Bullet.SphereTag>()
                .WithReadOnly(targets)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                .WithReadOnly(corpss)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .WithNativeDisableParallelForRestriction(pthit)
                .WithNativeDisableContainerSafetyRestriction(pthit)
                .WithNativeDisableParallelForRestriction(chhit)
                .WithNativeDisableContainerSafetyRestriction(chhit)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Translation pos,
                        in Particle.OptionalData additional,
                        in Explosion.SpecData spec,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        cmd.RemoveComponent<Explosion.HittableTag>(eqi, entity);


                        var filter = new CollisionFilter
                        {
                            BelongsTo = CollisionFilter.Default.BelongsTo,
                            CollidesWith = CatFlag.datail | CatFlag.envelope | CatFlag.detenv,
                            //CollidesWith = CatFlag.datail | CatFlag.detenv,
                        };


                        using var results = new NativeList<DistanceHitResult>(spec.NumMaxHitCollecting, Allocator.Temp);
                        var collector = targets.GetAllDistanceCollector(spec.HitRadius, results);
                        var isHit = cw.OverlapSphereCustom(pos.Value, spec.HitRadius, ref collector, filter);

                        if (!isHit) return;


                        for (var i = 0; i < collector.NumHits; i++)
                        {
                            ref var hit_ = ref UnsafeUtility.ArrayElementAsRef<DistanceHitResult>(results.GetUnsafePtr(), i);
                            var hit = hit_.core;

                            switch (hit.hitType)
                            {
                                // parts の destroy と main での wakeup components 制御が競合する問題もある
                                // （なので envelope 同志の衝突での wakeup 着脱と競合する可能性もはらんでいるかも？？ em 専用 system をつくるべきなのかなぁ）
                                case HitType.envelope:

                                    hit.PostStructureEnvelopeHitMessage(sthit);
                                    if (!isHit) hit.PostStructureEnvelopeHitMessage(sthit);
                                    break;

                                case HitType.part:

                                    hit.PostStructurePartHitMessage(pthit, parts);
                                    if (!isHit) hit.PostStructurePartHitMessage(pthit, parts);
                                    break;


                                case HitType.charactor:

                                    var otherCorpts = corpss[hit.hitEntity];
                                    if ((otherCorpts.BelongTo & corps.TargetCorps) == 0) return;

                                    var pow = spec.HitRadius * math.rcp(hit_.distance) * (hit.posision - pos.Value);
                                    hit.PostCharacterHitMessage(chhit, 1.0f, pow);
                                    break;


                                default:
                                    break;
                            }
                        }

                    }
                )
                .ScheduleParallel();

            //this.AddInputDependency(this.prevHitSystem.GetOutputDependency());
        }

    }

}