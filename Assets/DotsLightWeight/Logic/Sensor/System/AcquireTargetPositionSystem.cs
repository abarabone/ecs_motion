using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

namespace DotsLite.Character
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Dependency;


    // メイン位置を持つ物体を、いったん単なる位置になおす
    // 移動処理に汎用性をもたせられる

    //[DisableAutoCreation]
    [UpdateAfter(typeof(FindNearestTargeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class AcquireTargetPosiionSystem : DependencyAccessableSystemBase
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


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<TargetSensor.AcqurireTag>()
                .WithReadOnly(poss)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref TargetSensorResponse.PositionData current,
                        in TargetSensor.LinkTargetMainData mainlink
                    )
                =>
                    {

                        if (mainlink.TargetMainEntity == Entity.Null)
                        {
                            cmd.RemoveComponent<TargetSensor.AcqurireTag>(entityInQueryIndex, entity);

                            return;
                        }

                        var targetPos = poss[mainlink.TargetMainEntity];

                        current.Position = targetPos.Value;
                    }
                )
                .ScheduleParallel();
        }


    }

}

