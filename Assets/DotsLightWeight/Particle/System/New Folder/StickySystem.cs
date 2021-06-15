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

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class StickySystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえずエンジン側のを              
            // 重力が変化する可能性を考えて、毎フレーム取得する

            var g_ = gravity * harfsqdt;

            this.Entities
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    ref DynamicBuffer<Spring.StateData> states,
                    ref Psyllium.TranslationTailData tail,
                    in Spring.SpecData spec,
                    in Bullet.MoveSpecData movespec) =>
                {



                })
                .ScheduleParallel();

        }

    }

}

