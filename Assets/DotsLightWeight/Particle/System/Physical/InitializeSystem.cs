using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

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
    using DotsLite.Common.Extension;

    using Random = Unity.Mathematics.Random;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class InitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            this.Entities
                .WithName("Tail")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Psyllium.TranslationTailData tail,
                    in Translation pos,
                    in Particle.AdditionalData data) =>
                {
                    tail.PositionAndSize = pos.Value.As_float4(data.Radius);
                })
                .ScheduleParallel();

            this.Entities
                .WithName("TailLine")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    in Translation pos) =>
                {
                    for (var i = 0; i < tails.Length; i++)
                    {
                        var tail = tails.ElementAt(i).Position = pos.Value;
                    }
                })
                .ScheduleParallel();

        }

    }


}

