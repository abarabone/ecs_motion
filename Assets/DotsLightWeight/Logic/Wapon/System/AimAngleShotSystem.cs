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
    public class AimAngleShotSystem : DependencyAccessableSystemBase
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


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            //var bullets = this.GetComponentDataFromEntity<Bullet.>(isReadOnly: true);
            var targetposs = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);


            var dt = this.Time.DeltaTime;
            var currentTime = this.Time.ElapsedTime;
            var gravity = UnityEngine.Physics.gravity.As_float3();// ‚Æ‚è‚ ‚¦‚¸


            this.Entities
                .WithBurst()
                .WithReadOnly(targetposs)
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithNativeDisableContainerSafetyRestriction(poss)
                .WithNativeDisableContainerSafetyRestriction(rots)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref FunctionUnit.TriggerData trigger,
                        ref Rotation rot,
                        ref Translation pos,
                        in FunctionUnit.StateLinkData slink,
                        in FunctionUnitAiming.ParentBoneLinkData plink,
                        in FunctionUnitAiming.HighAngleShotData data
                    ) =>
                    {

                        trigger.IsTriggered = true;

                        var prot = rots[plink.ParentEntity].Value;
                        var ppos = poss[plink.ParentEntity].Value;

                        var targetpos = targetposs[data.TargetPostureEntity].Position + math.up() * 2.0f;
                        rot.Value = quaternion.LookRotationSafe(math.normalize(targetpos - ppos), Vector3.up);//prot;
                        pos.Value = ppos;


                        if (currentTime > data.EndTime)
                        {
                            cmd.RemoveComponent<FunctionUnitAiming.HighAngleShotData>
                                (entityInQueryIndex, entity);
                        }

                    }
                )
                .ScheduleParallel();
        }


    }

}