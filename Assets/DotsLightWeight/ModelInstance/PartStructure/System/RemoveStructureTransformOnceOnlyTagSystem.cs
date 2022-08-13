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
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform.MonolithicBone))]
    //[UpdateAfter(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class RemoveStructureTransformOnceOnlyTagSystem : DependencyAccessableSystemBase
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
                .WithAll<PartBone.TransformOnlyOnceTag>()
                .ForEach((Entity entity, int entityInQueryIndex) =>//, ref Bone.TransformOnlyOnceTag init) =>
                {
                    //if (init.count++ < 1) return;// 暫定　できればタグ操作だけでやりたいんだけど…

                    var eqi = entityInQueryIndex;
                    cmd.RemoveComponent<PartBone.TransformOnlyOnceTag>(eqi, entity);
                    cmd.RemoveComponent<Model.Bone.TransformTargetTag>(eqi, entity);
                    cmd.AddComponent<Disabled>(eqi, entity);
                })
                .ScheduleParallel();

        }


    }

}
