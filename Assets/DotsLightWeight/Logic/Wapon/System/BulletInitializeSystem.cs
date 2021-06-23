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
    using DotsLite.Targeting;

    using Random = Unity.Mathematics.Random;

    static class BringYourOwnDelegate
    {
        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
            (
                T0 t0, T1 t1,
                ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6,
                in T7 t7, in T8 t8, in T9 t9
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
    [UpdateBefore(typeof(InitializeSystem))]
    public class BulletInitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var pdt = TimeEx.PrevDeltaTime;
            var dt = this.Time.DeltaTime;
            var gravity = UnityEngine.Physics.gravity.As_float3().As_float4();// とりあえず

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Entities
                .WithName("Misc")
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach((
                    int nativeThreadIndex, int entityInQueryIndex,
                    ref Translation pos,
                    ref Particle.VelocityFactorData vfact,
                    ref Particle.LifeTimeSpecData t,
                    ref Bullet.LinkData link,
                    ref CorpsGroup.TargetWithArmsData corps,
                    in Bullet.MoveSpecData spec,
                    in Particle.AdditionalData data,
                    in Bullet.InitializeFromEmitterData init) =>
                {
                    var tid = nativeThreadIndex;
                    var eqi = entityInQueryIndex;
                    var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));

                    var mpos = poss[init.BulletMuzzleEntity].Value;
                    var mrot = rots[init.BulletMuzzleEntity].Value;


                    var rad = init.EmitterAccuracyRad;
                    var dir = rnd.CalcBulletDirection(mrot, rad);
                    var v = init.AimSpeed.xyz * spec.AimFactor + dir * spec.BulletSpeed;
                    pos.Value = mpos;
                    vfact.PrePosition = (mpos - v * pdt).As_float4();
                    // pos に足すと進んでしまうので、前フレームから引く


                    var d = spec.RangeDistanceFactor * init.EmitterRangeDistanceFactor;
                    var rspd = math.rcp(spec.BulletSpeed);
                    t.DurationSec = math.min(t.DurationSec, d * rspd);


                    corps.TargetCorps = init.TargetCorps;
                    link.OwnerStateEntity = init.OwnerStateEntity;
                })
                .ScheduleParallel();
        }

    }


}

