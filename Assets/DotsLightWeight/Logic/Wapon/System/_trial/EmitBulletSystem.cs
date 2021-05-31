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
    [UpdateAfter(typeof(WaponTriggerSystem2))]
    [UpdateAfter(typeof(EmitTimeProgressSystem))]
    public class EmitBulletSystem2 : DependencyAccessableSystemBase
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
                        in Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.BulletEmittingData emitter,
                        in Emitter.BulletMuzzleLinkData muzzle,
                        in Emitter.OwnerLinkData slink,
                        //ref FunctionUnit.EmittingStateData state,
                        //ref FunctionUnit.TriggerData trigger,
                        //in FunctionUnit.BulletEmittingData emitter,
                        //in FunctionUnit.StateLinkData slink,
                        //in FunctionUnit.MuzzleLinkData mlink,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        var freq = state.EmitFrequencyInCurrentFrame;

                        if (!trigger.IsTriggered) return;
                        //if (currentTime < state.NextEmitableTime) return;
                        if (freq <= 0) return;
                        //Debug.Log(freq);


                        var bulletData = bullets[emitter.Prefab];
                        var rot = rots[muzzle.MuzzleEntity].Value;
                        var pos = poss[muzzle.MuzzleEntity].Value;


                        var init = emitter.CalcEmittingParams(pos, rot, muzzle.MuzzlePositionLocal);

                        //for (var ifreq = 0; ifreq < freq; ifreq++)
                        //{
                        // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                        for (var i = 0; i < emitter.NumEmitMultiple * freq; i++)
                            {
                                BulletEmittingUtility.EmitBullet(cmd, eqi,
                                    emitter.Prefab, slink.StateEntity,
                                    init, corps.TargetCorps);
                            }
                        //}
                    }
                )
                .ScheduleParallel();
        }

    }

}