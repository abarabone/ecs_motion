//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Physics;
//using Unity.Physics.Systems;
//using UnityEngine.InputSystem;

//using Collider = Unity.Physics.Collider;
//using SphereCollider = Unity.Physics.SphereCollider;

//namespace DotsLite.Draw
//{
//    using DotsLite.Misc;
//    using DotsLite.Utilities;
//    using DotsLite.SystemGroup;
//    using DotsLite.Character;
//    using DotsLite.Structure;
//    using System.Runtime.CompilerServices;
//    using DotsLite.Dependency;


//    /// <summary>
//    /// ・常にトランスフォームが走るのは無駄
//    /// ・スリープ中はトランスフォーム不要
//    /// 　→スリープに切り替わる時にＴＦをオフにすればよい？
//    /// 　→ far か near の disable になっているほうに反映されない
//    /// ・切り替わる時に一度、両方トランスフォームすればいい？
//    /// 　・コライダの位置を正しくするため → far, near
//    /// 　・デブリの発生位置 → near
//    /// 　→ 
//    /// ・ far/near の切り替え時にＴＦするっていう手もある？
//    /// </summary>
//    [DisableAutoCreation]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform))]
//    [UpdateAfter(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
//    public class StructureTransformInitializeSystem : DependencyAccessableSystemBase
//    {


//        CommandBufferDependency.Sender cmddep;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
//        }

//        protected override void OnUpdate()
//        {
//            using var cmdScope = this.cmddep.WithDependencyScope();


//            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

//            var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
//            var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);


//            this.Entities
//                .WithBurst()
//                //.WithAll<Main.MainTag, Structure.Main.TransformOnceTag>()
//                .WithReadOnly(linkedGroups)
//                .WithReadOnly(parts)
//                .ForEach((
//                    Entity entity, int entityInQueryIndex,
//                    ref Main.TransformOnceTag init,
//                    in Main.BinderLinkData binder) =>
//                {
//                    var eqi = entityInQueryIndex;

//                    init.count++;
//                    if (init.count < 2) return;

//                    // 最初の１回だけはトランスフォームが走るようにしたい
//                    var children = linkedGroups[binder.BinderEntity];
//                    children.AddComponentsToAllBones<Model.TransformOption.ExcludeTransformTag>(cmd, eqi, parts);


//                    cmd.RemoveComponent<Structure.Main.TransformOnceTag>(eqi, entity);
//                })
//                .ScheduleParallel();
//        }


//    }

//}
