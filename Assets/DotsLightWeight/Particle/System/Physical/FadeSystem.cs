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
using System.Runtime.CompilerServices;


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
    [UpdateAfter(typeof(ParticleLifeTimeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class FadeSystem : SystemBase
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
                .WithName("Fade")
                .WithBurst()
                .ForEach(
                    (
                        ref Particle.AdditionalData data,
                        ref BillBoad.BlendAlphaFadeData blend,
                        ref BillBoad.AdditiveAlphaFadeData additive
                    ) =>
                    {
                        data.BlendColor.a = fade_(ref blend.Fader, dt);

                        data.AdditiveColor.a = fade_(ref additive.Fader, dt);
                    }
                )
                .ScheduleParallel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte fade_(ref BillBoad.AnimationUnit fader, float dt)
        {

            fader.Delay -= dt;
            var speed = math.select(fader.SpeedPerSec, 0, fader.Delay > 0);

            var next = fader.Current + speed * dt;
            fader.Current = math.clamp(next, fader.Min, fader.Max);
            return (byte)(fader.Current * 255);
        }

    }


}

