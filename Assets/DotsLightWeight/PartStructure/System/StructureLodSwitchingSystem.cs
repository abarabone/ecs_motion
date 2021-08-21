﻿using System.Collections;
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
        /// linked entity 番号を固定で使ってしまったので、問題でたらちゃんとなおさなければならない
        /// 
        /// 全オブジェクトを毎フレーム処理するのは無駄なので、コリジョンなどで処理したほうがよさそう
        /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class StructureLodSwitchingSystem : DependencyAccessableSystemBase
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
            var disableds = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<Main.MainTag>()
                .WithReadOnly(linkedGroups)
                //.WithReadOnly(excludes)
                .WithReadOnly(parts)
                .WithReadOnly(disableds)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in Main.BinderLinkData binder,
                        in DrawInstance.ModelLinkData model,
                        in DrawInstance.ModelLod2LinkData lod2
                    )
                =>
                    {
                        var eqi = entityInQueryIndex;
                        var children = linkedGroups[binder.BinderEntity];
                        

                        var isNearComponent = disableds.HasComponent(children[2].Value);
                        var isNearModel = model.DrawModelEntityCurrent == lod2.DrawModelEntityNear;

                        if (isNearModel & !isNearComponent)
                        {
                            children.ChangeToNear(cmd, eqi, parts);
                        }


                        var isFarComponent = !isNearComponent;
                        var isFarModel = model.DrawModelEntityCurrent == lod2.DrawModelEntityFar;

                        if (isFarModel & !isFarComponent)
                        {
                            children.ChangeToFar(cmd, eqi, parts);
                        }

                    }
                )
                .ScheduleParallel();
        }


    }

}
