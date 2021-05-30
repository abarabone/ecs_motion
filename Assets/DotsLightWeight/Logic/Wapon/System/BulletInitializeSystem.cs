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

    static class BringYourOwnDelegate
    {
        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
            (
                T0 t0, T1 t1,
                ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5,
                in T6 t6, in T7 t7, in T8 t8, in T9 t9
            );

        // Declare the function overload
        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
            (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> codeToRun)
            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
        =>
            LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    }

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class BulletInitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var gravity = UnityEngine.Physics.gravity.As_float3().As_float4();// ‚Æ‚è‚ ‚¦‚¸

            //var rnds = this.World.GetExistingSystem<RandomSystem>().RandomArray;


            this.Entities
                .WithName("Initialize")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    int nativeThreadIndex, int entityInQueryIndex,
                    ref Particle.TranslationTailData tail,
                    ref Bullet.VelocityData v,
                    ref Bullet.AccelerationData acc,
                    ref Particle.LifeTimeSpecData t,
                    in Translation pos,
                    in Bullet.MoveSpecData spec,
                    in Particle.AdditionalData data,
                    in Bullet.InitializeFromEmitterData init) =>
                {
                    var tid = nativeThreadIndex;
                    var eqi = entityInQueryIndex;
                    var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));


                    tail.PositionAndSize = pos.Value.As_float4(data.Size);

                    var rot = init.EmitterRotation;
                    var rad = init.EmitterAccuracyRad;
                    var dir = BulletEmittingUtility.CalcBulletDirection(rot, ref rnd, rad);
                    v.Velocity = init.AimSpeed * spec.AimFactor + (dir * spec.BulletSpeed).As_float4();

                    acc.Acceleration = gravity * spec.GravityFactor;

                    var d = spec.RangeDistanceFactor * init.EmitterRangeDistanceFactor;
                    var rspd = math.rcp(spec.BulletSpeed);
                    t.DurationSec = math.min(t.DurationSec, d * rspd);
                })
                .ScheduleParallel();

        }

    }


}

