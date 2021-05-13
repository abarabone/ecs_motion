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


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(CameraMoveSystem))]
    public class EmitBulletSystem : DependencyAccessableSystemBase
    {


        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        struct PtoPUnit
        {
            public float3 start;
            public float3 end;
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.MoveSpecData>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithBurst()
                .WithNone<Bullet.MoveSpecData>()
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref FunctionUnit.EmittingStateData state,
                        ref FunctionUnit.TriggerData trigger,
                        in FunctionUnit.BulletEmittingData emitter,
                        in FunctionUnit.StateLinkData slink,
                        in FunctionUnit.MuzzleLinkData mlink,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;

                        if (!trigger.IsTriggered) return;

                        trigger.IsTriggered = false;// 一発ずつオフにする


                        if (currentTime < state.NextEmitableTime) return;

                        var rnd = Random.CreateFromIndex((uint)entityInQueryIndex + (uint)math.asuint(dt) & 0x_7fff_ffff);

                        if (emitter.EffectPrefab != Entity.Null)
                        {
                            var mzrot = rots[mlink.MuzzleEntity].Value;
                            var mzpos = poss[mlink.MuzzleEntity].Value;
                            var efpos = calcEffectPosition_(mzrot, mzpos, emitter);
                            emitEffect_(cmd, eqi, emitter.EffectPrefab, efpos, rnd);
                        }



                        // 前回の発射が直前のフレームなら連続した発射間隔、はなれたフレームなら今フレームをベースにした発射感覚になる
                        var elapsed = 0.0f;
                        var frameBaseTime = currentTime - dt;
                        var isEmitPrevFrame = state.NextEmitableTime > frameBaseTime;
                        var baseTime = math.select(frameBaseTime, state.NextEmitableTime, isEmitPrevFrame);

                        var bulletData = bullets[emitter.BulletPrefab];
                        var rot = rots[mlink.EmitterEntity].Value;
                        var pos = poss[mlink.EmitterEntity].Value;

                        var bulletPos = calcBulletPosition_(rot, pos, in emitter);
                        var range = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor;

                        var g = new DirectionAndLength { Value = gravity.As_float4(bulletData.GravityFactor) };
                        var aim = new DirectionAndLength { Value = float3.zero.As_float4(bulletData.AimFactor) };
                        var acc = g.Ray + aim.Ray;

                        do
                        {
                            //state.NextEmitableTime = currentTime + emitter.EmittingInterval;
                            elapsed += emitter.EmittingInterval;
                            state.NextEmitableTime = baseTime + elapsed;

                            // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                            for (var i = 0; i < emitter.NumEmitMultiple; i++)
                            {
                                var bulletDir = calcBulletDirection_(rot, ref rnd, emitter.AccuracyRad);
                                var speed = bulletDir * bulletData.BulletSpeed;

                                emit_(cmd, entityInQueryIndex,
                                    emitter.BulletPrefab, slink.StateEntity,
                                    bulletPos, range, speed, acc, corps.TargetCorps);
                            }
                        }
                        while (currentTime >= state.NextEmitableTime);
                    }
                )
                .ScheduleParallel();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcEffectPosition_(
            quaternion rot, float3 pos, in FunctionUnit.BulletEmittingData emitter)
        {

            var muzpos = pos + math.mul(rot, emitter.MuzzlePositionLocal);

            return muzpos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emitEffect_(
            EntityCommandBuffer.ParallelWriter cmd, int eqi, Entity effectPrefab,
            float3 pos, Random rnd)
        {
            //if (effectPrefab == Entity.Null) return;

            var ent = cmd.Instantiate(eqi, effectPrefab);
            
            cmd.SetComponent(eqi, ent, new Translation
            {
                Value = pos,
            });
            cmd.SetComponent(eqi, ent, new BillBoad.RotationData
            {
                Direction = rnd.NextFloat2Direction() * rnd.NextFloat(0.8f, 1.2f),
            });
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletDirection_(
            quaternion dirrot, ref Random rnd, float accuracyRad)
        {
            
            var yrad = rnd.NextFloat(accuracyRad);
            var zrad = rnd.NextFloat(2.0f * math.PI);
            var bulletDir = math.mul(dirrot, math.forward(quaternion.EulerYZX(0.0f, yrad, zrad)));

            return bulletDir;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletPosition_(
            quaternion rot, float3 pos, in FunctionUnit.BulletEmittingData emitter)
        {

            var muzpos = pos + math.mul(rot, emitter.MuzzlePositionLocal);

            return muzpos;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_(
            EntityCommandBuffer.ParallelWriter cmd, int eqi,
            Entity bulletPrefab, Entity stateEntity,
            float3 bulletPosition, float range, float3 speed, float3 acc, Corps targetCorps)
        {

            var newBullet = cmd.Instantiate(eqi, bulletPrefab);

            cmd.SetComponent(eqi, newBullet,
                new Particle.TranslationPtoPData
                {
                    Start = bulletPosition,
                    End = bulletPosition
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.VelocityData
                {
                    Velocity = speed.As_float4(),
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.AccelerationData
                {
                    Acceleration = acc.As_float4(),
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.DistanceData
                {
                    RestRangeDistance = range,
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new Bullet.LinkData
                {
                    OwnerStateEntity = stateEntity,
                }
            );
            cmd.SetComponent(eqi, newBullet,
                new CorpsGroup.TargetWithArmsData
                {
                    TargetCorps = targetCorps,
                }
            );

        }

    }

}