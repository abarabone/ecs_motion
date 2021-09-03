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

namespace DotsLite.Draw
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Structure;
    using System.Runtime.CompilerServices;
    using DotsLite.Dependency;


    /// <summary>
    /// ・常にトランスフォームが走るのは無駄
    /// ・スリープ中はトランスフォーム不要
    /// 　→スリープに切り替わる時にＴＦをオフにすればよい？
    /// 　→ far か near の disable になっているほうに反映されない
    /// ・切り替わる時に一度、両方トランスフォームすればいい？
    /// 　・コライダの位置を正しくするため → far, near
    /// 　・デブリの発生位置 → near
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform))]
    //[UpdateAfter(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class RemoveStructureTransformOnceOnlyTagSystem : DependencyAccessableSystemBase
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

            this.Entities
                .WithBurst()
                .WithAll<Main.TransformOnlyOnceTag>()
                .ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    var eqi = entityInQueryIndex;

                    //init.count++;
                    //if (init.count < 2) return;

                    cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
                    cmd.RemoveComponent<Model.Bone.TransformTargetTag>(eqi, entity);
                    cmd.AddComponent<Disabled>(eqi, entity);
                })
                .ScheduleParallel();

            //var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
            //var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            // far near を限定してしまってもよいのだろうか？途中で切り替わったりしない？

            //this.Entities
            //    .WithBurst()
            //    .WithNone<Main.SleepFirstTag>()
            //    .WithAll<Main.MainTag>()
            //    .WithAll<Main.SleepingTag, Main.TransformOnlyOnceTag, Main.FarTag>()
            //    .WithReadOnly(linkedGroups)
            //    .WithReadOnly(parts)
            //    .ForEach((
            //        Entity entity, int entityInQueryIndex,
            //        //ref Main.TransformOnceTag init,
            //        in Main.BinderLinkData binder) =>
            //    {
            //        var eqi = entityInQueryIndex;

            //        //init.count++;
            //        //if (init.count < 2) return;

            //        // 最初の１回だけはトランスフォームが走るようにしたい
            //        var children = linkedGroups[binder.BinderEntity];
            //        cmd.RemoveComponentFromNearParts<Model.Bone.TransformTargetTag>(eqi, children, parts);

            //        cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
            //        Debug.Log("sleep far");
            //    })
            //    .ScheduleParallel();

            //this.Entities
            //    .WithBurst()
            //    .WithNone<Main.SleepFirstTag>()
            //    .WithAll<Main.MainTag>()
            //    .WithAll<Main.SleepingTag, Main.TransformOnlyOnceTag, Main.NearTag>()
            //    .WithReadOnly(linkedGroups)
            //    .ForEach((
            //        Entity entity, int entityInQueryIndex,
            //        //ref Main.TransformOnceTag init,
            //        in Main.BinderLinkData binder) =>
            //    {
            //        var eqi = entityInQueryIndex;

            //        //init.count++;
            //        //if (init.count < 2) return;

            //        // 最初の１回だけはトランスフォームが走るようにしたい
            //        var children = linkedGroups[binder.BinderEntity];
            //        cmd.RemoveComponentFromFar<Model.Bone.TransformTargetTag>(eqi, children);

            //        cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
            //        Debug.Log("sleep near");
            //    })
            //    .ScheduleParallel();

            //this.Entities
            //    .WithBurst()
            //    .WithAll<Main.MainTag, Main.SleepFirstTag>()
            //    .WithAll<Main.SleepingTag>()//, Main.TransformOnlyOnceTag>()
            //    .WithReadOnly(linkedGroups)
            //    .WithReadOnly(parts)
            //    .ForEach((
            //        Entity entity, int entityInQueryIndex,
            //        //ref Main.TransformOnlyOnceTag init,
            //        in Main.BinderLinkData binder) =>
            //    {
            //        var eqi = entityInQueryIndex;

            //        //init.count++;
            //        //if (init.count < 2) return;

            //        // 最初の１回だけはトランスフォームが走るようにしたい
            //        var children = linkedGroups[binder.BinderEntity];
            //        cmd.RemoveComponentFromFar<Model.Bone.TransformTargetTag>(eqi, children);
            //        //cmd.RemoveComponentFromFar<Disabled>(eqi, children);
            //        cmd.RemoveComponentsFromNearParts<Disabled, Model.Bone.TransformTargetTag>(eqi, children, parts);

            //        cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
            //        cmd.RemoveComponent<Main.SleepFirstTag>(eqi, entity);
            //        Debug.Log("sleep first");
            //    })
            //    .ScheduleParallel();
        }


    }

}
