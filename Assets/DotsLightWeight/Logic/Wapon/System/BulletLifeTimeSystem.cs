﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Draw;
    using Abarabone.Particle;
    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    //[UpdateAfter(typeof())]
    public class BulletLifeTimeSystem : SystemBase
    {


        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Particle.AdditionalData additional,
                        ref Bullet.LifeTimeData timer,
                        in Bullet.DistanceData dist
                    ) =>
                    {

                        timer.LifeTime -= deltaTime;

                        var transparency = math.max(timer.LifeTime, 0.0f) * timer.InvTotalTime;
                        additional.Color = additional.Color.ApplyAlpha(transparency);

                        if (timer.LifeTime <= 0.0f | dist.RestRangeDistance <= 0.0f)
                        {
                            cmd.DestroyEntity(entityInQueryIndex, entity);
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

    }


}

