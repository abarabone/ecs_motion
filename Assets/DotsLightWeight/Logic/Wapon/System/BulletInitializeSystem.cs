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

    //static class BringYourOwnDelegate
    //{
    //    // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
    //    [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
    //    public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    //        (
    //            T0 t0, T1 t1,
    //            ref T2 t2, ref T3 t3, ref T4 t4,
    //            in T5 t5, in T6 t6, in T7 t7, in T8 t8
    //        );

    //    // Declare the function overload
    //    public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8>
    //        (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8> codeToRun)
    //        where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
    //    =>
    //        LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    //}

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(DotsLite.Particle.InitializeSystem))]
    public class BulletInitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var gravity = UnityEngine.Physics.gravity.As_float3().As_float4();// とりあえず

            //var rnds = this.World.GetExistingSystem<RandomSystem>().RandomArray;


            this.Entities
                .WithName("Misc")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    int nativeThreadIndex, int entityInQueryIndex,
                    ref Translation pos,
                    //ref Bullet.VelocityData v,
                    ref Particle.VelocityFactorData vfact,
                    ref Particle.LifeTimeSpecData t,
                    in Bullet.MoveSpecData spec,
                    in Particle.AdditionalData data,
                    in Bullet.InitializeFromEmitterData init) =>
                {
                    var tid = nativeThreadIndex;
                    var eqi = entityInQueryIndex;
                    var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));
                    var prepos = pos.Value;


                    var rot = init.EmitterRotation;
                    var rad = init.EmitterAccuracyRad;
                    var dir = BulletEmittingUtility.CalcBulletDirection(rot, ref rnd, rad);
                    var v = init.AimSpeed.xyz * spec.AimFactor + dir * spec.BulletSpeed;
                    //pos.Value += v * dt;


                    vfact.PrePosition = (prepos - v * TimeEx.PrevDeltaTime).As_float4();
                    // pos に足すと進んでしまうので、前フレームから引く


                    var d = spec.RangeDistanceFactor * init.EmitterRangeDistanceFactor;
                    var rspd = math.rcp(spec.BulletSpeed);
                    t.DurationSec = math.min(t.DurationSec, d * rspd);
                })
                .ScheduleParallel();
        }

    }


}

