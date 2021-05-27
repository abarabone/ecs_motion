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
using System.Runtime.CompilerServices;
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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class BulletRayHitToEmitSystem : DependencyAccessableSystemBase
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


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
            var cw = phyScope.PhysicsWorld.CollisionWorld;


            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
            //var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

            //var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);

            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<Bullet.RayTag>()
                .WithReadOnly(targets)
                //.WithReadOnly(parts)
                //.WithReadOnly(corpss)
                .WithReadOnly(cw)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        //in Particle.TranslationPtoPData ptop,
                        in Translation pos,
                        in Particle.TranslationTailData tail,
                        in Bullet.LinkData link,
                        in Bullet.EmitData emit,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        
                        var hit = cw.BulletHitRay
                            (link.OwnerStateEntity, pos.Value, tail.Position, 1.0f, targets);

                        if (!hit.isHit) return;


                        var prefab = emit.EmittingPrefab;
                        var state = link.OwnerStateEntity;
                        var hpos = hit.core.posision;
                        var cps = corps.TargetCorps;
                        emit_(cmd, eqi, prefab, state, hpos, cps);


                        cmd.DestroyEntity(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_(
            EntityCommandBuffer.ParallelWriter cmd, int eqi,
            Entity prefab, Entity stateEntity, float3 position, Corps targetCorps)
        {

            var instance = cmd.Instantiate(eqi, prefab);

            cmd.SetComponent(eqi, instance,
                new Translation
                {
                    Value = position,
                }
            );
            cmd.SetComponent(eqi, instance,
                new Bullet.LinkData
                {
                    OwnerStateEntity = stateEntity,
                }
            );
            cmd.SetComponent(eqi, instance,
                new CorpsGroup.TargetWithArmsData
                {
                    TargetCorps = targetCorps,
                }
            );

        }
    }

}