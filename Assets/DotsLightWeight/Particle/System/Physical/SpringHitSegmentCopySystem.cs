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
    using DotsLite.Common.Extension;
    using DotsLite.ParticleSystem.Aurthoring;

    using Random = Unity.Mathematics.Random;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    [UpdateAfter(typeof(MoveSpringSystem))]
    public partial class SpringHitSegmentCopySystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;

            var hftt = dt * dt * 0.5f;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえずエンジン側のを
                                                                  // 重力が変化する可能性を考えて、毎フレーム取得する
            var gtt = gravity * hftt;

            this.Entities
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Translation pos,
                    ref Psyllium.TranslationTailData tail,
                    in Spring.HittableSegmentData hittable,
                    in DynamicBuffer<LineParticle.TranslationTailLineData> tails) =>
                {
                    pos.Value = tails[hittable.Index.x].Position;
                    tail.Position = tails[hittable.Index.y].Position;
                })
                .ScheduleParallel();

        }

    }


}

