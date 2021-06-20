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
    public class ParticleAnimationSystem : SystemBase//DependencyAccessableSystemBase
    {


        //CommandBufferDependency.Sender cmddep;


        //protected override void OnCreate()
        //{
        //    base.OnCreate();

        //    this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        //}

        protected override void OnUpdate()
        {
            //    using var cmdScope = this.cmddep.WithDependencyScope();


            //    var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var currentTime = (float)this.Time.ElapsedTime;


            this.Entities
                .WithName("Initialize")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                //.WithAll<BillBoad.UvAnimationInitializeTag>()
                .ForEach(
                    (
                        //Entity entity, int entityInQueryIndex,
                        ref BillBoad.UvAnimationWorkData work,
                        in BillBoad.UvAnimationData data
                    ) =>
                    {
                        //var eqi = entityInQueryIndex;

                        work.NextAnimationTime = currentTime + data.TimeSpan;

                        //cmd.RemoveComponent<BillBoad.UvAnimationInitializeTag>(eqi, entity);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithName("Progress")
                .WithBurst()
                .ForEach(
                    (
                        ref BillBoad.UvCursorData cursor,
                        ref BillBoad.UvAnimationWorkData work,
                        in BillBoad.UvAnimationData data
                    ) =>
                    {
                        if (work.NextAnimationTime > currentTime) return;

                        var overtime = currentTime - work.NextAnimationTime;


                        var freq = (int)(overtime * data.TimeSpanR) + 1;

                        var newindex = math.min(cursor.CurrentIndex + freq , data.AnimationIndexMax);
                        cursor.CurrentIndex = newindex & data.CursorAnimationMask;
                        //var newindex = (cursor.CurrentIndex + freq) & data.CursorAnimationMask;
                        //cursor.CurrentIndex = math.min(newindex, data.AnimationIndexMax);

                        work.NextAnimationTime = currentTime + data.TimeSpan * freq;
                    }
                )
                .ScheduleParallel();
        }

    }


}

