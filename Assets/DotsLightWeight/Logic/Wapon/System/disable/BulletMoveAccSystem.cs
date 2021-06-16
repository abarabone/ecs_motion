//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Transforms;
//using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
//using Unity.Entities.UniversalDelegates;

//using System.Runtime.InteropServices;
//using UnityEngine.Assertions.Must;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using UnityEngine.InputSystem;
//using UnityEngine.Assertions;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.Arms.disable
//{

//    using DotsLite.Model;
//    using DotsLite.Model.Authoring;
//    using DotsLite.Arms;
//    using DotsLite.Character;
//    using DotsLite.Draw;
//    using DotsLite.Particle;
//    using DotsLite.CharacterMotion;
//    using DotsLite.Misc;
//    using DotsLite.Utilities;
//    using DotsLite.Collision;
//    using DotsLite.SystemGroup;
//    using DotsLite.Common.Extension;

//    using Random = Unity.Mathematics.Random;

//    [DisableAutoCreation]
//    //[UpdateInGroup(typeof(InitializationSystemGroup))]
//    //[UpdateAfter(typeof(ObjectInitializeSystem))]
//    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
//    //[UpdateAfter(typeof())]
//    public class BulletMoveAccSystem : SystemBase
//    {


//        protected override void OnUpdate()
//        {

//            var dt = this.Time.DeltaTime;
//            var sqdt = dt * dt;
//            var gravity = UnityEngine.Physics.gravity.As_float3().As_float4();// とりあえずエンジン側のを
//                                                                              // 重力が変化する可能性を考えて、毎フレーム取得する
            

//            this.Entities
//                .WithName("TailLineCopy")
//                .WithBurst()
//                .WithAll<Psyllium.MoveTailTag>()
//                .ForEach((
//                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
//                    in Psyllium.TranslationTailData tail) =>
//                {
//                    for (var i = tails.Length; i-- > 1;)
//                    {
//                        tails.ElementAt(i).Position = tails[i - 1].Position;
//                    }

//                    tails.ElementAt(0).Position = tail.Position;
//                })
//                .ScheduleParallel();

//            this.Entities
//                .WithName("TailCopy")
//                .WithBurst()
//                .WithAll<Psyllium.MoveTailTag>()
//                .ForEach((
//                    ref Psyllium.TranslationTailData tail,
//                    in Translation pos) =>
//                {
//                    tail.PositionAndSize = pos.Value.As_float4(tail.Size);
//                })
//                .ScheduleParallel();

//            this.Entities
//                .WithName("Move")
//                .WithBurst()
//                .ForEach((
//                    ref Translation pos,
//                    ref Bullet.VelocityData v,
//                    in Bullet.AccelerationData acc,
//                    in Bullet.MoveSpecData spec) =>
//                {
//                    var g = gravity * spec.GravityFactor;
//                    var a = acc.Acceleration + g;

//                    v.Velocity += a * dt;


//                    var d = v.Velocity.xyz * dt;

//                    pos.Value += d;
//                })
//                .ScheduleParallel();

//        }

//    }


//}

