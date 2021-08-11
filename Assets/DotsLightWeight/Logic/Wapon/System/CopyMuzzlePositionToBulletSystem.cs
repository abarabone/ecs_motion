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
using System.Runtime.CompilerServices;
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

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(BulletInitializeSystem))]
    [UpdateAfter(typeof(MoveSpringInitializeSystem))]
    public class CopyMuzzlePositionToBulletInitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {
            this.Entities
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Psyllium.TranslationTailData tail,
                    in Translation pos,
                    in DynamicBuffer<LineParticle.TranslationTailLineData> tails) =>
                {



                })
                .ScheduleParallel();
        }
    }

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    [UpdateAfter(typeof(MoveSpringSystem))]
    public class CopyMuzzlePositionToBulletSystem : SystemBase
    {


        protected override void OnUpdate()
        {


            this.Entities
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    ref DynamicBuffer<Spring.StatesData> states,
                    ref Psyllium.TranslationTailData tail,
                    in Spring.StickyApplyData sticky,
                    in Spring.SpecData spec) =>
                {



                })
                .ScheduleParallel();

        }

    }

}

