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


    /// <summary>
    /// １つだけ放出する。
    /// trigger.IsTriggered が真のとき、放出される。
    /// 現状は、bullet の state.NextEmitableTime 更新に依存する。
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(CameraMoveSystem))]
    [UpdateAfter(typeof(WaponTriggerSystem2))]
    [UpdateAfter(typeof(EmitTimeProgressSystem))]
    public class EmitEffectSystem : DependencyAccessableSystemBase
    {


        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            //var bullets = this.GetComponentDataFromEntity<Bullet.MoveSpecData>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithBurst()
                //.WithNone<Bullet.MoveSpecData>()
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                //.WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        in Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.EffectEmittingData emitter,
                        in Emitter.EffectMuzzleLinkData muzzle,
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
                        

                        var mzrot = rots[muzzle.MuzzleEntity].Value;
                        var mzpos = poss[muzzle.MuzzleEntity].Value;
                        var efpos = BulletEmittingUtility.CalcMuzzlePosition(mzrot, mzpos, muzzle.MuzzlePositionLocal);

                        BulletEmittingUtility.EmitEffect(cmd, eqi, emitter.Prefab, efpos);
                    }
                )
                .ScheduleParallel();
        }


    }

}