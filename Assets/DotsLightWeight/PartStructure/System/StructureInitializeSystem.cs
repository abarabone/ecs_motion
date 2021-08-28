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
    /// 　→ 
    /// ・ far/near の切り替え時にＴＦするっていう手もある？
    /// sleep on
    ///ＴＦ抑制 ボーンタグ消去
    ///wake up

    ///ＴＦオン ボーンタグ追加


    ///far/near

    ///enable/disable disable タグ追加／削除

    ///far/near with sleep

    ///near/farＴＦ一度 ボーンタグ追加＋oncetag
    ///oncetag

    ///enable/disable disable タグ追加／削除

    ///near with sleep

    ///near enable/far disable disable タグ追加／削除
    ///farＴＦ一度 ボーンタグ追加＋oncetag

    ///far with sleep

    ///far enable/near disable disable タグ追加／削除
    ///nearＴＦ一度 ボーンタグ追加＋oncetag
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform))]
    [UpdateAfter(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    public class RemoveStructureTransformOnceOnlySystem : DependencyAccessableSystemBase
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


            this.Entities
                .WithBurst()
                .WithAll<Main.MainTag>()
                .WithAll<Main.SleepingTag, Main.TransformOnlyOnceTag, Main.FarTag>()
                .WithReadOnly(linkedGroups)
                .WithReadOnly(parts)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    //ref Main.TransformOnceTag init,
                    in Main.BinderLinkData binder) =>
                {
                    var eqi = entityInQueryIndex;

                    //init.count++;
                    //if (init.count < 2) return;

                    // 最初の１回だけはトランスフォームが走るようにしたい
                    var children = linkedGroups[binder.BinderEntity];
                    cmd.RemoveComponentFromNearParts<Model.Bone.TransformTargetTag>(eqi, children, parts);

                    cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
                })
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithAll<Main.MainTag>()
                .WithAll<Main.SleepingTag, Main.TransformOnlyOnceTag, Main.NearTag>()
                .WithReadOnly(linkedGroups)
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    //ref Main.TransformOnceTag init,
                    in Main.BinderLinkData binder) =>
                {
                    var eqi = entityInQueryIndex;

                    //init.count++;
                    //if (init.count < 2) return;

                    // 最初の１回だけはトランスフォームが走るようにしたい
                    var children = linkedGroups[binder.BinderEntity];
                    cmd.RemoveComponentFromFar<Model.Bone.TransformTargetTag>(eqi, children);

                    cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
                })
                .ScheduleParallel();
        }


    }

}
