using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using UnityEngine.XR;
using Unity.Physics.Systems;

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using Unity.Physics;
    using DotsLite.Structure;
    using UnityEngine.Rendering;
    using DotsLite.Dependency;
    using DotsLite.Utilities;
    using DotsLite.Targeting;

    using Random = Unity.Mathematics.Random;
    using System;

    // ボーントランスフォームに組み込めればよいが、とりあえず独立で
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    [UpdateBefore(typeof(MoveSpringSystem))]
    public class AccessoryTransformSystem : SystemBase
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