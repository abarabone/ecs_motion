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

namespace DotsLite.Arms
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
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    //[UpdateAfter(typeof())]
    public class MoveSpringSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var gravity = UnityEngine.Physics.gravity.As_float3().As_float4();// とりあえずエンジン側のを
                                                                              // 重力が変化する可能性を考えて、毎フレーム取得する

            this.Entities
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<Spring.StateData> states,
                    in Translation pos,
                    in Psyllium.TranslationTailData tail,
                    in DynamicBuffer<LineParticle.TranslationTailLineData> tails) =>
                    {

                        states[0] = new Spring.StateData
                        {
                            PrePosition = pos.Value.As_float4(),
                        };
                        states[1] = new Spring.StateData
                        {
                            PrePosition = tail.PositionAndSize,
                        };
                        for (var i = 0; i < tails.Length; i++)
                        {
                            states[2 + i] = new Spring.StateData
                            {
                                PrePosition = tails[i].PositionAndColor
                            };
                        }
                    })
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .ForEach((
                    ref Translation pos,
                    ref Psyllium.TranslationTailData tail,
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    ref DynamicBuffer<Spring.StateData> states,
                    in Spring.SpecData spec,
                    in Bullet.MoveSpecData movespec) =>
                {
                    var g = gravity * movespec.GravityFactor;


                    float3 currpos, nextpos;
                    float3 currvt, nextvt;
                    float3 fttup, fttdown;
                    float3 newpos;

                    newpos = calcNewPositionFirst_(pos.Value, states[0].PrePosition.xyz, tail.Position, states[1].PrePosition.xyz, spec);
                    pos.Value = newpos;
                    states.shiftPrePosition(0, currpos);

                    newpos = calcNewPosition_(tails[0].Position, states[2].PrePosition.xyz, spec);
                    tail.Position = newpos;
                    states.shiftPrePosition(1, currpos);

                    for (var i = 0; i < tails.Length - 1; i++)
                    {
                        newpos = calcNewPosition_(tails[1 + i].Position, states[3 + i].PrePosition.xyz, spec);
                        tails.setResultPosition(i, newpos);
                        states.shiftPrePosition(2 + i, currpos);
                    }

                    newpos = calcNewPositionLast_();
                    tails.setResultPosition(tails.Length - 1, newpos);
                    states.shiftPrePosition(states.Length - 1, currpos);

                    return;


                    float3 calcNewPositionFirst_(
                        float3 _currNowPos, float3 _currPrePos, float3 _nextNowPos, float3 _nextPrePos, Spring.SpecData spec)
                    {
                        currpos = _currNowPos;
                        currvt = calc_vt(_currNowPos, _currPrePos);

                        nextpos = _nextNowPos;
                        nextvt = calc_vt(_nextNowPos, _nextPrePos);

                        fttdown = calc_ftt(currpos, nextpos, currvt, nextvt, spec, dt);

                        //return currpos + currvt - fttdown;
                        return currpos - fttdown;
                    }

                    float3 calcNewPosition_(float3 _nextNowPos, float3 _nextPrePos, Spring.SpecData spec)
                    {
                        currpos = nextpos;
                        currvt = nextvt;

                        nextpos = _nextNowPos;
                        nextvt = calc_vt(_nextNowPos, _nextPrePos);

                        fttup = fttdown;
                        fttdown = calc_ftt(currpos, nextpos, currvt, nextvt, spec, dt);

                        return currpos + currvt + fttup - fttdown;
                    }

                    float3 calcNewPositionLast_()
                    {
                        currpos = nextpos;
                        currvt = nextvt;

                        fttup = fttdown;

                        return currpos + currvt + fttup;
                    }
                })
                .ScheduleParallel();

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calc_vt(float3 curpos, float3 prepos) => curpos - prepos;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calc_ftt(
            float3 p0, float3 p1, float3 vt0, float3 vt1,
            Spring.SpecData spec, float dt)
        {
            var line = p0 - p1;

            var len = math.length(line);
            if (len == 0.0f) return float3.zero;
            if (math.isinf(len)) return float3.zero;
            if (math.isnan(len)) return float3.zero;
            var lenrcp = math.rcp(len);
            //lenrcp = math.select(0.0f, lenrcp, math.isnan(lenrcp));
            var dir = line * lenrcp;

            var f0 = (len - spec.Rest) * spec.Spring;// 伸びに抵抗し、縮もうとする力がプラス
            var ft0 = dir * f0 * dt;

            var ft1 = 0;// spec.Dumper * (vt1 - vt0);

            return (ft0 - ft1) * dt;
        }
    }

    static class SpringUtility
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void setResultPosition(
            ref this DynamicBuffer<LineParticle.TranslationTailLineData> tails, int i, float3 newpos)
        {
            var tail = tails[i];
            tail.Position = newpos;
            tails[i] = tail;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void shiftPrePosition(
            ref this DynamicBuffer<Spring.StateData> preposs, int i, float3 newpos)
        {
            preposs[i] = new Spring.StateData
            {
                PrePosition = newpos.As_float4(),
            };
        }
    }
}

