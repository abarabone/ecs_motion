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

using Random = Unity.Mathematics.Random;

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
    public class MoveEasingSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;


            this.Entities
                .WithName("InitializeWithSetting")
                .WithAll<Particle.LifeTimeInitializeTag>()
                .WithBurst()
                .ForEach((
                    int nativeThreadIndex, int entityInQueryIndex,
                    ref Particle.EasingData data,
                    in Particle.EasingSetting setting,
                    in Translation pos) =>
                {
                    var tid = nativeThreadIndex;
                    var eqi = entityInQueryIndex;
                    var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));

                    var last = rnd.NextFloat(setting.LastDistanceMin, setting.LastDistanceMax);
                    var endpos = pos.Value + rnd.NextFloat3Direction() * last;

                    data.Set(endpos, data.Rate);
                })
                .ScheduleParallel();

            this.Entities
                .WithName("Initialize")
                .WithAll<Particle.LifeTimeInitializeTag>()
                .WithNone<Particle.EasingSetting>()
                .WithBurst()
                .ForEach((
                    ref Particle.EasingData data,
                    in Translation pos) =>
                {
                    var endpos = pos.Value + data.LastPosition;

                    data.Set(endpos, data.Rate);
                })
                .ScheduleParallel();

            this.Entities
                .WithName("Move")
                .WithBurst()
                .ForEach((
                    ref Translation pos,
                    in Particle.EasingData data) =>
                {

                    var newpos = (data.LastPosition - pos.Value) * data.Rate * dt;

                    pos.Value += newpos;

                })
                .ScheduleParallel();
        }

    }


}

