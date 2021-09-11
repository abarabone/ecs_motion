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

    using DotsLite.Utility.Log.NoShow;

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
            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            const float l = 0.01f;
            var limit = new float3(l, l, l);// l * l, l * l, l * l);

            //var curtime = (float)this.Time.ElapsedTime;
            var dt = this.Time.DeltaTime;

            this.Entities
                .WithName("far")
                .WithBurst()
                .WithNone<Main.SleepingTag>()
                .WithAll<Main.FarTag>()
                //.WithAny<Main.FarTag, Main.NearTag>()
                .WithReadOnly(parts)
                .WithReadOnly(linkedGroups)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref Main.SleepTimerData timer,
                    in Main.BinderLinkData binder,
                    in Draw.DrawInstance.ModelLinkData model,
                    in Draw.DrawInstance.ModelLod2LinkData mlink,
                    in Translation pos) =>
                {
                    var eqi = entityInQueryIndex;


                    if (!isTimerCompleted_(in timer))
                    {
                        progressTimer_IfNotMove_(ref timer, in pos);
                        return;
                    }


                    // 直接判別してしまうと、near になる前の位置でＴＦしてしまうのでだめ
                    //if (model.DrawModelEntityCurrent == mlink.DrawModelEntityNear)
                    //{
                        //resetTimer_(ref timer);

                        //cmd.ChangeComponentsToSleepOnNear(entity, eqi, binder, parts, linkedGroups);
                        //_._log("to sleep near");
                        //return;
                    //}
                    //if (model.DrawModelEntityCurrent == mlink.DrawModelEntityFar) // null も含めるため
                    {
                        resetTimer_(ref timer);

                        cmd.ChangeComponentsToSleepOnFar(entity, eqi, binder, parts, linkedGroups);
                        _._log("to sleep far");
                        return;
                    }
                })
                .ScheduleParallel();

            //// なんかできない
            //var dep2 = this.Entities
            //    .WithName("near")
            //    .WithBurst()
            //    .WithNone<Main.SleepingTag>()
            //    .WithAll<Main.NearTag>()
            //    .WithReadOnly(parts)
            //    .WithReadOnly(linkedGroups)
            //    .ForEach((
            //        Entity entity, int entityInQueryIndex,
            //        ref Main.SleepTimerData timer,
            //        in Main.BinderLinkData binder,
            //        in Translation pos) =>
            //    {
            //        var eqi = entityInQueryIndex;


            //        if (isTimerCompleted_(in timer))
            //        {
            //            resetTimer_(ref timer);
            //            //changeComponentsToSleep_(in binder);
            //            cmd.ChangeComponentsToSleepOnNear(entity, eqi, binder, parts, linkedGroups);
            //            //Debug.Log("to sleep near");
            //            return;
            //        }

            //        progressTimer_IfNotMove_(ref timer, in pos);
            //        return;


            //        //void changeComponentsToSleep_(in Main.BinderLinkData binder) =>
            //        //    cmd.ChangeComponentsToSleepOnNear(entity, eqi, binder, parts, linkedGroups);

            //    })
            //    .ScheduleParallel(this.Dependency);

            //this.Dependency = JobHandle.CombineDependencies(dep0, dep1, dep2);
            //return;

            //this.Dependency = JobHandle.CombineDependencies(dep0, dep1);
            return;


            static bool isTimerCompleted_(in Main.SleepTimerData timer) =>
                timer.StillnessTime >= Main.SleepTimerData.Margin;

            static void resetTimer_(ref Main.SleepTimerData timer) =>
                timer.PrePositionAndTime = 0.0f;

            void progressTimer_IfNotMove_(ref Main.SleepTimerData timer, in Translation pos)
            {
                //var isStillness = math.all(math.abs(v.Linear) < limit) & math.all(math.abs(v.Angular) < limit);
                var isStillness = math.all(math.abs(pos.Value - timer.PrePosition) < limit);

                timer.PrePositionAndTime = math.select(
                    new float4(pos.Value, 0.0f),
                    new float4(pos.Value, timer.StillnessTime + dt),
                    isStillness);
            }
        }
    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class StructureEnvelopeSleepSwitchingSystem_near : DependencyAccessableSystemBase
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
            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            const float l = 0.01f;
            var limit = new float3(l, l, l);// l * l, l * l, l * l);

            //var curtime = (float)this.Time.ElapsedTime;
            var dt = this.Time.DeltaTime;

            this.Entities
                .WithName("near")
                .WithBurst()
                .WithNone<Main.SleepingTag>()
                .WithAll<Main.NearTag>()
                .WithReadOnly(parts)
                .WithReadOnly(linkedGroups)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref Main.SleepTimerData timer,
                    in Main.BinderLinkData binder,
                    in Translation pos) =>
                {
                    var eqi = entityInQueryIndex;


                    if (isTimerCompleted_(in timer))
                    {
                        resetTimer_(ref timer);
                        cmd.ChangeComponentsToSleepOnNear(entity, eqi, binder, parts, linkedGroups);
                        _._log("to sleep near");
                        return;
                    }

                    progressTimer_IfNotMove_(ref timer, in pos);
                })
                .ScheduleParallel();

            return;


            static bool isTimerCompleted_(in Main.SleepTimerData timer) =>
                timer.StillnessTime >= Main.SleepTimerData.Margin;

            static void resetTimer_(ref Main.SleepTimerData timer) =>
                timer.PrePositionAndTime = 0.0f;

            void progressTimer_IfNotMove_(ref Main.SleepTimerData timer, in Translation pos)
            {
                //var isStillness = math.all(math.abs(v.Linear) < limit) & math.all(math.abs(v.Angular) < limit);
                var isStillness = math.all(math.abs(pos.Value - timer.PrePosition) < limit);

                timer.PrePositionAndTime = math.select(
                    new float4(pos.Value, 0.0f),
                    new float4(pos.Value, timer.StillnessTime + dt),
                    isStillness);
            }
        }
    }


    ///// <summary>
    ///// とりあえず、far/near なしのスリープはここでやる
    ///// すべての子を一度トランスフォームする
    ///// </summary>
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //public class StructureEnvelopeSleepSwitchingSystem_First : DependencyAccessableSystemBase
    //{


    //    CommandBufferDependency.Sender cmddep;


    //    protected override void OnCreate()
    //    {
    //        base.OnCreate();

    //        this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    //    }

    //    protected override void OnUpdate()
    //    {
    //        using var cmdScope = this.cmddep.WithDependencyScope();


    //        var cmd = cmdScope.CommandBuffer.AsParallelWriter();

    //        var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
    //        var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

    //        const float l = 0.01f;
    //        var limit = new float3(l, l, l);// l * l, l * l, l * l);

    //        //var curtime = (float)this.Time.ElapsedTime;
    //        var dt = this.Time.DeltaTime;

    //        this.Entities
    //            .WithName("all")
    //            .WithBurst()
    //            .WithNone<Main.SleepingTag>()
    //            .WithNone<Main.FarTag, Main.NearTag>()
    //            .WithReadOnly(parts)
    //            .WithReadOnly(linkedGroups)
    //            .ForEach((
    //                Entity entity, int entityInQueryIndex,
    //                ref Main.SleepTimerData timer,
    //                in Main.BinderLinkData binder,
    //                in Draw.DrawInstance.ModelLinkData model,
    //                in Draw.DrawInstance.ModelLod2LinkData mlink,
    //                in Translation pos) =>
    //            {
    //                var eqi = entityInQueryIndex;


    //                if (!isTimerCompleted_(in timer))
    //                {
    //                    progressTimer_IfNotMove_(ref timer, in pos);
    //                    return;
    //                }

    //                {
    //                    resetTimer_(ref timer);

    //                    cmd.ChangeComponentsToSleep(entity, eqi, binder, parts, linkedGroups);
    //                    _._log("to sleep first");
    //                    return;
    //                }
    //            })
    //            .ScheduleParallel();

    //        return;


    //        static bool isTimerCompleted_(in Main.SleepTimerData timer) =>
    //            timer.StillnessTime >= Main.SleepTimerData.Margin;

    //        static void resetTimer_(ref Main.SleepTimerData timer) =>
    //            timer.PrePositionAndTime = 0.0f;

    //        void progressTimer_IfNotMove_(ref Main.SleepTimerData timer, in Translation pos)
    //        {
    //            //var isStillness = math.all(math.abs(v.Linear) < limit) & math.all(math.abs(v.Angular) < limit);
    //            var isStillness = math.all(math.abs(pos.Value - timer.PrePosition) < limit);

    //            timer.PrePositionAndTime = math.select(
    //                new float4(pos.Value, 0.0f),
    //                new float4(pos.Value, timer.StillnessTime + dt),
    //                isStillness);
    //        }
    //    }
    //}
}