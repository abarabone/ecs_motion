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
    /// �E��Ƀg�����X�t�H�[��������͖̂���
    /// �E�X���[�v���̓g�����X�t�H�[���s�v
    /// �@���X���[�v�ɐ؂�ւ�鎞�ɂs�e���I�t�ɂ���΂悢�H
    /// �@�� far �� near �� disable �ɂȂ��Ă���ق��ɔ��f����Ȃ�
    /// �E�؂�ւ�鎞�Ɉ�x�A�����g�����X�t�H�[������΂����H
    /// �@�E�R���C�_�̈ʒu�𐳂������邽�� �� far, near
    /// �@�E�f�u���̔����ʒu �� near
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

                    cmd.RemoveComponent<Model.Bone.TransformTargetTag>(eqi, entity);
                    cmd.RemoveComponent<Main.TransformOnlyOnceTag>(eqi, entity);
                })
                .ScheduleParallel();

            //var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
            //var parts = this.GetComponentDataFromEntity<Part.PartData>(isReadOnly: true);

            // far near �����肵�Ă��܂��Ă��悢�̂��낤���H�r���Ő؂�ւ�����肵�Ȃ��H

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

            //        // �ŏ��̂P�񂾂��̓g�����X�t�H�[��������悤�ɂ�����
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

            //        // �ŏ��̂P�񂾂��̓g�����X�t�H�[��������悤�ɂ�����
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

            //        // �ŏ��̂P�񂾂��̓g�����X�t�H�[��������悤�ɂ�����
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