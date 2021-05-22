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
    public class EmitEffectSystem2 : DependencyAccessableSystemBase
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

            //var bullets = this.GetComponentDataFromEntity<Bullet.MoveSpecData>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// とりあえず


            this.Entities
                .WithBurst()
                .WithNone<Bullet.MoveSpecData>()
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                //.WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Emitter.StateData state,
                        in Emitter.TriggerData trigger,
                        in Emitter.EffectEmittingData emitter,
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

                        if (!trigger.IsTriggered) return;


                        if (currentTime < state.NextEmitableTime) return;

                        var rnd = Random.CreateFromIndex((uint)eqi + (uint)math.asuint(dt) & 0x_7fff_ffff);

                        var mzrot = rots[emitter.MuzzleEntity].Value;
                        var mzpos = poss[emitter.MuzzleEntity].Value;
                        var efpos = calcPosition_(mzrot, mzpos, emitter);

                        emit_(cmd, eqi, emitter.Prefab, efpos, rnd);
                    }
                )
                .ScheduleParallel();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcPosition_(
            quaternion rot, float3 pos, in Emitter.EffectEmittingData emitter)
        {

            var muzpos = pos + math.mul(rot, emitter.MuzzlePositionLocal);

            return muzpos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_(
            EntityCommandBuffer.ParallelWriter cmd, int eqi, Entity effectPrefab,
            float3 pos, Random rnd)
        {
            //if (effectPrefab == Entity.Null) return;

            var ent = cmd.Instantiate(eqi, effectPrefab);

            cmd.SetComponent(eqi, ent, new Translation
            {
                Value = pos,
            });
            cmd.SetComponent(eqi, ent, new BillBoad.RotationData
            {
                Direction = rnd.NextFloat2Direction() * rnd.NextFloat(0.8f, 1.2f),
            });
        }


    }

}