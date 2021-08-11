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
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(CameraMoveSystem))]
    [UpdateAfter(typeof(WaponTriggerSystem))]
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

            //var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            //var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            //this.Entities
            //    .WithBurst()
            //    //.WithNone<Bullet.MoveSpecData>()
            //    //.WithReadOnly(rots)
            //    .WithReadOnly(poss)
            //    .ForEach(
            //        (
            //            Entity entity, int entityInQueryIndex,
            //            in Emitter.StateData state,
            //            in Emitter.TriggerData trigger,
            //            in Emitter.EffectEmittingData emitter,
            //            //in Emitter.EffectMuzzleLinkData mzlink,
            //            in Emitter.OwnerLinkData slink,
            //            in CorpsGroup.TargetWithArmsData corps
            //        ) =>
            //        {
            //            var eqi = entityInQueryIndex;
            //            var freq = state.EmitFrequencyInCurrentFrame;

            //            if (!trigger.IsTriggered) return;
            //            if (freq <= 0) return;

            //            //var rot = rots[mzlink.MuzzleEntity].Value;
            //            var pos = poss[entity];// mzlink.MuzzleEntity];
            //            //var efpos = BulletEmittingUtility.CalcMuzzlePosition(rot, pos, mzlink.MuzzlePositionLocal.xyz);


            //            var ent = cmd.Instantiate(eqi, emitter.Prefab);
            //            cmd.SetComponent(eqi, ent, pos);
            //            //BulletEmittingUtility.EmitEffect(cmd, eqi, emitter.Prefab, efpos);
            //        }
            //    )
            //    .ScheduleParallel();

            this.Entities
                .WithBurst()
                //.WithNone<Emitter.EffectMuzzleLinkData>()
                //.WithNone<Bullet.MoveSpecData>()
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        in Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.EffectEmittingData emitter,
                        //in Emitter.OwnerLinkData slink,
                        in Translation pos
                        //in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        var freq = state.EmitFrequencyInCurrentFrame;

                        if (!trigger.IsTriggered) return;
                        if (freq <= 0) return;

                        var ent = cmd.Instantiate(eqi, emitter.Prefab);
                        cmd.SetComponent(eqi, ent, pos);
                        //BulletEmittingUtility.EmitEffect(cmd, eqi, emitter.Prefab, pos.Value);
                    }
                )
                .ScheduleParallel();
        }


    }

}