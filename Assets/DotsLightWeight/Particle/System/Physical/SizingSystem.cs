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


namespace DotsLite.ParticleSystem
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.ParticleSystem;
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
    [UpdateAfter(typeof(ParticleLifeTimeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public partial class SizingSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;


            //this.Entities
            //    .WithName("Initialize")
            //    .WithAll<Particle.LifeTimeInitializeTag>()
            //    .WithBurst()
            //    .ForEach(
            //        (
            //            int nativeThreadIndex, int entityInQueryIndex,
            //            ref BillBoad.RotationData rot
            //        ) =>
            //        {
            //            var tid = nativeThreadIndex;
            //            var eqi = entityInQueryIndex;
            //            var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));

            //            rot.Direction = rnd.NextFloat2Direction();

            //        }
            //    )
            //    .ScheduleParallel();

            this.Entities
                .WithName("Sizing")
                .WithBurst()
                .ForEach(
                    (
                        ref Particle.OptionalData data,
                        in BillBoad.SizeAnimationData anim,
                        in Particle.LifeTimeData timer
                    ) =>
                    {

                        var elapsed = currentTime - timer.StartTime;
                        var normalizeTime = math.saturate(elapsed * anim.MaxTimeSpanR);

                        data.Radius = math.lerp(anim.StartSize, anim.EndSize, normalizeTime);

                    }
                )
                .ScheduleParallel();
        }

    }


}

