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

namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Unity.Physics;
    using Abarabone.Structure;
    using UnityEngine.Rendering;
    using Abarabone.Dependency;
    using Abarabone.Utilities;

    using Random = Unity.Mathematics.Random;
    using System;


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
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


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.SpecData>(isReadOnly: true);


            // カメラは暫定
            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );


            var deltaTime = this.Time.DeltaTime;
            var currentTime = this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithBurst()
                .WithNone<Bullet.SpecData>()
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref FunctionUnit.EmittingStateData state,
                        ref FunctionUnit.TriggerData trigger,
                        in FunctionUnit.BulletEmittingData emitter,
                        in FunctionUnit.OwnerLinkData link
                    ) =>
                    {
                        if (!trigger.IsTriggered) return;

                        trigger.IsTriggered = false;// 一発ずつオフにする


                        if (currentTime < state.NextEmitableTime) return;

                        // 前回の発射が直前のフレームなら連続した発射間隔、はなれたフレームなら今フレームをベースにした発射感覚になる
                        var elapsed = 0.0f;
                        var frameBaseTime = currentTime - deltaTime;
                        var isEmitPrevFrame = state.NextEmitableTime > frameBaseTime;
                        var baseTime = math.select(frameBaseTime, state.NextEmitableTime, isEmitPrevFrame);

                        var rnd = Random.CreateFromIndex((uint)entityInQueryIndex + (uint)math.asuint(deltaTime) & 0x_7fff_ffff);

                        var bulletData = bullets[emitter.BulletPrefab];
                        var rot = rots[link.MuzzleEntity];
                        var pos = poss[link.MuzzleEntity];

                        //var bulletPos = calcBulletPosition_(camrot, campos, in emitter, in bulletData);
                        var bulletPos = calcBulletPosition_(rot, pos, in emitter, in bulletData);
                        var range = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor;

                        var g = new DirectionAndLength { Value = gravity.As_float4(bulletData.GravityFactor) };
                        var aim = new DirectionAndLength { Value = float3.zero.As_float4(bulletData.AimFactor) };
                        var acc = math.normalizesafe(g.Ray + aim.Ray);

                        do
                        {
                            //state.NextEmitableTime = currentTime + emitter.EmittingInterval;
                            elapsed += emitter.EmittingInterval;
                            state.NextEmitableTime = baseTime + elapsed;

                            // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                            for (var i = 0; i < emitter.NumEmitMultiple; i++)
                            {
                                //var bulletDir = calcBulletDirection_(camrot, bulletPos, ref rnd, emitter.AccuracyRad);
                                var bulletDir = calcBulletDirection_(rot.Value, bulletPos, ref rnd, emitter.AccuracyRad);
                                var speed = bulletDir.As_float4(bulletData.BulletSpeed);

                                emit_(cmd, entityInQueryIndex, emitter.BulletPrefab, bulletPos, range, speed, acc);
                            }
                        }
                        while (currentTime >= state.NextEmitableTime);
                    }
                )
                .ScheduleParallel();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletDirection_(
            quaternion dirrot, float3 start, ref Random rnd, float accuracyRad)
        {

            var yrad = rnd.NextFloat(accuracyRad);
            var zrad = rnd.NextFloat(2.0f * math.PI);
            var bulletDir = math.mul(dirrot, math.forward(quaternion.EulerYZX(0.0f, yrad, zrad)));

            return bulletDir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletPosition_(
            //quaternion camrot, float3 campos,
            Rotation rot, Translation pos,
            in FunctionUnit.BulletEmittingData emitter, in Bullet.SpecData bulletData)
        {

            var muzpos = pos.Value + math.mul(rot.Value, emitter.MuzzlePositionLocal);
            //var muzzleDir = math.forward(camrot);
            //var muzpos = campos + muzzleDir * 0.5f;

            return muzpos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_(
            EntityCommandBuffer.ParallelWriter cmd, int entityInQueryIndex, Entity bulletPrefab,
            float3 bulletPosition, float range, float4 speed, float3 acc)
        {

            var newBullet = cmd.Instantiate(entityInQueryIndex, bulletPrefab);

            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Particle.TranslationPtoPData
                {
                    Start = bulletPosition,
                    End = bulletPosition
                }
            );
            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Bullet.VelocityData
                {
                    //Velocity = (bulletDirection * speed).As_float4(),
                    DirAndLen = speed,
                }
            );
            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Bullet.AccelerationData
                {
                    Acceleration = acc.As_float4(),
                }
            );
            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Bullet.DistanceData
                {
                    RestRangeDistance = range,
                }
            );

        }

    }

}