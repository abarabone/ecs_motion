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

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace DotsLite.Particle
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Dependency;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class ParticleLifeTimeSystem : DependencyAccessableSystemBase
    {


        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var currentTime = (float)this.Time.ElapsedTime;


            this.Entities
                .WithName("Initialize")
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Particle.LifeTimeData timer,
                        in Particle.LifeTimeSpecData spec
                    ) =>
                    {
                        var eqi = entityInQueryIndex;

                        timer.EndTime = currentTime + spec.DurationSec;

                        cmd.RemoveComponent<Particle.LifeTimeSpecData>(eqi, entity);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithName("LifeTime")
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Particle.LifeTimeData timer
                        //in Particle.LifeTimeSpecData spec
                    ) =>
                    {
                        var eqi = entityInQueryIndex;

                        //if (timer.EndTime == 0)
                        //{
                        //    timer.EndTime = currentTime + spec.DurationSec;
                        //}

                        if (timer.EndTime > currentTime) return;

                        cmd.DestroyEntity(eqi, entity);
                    }
                )
                .ScheduleParallel();
        }

    }


}

