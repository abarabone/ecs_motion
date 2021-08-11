using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace DotsLite.Character
{
    using DotsLite.Dependency;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Geometry;
    using DotsLite.Collision;
    using DotsLite.Model;


    using DotsLite.CharacterMotion;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    public class EasingMoveSpeedSystem : DependencyAccessableSystemBase
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


            var dt = this.Time.DeltaTime;
            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            this.Entities
                .WithBurst()
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref Move.SpeedParamaterData speed,
                    ref Move.EasingSpeedData ease)
                =>
                    {

                        //var newspd = speed.SpeedPerSec +
                        //    (ease.TargetSpeedPerSec - speed.SpeedPerSec) * ease.Rate * dt;

                        //speed.SpeedPerSec = math.clamp(newspd, 0.0f, speed.SpeedPerSecMax);


                        var cur = speed.SpeedPerSec;
                        var tar = ease.TargetSpeedPerSec;
                        var max = speed.SpeedPerSecMax;
                        var rate = ease.Rate;

                        var newspd = cur + (tar - cur) * rate * dt;
                        var latest = math.clamp(newspd, 0.0f, max);

                        speed.SpeedPerSec = latest;

                        var isAlmostSame = math.abs(latest - tar) <= math.abs(max * 0.01f);
                        if (isAlmostSame)
                        {
                            speed.SpeedPerSec = ease.TargetSpeedPerSec;

                            cmd.RemoveComponent<Move.EasingSpeedData>(entityInQueryIndex, entity);
                        }

                    }
                )
                .ScheduleParallel();
        }

    }
}
