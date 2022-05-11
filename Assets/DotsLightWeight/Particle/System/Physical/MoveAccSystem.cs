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

    using Random = Unity.Mathematics.Random;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    public partial class MoveAccSystem : SystemBase
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
                .WithName("Move")
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Translation pos,
                    ref Particle.VelocityFactorData vfact,
                    in Particle.VelocitySpecData spec) =>
                {
                    var prepos = pos.Value;
                    var g = gtt * spec.GravityFactor;
                    var a = spec.Acceleration * hftt;

                    var vt = pos.Value - vfact.PrePosition.xyz;
                    var d = vt * dtrate + g + a;

                    pos.Value += d;
                    vfact.PrePosition = prepos.As_float4();
                })
                .ScheduleParallel();

        }

    }


}

