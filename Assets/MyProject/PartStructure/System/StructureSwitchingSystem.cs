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

namespace Abarabone.Draw
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Structure;
    using System.Runtime.CompilerServices;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    //[UpdateAfter(typeof())]
    public class StructureSwitchingSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        /// <summary>
        /// linked entity 番号を固定で使ってしまったので、問題でたらちゃんとなおさなければならない
        /// </summary>
        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

            var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
            //var excludes = this.GetComponentDataFromEntity<PhysicsExclude>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);
            var disableds = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<Structure.MainTag>()
                .WithReadOnly(linkedGroups)
                //.WithReadOnly(excludes)
                .WithReadOnly(parts)
                .WithReadOnly(disableds)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in ObjectMain.BinderLinkData binder,
                        in DrawInstance.ModeLinkData model,
                        in DrawInstance.ModelLod2LinkData lod2
                    ) =>
                    {
                        var children = linkedGroups[binder.BinderEntity];
                        
                        var isNearComponent = disableds.HasComponent(children[2].Value);
                        var isNearModel = model.DrawModelEntityCurrent == lod2.DrawModelEntityNear;

                        if (isNearModel & !isNearComponent)
                        {
                            changeToNear(entityInQueryIndex, entity, cmd, children, parts);
                        }


                        var isFarComponent = !isNearComponent;
                        var isFarModel = model.DrawModelEntityCurrent == lod2.DrawModelEntityFar;

                        if (isFarModel & !isFarComponent)
                        {
                            changeToFar(entityInQueryIndex, entity, cmd, children, parts);
                        }

                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void changeToNear
            (
                int uniqueIndex, Entity entity,
                EntityCommandBuffer.ParallelWriter cmd,
                DynamicBuffer<LinkedEntityGroup> children,
                ComponentDataFromEntity<StructurePart.PartData> partData
            )
        {

            //cmd.AddComponent<Structure.ShowNearTag>(uniqueIndex, entity);

            cmd.AddComponent<Disabled>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.RemoveComponent<Disabled>(uniqueIndex, children[i].Value);
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void changeToFar
            (
                int uniqueIndex, Entity entity,
                EntityCommandBuffer.ParallelWriter cmd,
                DynamicBuffer<LinkedEntityGroup> children,
                ComponentDataFromEntity<StructurePart.PartData> partData
            )
        {

            //cmd.RemoveComponent<Structure.ShowNearTag>(uniqueIndex, entity);

            cmd.RemoveComponent<Disabled>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent<Disabled>(uniqueIndex, children[i].Value);
            }
        }



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void changeToNear
        //    (
        //        Entity mainEntity, int uniqueIndex,
        //        EntityCommandBuffer.ParallelWriter cmd,
        //        DynamicBuffer<LinkedEntityGroup> children,
        //        ComponentDataFromEntity<StructurePart.PartData> partData
        //    )
        //{

        //    cmd.AddComponent<PhysicsExclude>(uniqueIndex, mainEntity);


        //    for (var i = 2; i < children.Length; i++)
        //    {
        //        var child = children[i].Value;
        //        if (!partData.HasComponent(child)) continue;

        //        cmd.RemoveComponent<Disabled>(uniqueIndex, child);
        //    }
        //}



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void changeToFar
        //    (
        //        Entity mainEntity, int uniqueIndex,
        //        EntityCommandBuffer.ParallelWriter cmd,
        //        DynamicBuffer<LinkedEntityGroup> children,
        //        ComponentDataFromEntity<StructurePart.PartData> partData
        //    )
        //{

        //    cmd.RemoveComponent<PhysicsExclude>(uniqueIndex, mainEntity);


        //    for (var i = 2; i < children.Length; i++)
        //    {
        //        var child = children[i].Value;
        //        if (!partData.HasComponent(child)) continue;

        //        cmd.AddComponent<Disabled>(uniqueIndex, child);
        //    }
        //}


    }

}
