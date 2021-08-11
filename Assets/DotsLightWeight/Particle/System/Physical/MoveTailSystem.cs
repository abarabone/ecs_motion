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
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    [UpdateBefore(typeof(MoveAccSystem))]
    public class MoveTailSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            this.Entities
                .WithName("TailLineCopy")
                .WithBurst()
                .WithAll<Psyllium.MoveTailTag>()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    in Psyllium.TranslationTailData tail) =>
                {
                    for (var i = tails.Length; i-- > 1;)
                    {
                        tails.ElementAt(i).Position = tails[i - 1].Position;
                    }

                    tails.ElementAt(0).Position = tail.Position;
                })
                .ScheduleParallel();

            this.Entities
                .WithName("TailCopy")
                .WithBurst()
                .WithAll<Psyllium.MoveTailTag>()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Psyllium.TranslationTailData tail,
                    in Translation pos) =>
                {
                    tail.PositionAndSize = pos.Value.As_float4(tail.Size);
                })
                .ScheduleParallel();

        }

    }


}

