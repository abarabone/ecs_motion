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
                    ref DynamicBuffer<Spring.StateData> state,
                    in Translation pos,
                    in Psyllium.TranslationTailData tail,
                    in DynamicBuffer<LineParticle.TranslationTailLineData> line) =>
                    {

                        state[0] = new Spring.StateData
                        {
                            PrePosition = pos.Value.As_float4(),
                        };
                        state[1] = new Spring.StateData
                        {
                            PrePosition = tail.PositionAndSize,
                        };
                        for (var i = 0; i < line.Length; i++)
                        {
                            state[2 + i] = new Spring.StateData
                            {
                                PrePosition = line[i].PositionAndColor
                            };
                        }
                    })
                .ScheduleParallel();

            //this.Entities
            //    .WithBurst()
            //    .ForEach((
            //        ref Translation pos,
            //        ref Psyllium.TranslationTailData tail,
            //        ref DynamicBuffer<LineParticle.TranslationTailLineData> line,
            //        ref DynamicBuffer<Spring.StateData> state,
            //        in Spring.SpecData spec,
            //        in Bullet.MoveSpecData movespec) =>
            //    {
            //        var g = gravity * movespec.GravityFactor;


            //        var p0 = pos.Value;
            //        var p1 = tail.Position;
            //        var vt0 = SpringUtility.calc_vt(p0, state[0].PrePosition.xyz);
            //        var vt1 = SpringUtility.calc_vt(p1, state[1].PrePosition.xyz);
            //        var ftt_0 = float3.zero;
            //        var ftt01 = spec.calc_ftt(p0, p1, vt0, vt1, dt);

            //        var nextpos = p0 + vt0 + ftt_0 + ftt01;
            //        var prepos = p0;

            //        pos.Value = nextpos;
            //        state[0] = new Spring.StateData { PrePosition = prepos.As_float4() };


            //        p0 = p1;
            //        p1 = line[0].Position;
            //        vt0 = vt1;
            //        vt1 = SpringUtility.calc_vt(p1, state[0 + 2].PrePosition.xyz);
            //        ftt_0 = ftt01;
            //        ftt01 = spec.calc_ftt(p0, p1, vt0, vt1, dt);

            //        nextpos = p0 + vt0 + ftt_0 + ftt01;
            //        prepos = p0;

            //        tail.Position = nextpos;
            //        state[1] = new Spring.StateData { PrePosition = prepos.As_float4() };


            //        for (var i = 1; i < line.Length; i++)
            //        {
            //            p0 = p1;
            //            p1 = line[i].Position;
            //            vt0 = vt1;
            //            vt1 = SpringUtility.calc_vt(p1, state[i + 2].PrePosition.xyz);
            //            ftt_0 = ftt01;
            //            ftt01 = spec.calc_ftt(p0, p1, vt0, vt1, dt);

            //            nextpos = p0 + vt0 + ftt_0 + ftt01;
            //            prepos = p0;

            //            var res = line[i - 1];
            //            res.Position = nextpos;
            //            line[i - 1] = res;
            //            state[1] = new Spring.StateData
            //            {
            //                PrePosition = prepos.As_float4()
            //            };
            //        }
            //    })
            //    .ScheduleParallel();

        }

    }

    static class SpringUtility
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 calc_vt(float3 curpos, float3 prepos) => curpos - prepos;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 calc_ftt(this Spring.SpecData spec,
            float3 p0, float3 p1, float3 vt0, float3 vt1,
            float dt)
        {
            var line = p0 - p1;

            var len = math.length(line);
            var lenrcp = math.rcp(len);
            if (math.isinf(lenrcp)) return float3.zero;
            //lenrcp = math.select(0.0f, lenrcp, math.isnan(lenrcp));
            var dir = line * lenrcp;

            var f0 = (len - spec.Rest) * spec.Spring;// 伸びに抵抗し、縮もうとする力がプラス
            var ft0 = dir * f0 * dt;

            var ft1 = spec.Dumper * (vt1 - vt0);

            return (ft0 - ft1) * dt;
        }

    }
}

