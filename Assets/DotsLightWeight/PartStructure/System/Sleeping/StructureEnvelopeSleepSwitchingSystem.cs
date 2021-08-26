using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Structure
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Structure;
    using System.Runtime.CompilerServices;
    using DotsLite.Dependency;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class StructureEnvelopeSleepSwitchingSystem : DependencyAccessableSystemBase
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

            var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
            //var excludes = this.GetComponentDataFromEntity<PhysicsExclude>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);
            //var disableds = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);

            const float l = 0.01f;
            var limit = new float3(l, l, l);// l * l, l * l, l * l);

            //var curtime = (float)this.Time.ElapsedTime;
            var dt = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<PhysicsVelocity>()
                .WithReadOnly(parts)
                .WithReadOnly(linkedGroups)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref Main.SleepTimerData timer,
                    in Main.BinderLinkData binder,
                    in Translation pos) =>
                    //in PhysicsVelocity v) =>
                {
                    var eqi = entityInQueryIndex;


                    if (isTimerCompleted_(in timer))
                    {
                        resetTimer_(ref timer);
                        changeComponentsToSleep_(in binder);
                        return;
                    }

                    progressTimer_IfNotMove_(ref timer, in pos);
                    return;


                    bool isTimerCompleted_(in Main.SleepTimerData timer) =>
                        timer.StillnessTime >= Main.SleepTimerData.Margin;

                    void resetTimer_(ref Main.SleepTimerData timer) =>
                        timer.PrePositionAndTime = 0.0f;

                    void changeComponentsToSleep_(in Main.BinderLinkData binder) =>
                        cmd.ChangeComponentsToSleep(entity, eqi, binder, parts, linkedGroups);

                    void progressTimer_IfNotMove_(ref Main.SleepTimerData timer, in Translation pos)
                    {
                        //var isStillness = math.all(math.abs(v.Linear) < limit) & math.all(math.abs(v.Angular) < limit);
                        var isStillness = math.all(math.abs(pos.Value - timer.PrePosition) < limit);

                        timer.PrePositionAndTime = math.select(
                            new float4(pos.Value, 0.0f),
                            new float4(pos.Value, timer.StillnessTime + dt),
                            isStillness);
                    }
                })
                .ScheduleParallel();
        }
    }
}
