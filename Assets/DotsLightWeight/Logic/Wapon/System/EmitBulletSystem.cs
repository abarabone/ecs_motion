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

    //static class BringYourOwnDelegate2
    //{
    //    // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
    //    [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
    //    public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    //        (
    //            T0 t0, T1 t1,
    //            in T2 t2, in T3 t3, in T4 t4, in T5 t5, in T6 t6, in T7 t7, in T8 t8
    //        );

    //    // Declare the function overload
    //    public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8>
    //        (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8> codeToRun)
    //        where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
    //    =>
    //        LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    //}


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(CameraMoveSystem))]
    [UpdateAfter(typeof(WaponTriggerSystem))]
    [UpdateAfter(typeof(EmitTimeProgressSystem))]
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
            //var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithName("WithCameraMuzzle")
                .WithBurst()
                //.WithNone<Bullet.MoveSpecData>()
                //.WithReadOnly(rots)
                //.WithReadOnly(poss)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.BulletEmittingData emitter,
                        in Emitter.BulletMuzzleLinkData mzlink,
                        //in Emitter.EffectMuzzleLinkData eflink,
                        in Emitter.OwnerLinkData slink,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        var freq = state.EmitFrequencyInCurrentFrame;

                        if (!trigger.IsTriggered) return;
                        if (freq <= 0) return;
                        //Debug.Log(freq);


                        var init = new Bullet.InitializeFromEmitterData
                        {
                            EmitterAccuracyRad = emitter.AccuracyRad,
                            EmitterRangeDistanceFactor = emitter.RangeDistanceFactor,
                            AimSpeed = 0,
                            BulletMuzzleEntity = mzlink.MuzzleEntity,
                            EffectMuzzleEntity = entity,//eflink.MuzzleEntity,
                            OwnerStateEntity = slink.StateEntity,
                            TargetCorps = corps.TargetCorps,
                        };

                        //for (var ifreq = 0; ifreq < freq; ifreq++)
                        //{
                        // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                        for (var i = 0; i < emitter.NumEmitMultiple * freq; i++)
                            {
                                var newBullet = cmd.Instantiate(eqi, emitter.Prefab);
                                cmd.SetComponent(eqi, newBullet, init);
                            }
                        //}
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithName("WithoutCameraMuzzle")
                .WithBurst()
                .WithNone<Emitter.BulletMuzzleLinkData>()
                //.WithNone<Bullet.MoveSpecData>()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.BulletEmittingData emitter,
                        in Emitter.OwnerLinkData slink,
                        in CorpsGroup.TargetWithArmsData corps
                    ) =>
                    {
                        var eqi = entityInQueryIndex;
                        var freq = state.EmitFrequencyInCurrentFrame;

                        if (!trigger.IsTriggered) return;
                        if (freq <= 0) return;
                        //Debug.Log(freq);


                        var init = new Bullet.InitializeFromEmitterData
                        {
                            EmitterAccuracyRad = emitter.AccuracyRad,
                            EmitterRangeDistanceFactor = emitter.RangeDistanceFactor,
                            AimSpeed = 0,
                            BulletMuzzleEntity = entity,
                            EffectMuzzleEntity = entity,
                            OwnerStateEntity = slink.StateEntity,
                            TargetCorps = corps.TargetCorps,
                        };


                        //for (var ifreq = 0; ifreq < freq; ifreq++)
                        //{
                        // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                        for (var i = 0; i < emitter.NumEmitMultiple * freq; i++)
                        {
                            var newBullet = cmd.Instantiate(eqi, emitter.Prefab);
                            cmd.SetComponent(eqi, newBullet, init);
                        }
                        //}
                    }
                )
                .ScheduleParallel();
        }

    }

}