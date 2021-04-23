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

namespace Abarabone.Character
{
    using Abarabone.Dependency;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Geometry;
    using Abarabone.Physics;
    using Abarabone.Model;


    using Abarabone.CharacterMotion;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
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
            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Move.SpeedParamaterData speed,
                        ref Move.EasingSpeedData ease
                    )
                =>
                    {

                        var newspd = speed.SpeedPerSec +
                            (ease.TargetSpeedPerSec - speed.SpeedPerSec) * ease.Rate * dt;

                        speed.SpeedPerSec = math.clamp(newspd, 0.0f, speed.SpeedPerSecMax);


                        if (math.abs(speed.SpeedPerSec - ease.TargetSpeedPerSec) <= 0.01f)//float.MinValue)
                        {
                            cmd.RemoveComponent<Move.EasingSpeedData>(entityInQueryIndex, entity);
                        }

                    }
                )
                .ScheduleParallel();
        }

    }
}
