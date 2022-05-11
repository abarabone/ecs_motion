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
    using DotsLite.ParticleSystem;
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
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(WaponTriggerSystem))]
    public partial class EmitTimeProgressSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;


            this.Entities
                .WithBurst()
                //.WithNone<Bullet.MoveSpecData>()
                .ForEach(
                    (
                        ref Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.BulletEmittingData emitter
                    ) =>
                    {
                        if (!trigger.IsTriggered) return;


                        var frameBaseTime = BulletEmittingUtility.CalcBaseTime(currentTime, state.NextEmitableTime, dt);
                        var freq = BulletEmittingUtility.CalcFreq(currentTime, frameBaseTime, emitter.EmittingIntervalRcp);

                        //Debug.Log($"current:{currentTime:0.00} freq:{freq} base:{frameBaseTime:0.00} next:{state.NextEmitableTime:0.00} dt:{dt:0.00}");
                        state.EmitFrequencyInCurrentFrame = freq;
                        state.NextEmitableTime = frameBaseTime + emitter.EmittingInterval * freq;
                    }
                )
                .ScheduleParallel();
        }


    }

}