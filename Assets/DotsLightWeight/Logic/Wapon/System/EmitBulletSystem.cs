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
    //            T0 t0, T1 t1, T2 t2,
    //            ref T3 t3,
    //            in T4 t4, in T5 t5, in T6 t6, in T7 t7, in T8 t8
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


                        if (emitter.EffectPrefab != Entity.Null)
                        {
                            var mzrot = rots[mlink.MuzzleEntity].Value;
                            var mzpos = poss[mlink.MuzzleEntity].Value;
                            var efpos = BulletEmittingUtility.CalcMuzzlePosition(mzrot, mzpos, emitter.MuzzlePositionLocal);
                            BulletEmittingUtility.EmitEffect(cmd, eqi, emitter.EffectPrefab, efpos);
                        }

                        {
                            var bulletData = bullets[emitter.BulletPrefab];
                            var rot = rots[mlink.EmitterEntity].Value;
                            var pos = poss[mlink.EmitterEntity].Value;

                            var mzpos = BulletEmittingUtility.CalcMuzzlePosition(rot, pos, emitter.MuzzlePositionLocal);
                            var init = emitter.CreateInitData(pos, rot);
                            var frameBaseTime = BulletEmittingUtility.CalcBaseTime(currentTime, state.NextEmitableTime, dt);
                            var freq = BulletEmittingUtility.CalcFreq(currentTime, frameBaseTime, emitter.EmittingInterval);

                            //Debug.Log(freq);
                            //for (var ifreq = 0; ifreq < freq; ifreq++)
                            //{
                            // それぞれ別のエンティティに振り分けたほうが、ジョブの粒度が平均化に近づくかも…
                            for (var i = 0; i < emitter.NumEmitMultiple * freq; i++)
                            {
                                BulletEmittingUtility.EmitBullet(cmd, eqi,
                                    emitter.BulletPrefab, slink.StateEntity, mzpos, init, corps.TargetCorps);
                            }
                            //}

                            state.NextEmitableTime = frameBaseTime + emitter.EmittingInterval * freq;
                        }
                    }
                )
                .ScheduleParallel();
        }

    }

}