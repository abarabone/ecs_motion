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
            var currentTime = (float)this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithBurst()
                //.WithNone<Bullet.MoveSpecData>()
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref FunctionUnit.EmittingStateData state,
                        in FunctionUnit.TriggerData trigger,
                        in FunctionUnit.BulletEmittingData emitter,
                        in FunctionUnit.StateLinkData slink,
                        in FunctionUnit.MuzzleLinkData mlink,
                        //in Bullet.MoveSpecData bulletData,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;

                        if (!trigger.IsTriggered) return;
                        if (currentTime < state.NextEmitableTime) return;

                        var rnd = Random.CreateFromIndex((uint)eqi + (uint)math.asuint(dt) & 0x_7fff_ffff);


                        if (emitter.EffectPrefab != Entity.Null)
                        {
                            var mzrot = rots[mlink.MuzzleEntity].Value;
                            var mzpos = poss[mlink.MuzzleEntity].Value;
                            var efpos = BulletEmittingUtility.CalcMuzzlePosition(mzrot, mzpos, emitter.MuzzlePositionLocal);
                            BulletEmittingUtility.EmitEffect(cmd, eqi, emitter.EffectPrefab, efpos, ref rnd);
                        }

                        {
                            var bulletData = bullets[emitter.BulletPrefab];
                            var rot = rots[mlink.EmitterEntity].Value;
                            var pos = poss[mlink.EmitterEntity].Value;

                            var bulletPos = BulletEmittingUtility.CalcMuzzlePosition(rot, pos, emitter.MuzzlePositionLocal);
                            var acc = BulletEmittingUtility.CalcAcc(gravity, bulletData.GravityFactor);
                            var range = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor;
                            //var spd = BulletEmittingUtility.CalcAimSpeed(, bulletData.AimFactor);


                            // 前回の発射が直前のフレームなら連続した発射間隔、はなれたフレームなら今フレームをベースにした発射間隔になる
                            var frameBaseTime = BulletEmittingUtility.CalcBaseTime(currentTime, state.NextEmitableTime, dt);

                            var d = currentTime - frameBaseTime;
                            var freq = (int)(d * math.rcp(emitter.EmittingInterval)) + 1;
                            //Debug.Log(freq);

                            state.NextEmitableTime = frameBaseTime + emitter.EmittingInterval * freq;

                            //for (var ifreq = 0; ifreq < freq; ifreq++)
                            //{
                            // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                            for (var i = 0; i < emitter.NumEmitMultiple * freq; i++)
                            {
                                var bulletDir = BulletEmittingUtility.CalcBulletDirection(rot, ref rnd, emitter.AccuracyRad);
                                var speed = bulletDir * bulletData.BulletSpeed;

                                BulletEmittingUtility.EmitBullet(cmd, eqi,
                                    emitter.BulletPrefab, slink.StateEntity,
                                    bulletPos, range, speed, acc, corps.TargetCorps);
                            }
                            //}

                            ////// 前回の発射が直前のフレームなら連続した発射間隔、はなれたフレームなら今フレームをベースにした発射間隔になる
                            ////var frameBaseTime = BulletEmittingUtility.CalcBaseTime(currentTime, state.NextEmitableTime, dt);

                            ////var nextTime = frameBaseTime;
                            ////do
                            ////{
                            ////    nextTime += emitter.EmittingInterval;

                            ////    // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                            ////    for (var i = 0; i < emitter.NumEmitMultiple; i++)
                            ////    {
                            ////        var bulletDir = BulletEmittingUtility.CalcBulletDirection(rot, ref rnd, emitter.AccuracyRad);
                            ////        var speed = bulletDir * bulletData.BulletSpeed;

                            ////        BulletEmittingUtility.EmitBullet(cmd, eqi,
                            ////            emitter.BulletPrefab, slink.StateEntity,
                            ////            bulletPos, range, speed, acc, corps.TargetCorps);
                            ////    }
                            ////}
                            ////while (currentTime >= nextTime);


                            ////state.NextEmitableTime = nextTime;
                        }
                    }
                )
                .ScheduleParallel();
        }

    }

}