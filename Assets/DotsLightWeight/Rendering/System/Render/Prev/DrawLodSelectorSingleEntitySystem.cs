using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsLite.Draw
{
    
    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Particle;
    using System.Runtime.CompilerServices;
    using DotsLite.Utilities;
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    ////[UpdateBefore(typeof(DrawCullingSystem))]
    ////[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.DrawPrev.Lod))]
    public class DrawLodSelectorSingleEntitySystem : DependencyAccessableSystemBase
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

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var campos = Camera.main.transform.position.As_float3();


            this.Entities
                .WithName("LodOnly")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNone<DrawInstance.NeedLodCurrentTag>()
                .ForEach((
                    ref DrawInstance.ModelLinkData modelLink,
                    in DrawInstance.ModelLod2LinkData lodLink,
                    in Translation pos) =>
                {

                    modelLink.DrawModelEntityCurrent = selectModel_(pos.Value, campos, lodLink, modelLink);

                })
                .ScheduleParallel();

            this.Entities
                .WithName("LodAddTag")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithAll<DrawInstance.NeedLodCurrentTag>()
                .ForEach((
                    Entity entity, int entityInQueryIndex,
                    ref DrawInstance.ModelLinkData modelLink,
                    in DrawInstance.ModelLod2LinkData lodLink,
                    in Translation pos) =>
                {
                    var eqi = entityInQueryIndex;

                    var currModel = modelLink.DrawModelEntityCurrent;
                    var nextModel = selectModel_(pos.Value, campos, lodLink, modelLink);

                    if (currModel == nextModel) return;


                    modelLink.DrawModelEntityCurrent = nextModel;

                    if (nextModel == lodLink.DrawModelEntityNear)
                    {
                        cmd.AddComponent<DrawInstance.LodCurrentIsNearTag>(eqi, entity);
                        cmd.RemoveComponent<DrawInstance.LodCurrentIsFarTag>(eqi, entity);
                        return;
                    }

                    if (nextModel == lodLink.DrawModelEntityFar)
                    {
                        cmd.AddComponent<DrawInstance.LodCurrentIsFarTag>(eqi, entity);
                        cmd.RemoveComponent<DrawInstance.LodCurrentIsNearTag>(eqi, entity);
                        return;
                    }

                    //modelLink.DrawModelEntityCurrent =
                    //    selectModel_WithAddTag_(cmd, eqi, entity, pos.Value, campos, lodLink, modelLink);

                })
                .ScheduleParallel();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Entity selectModel_(
            float3 targetpos_, float3 campos_,
            DrawInstance.ModelLod2LinkData lodLink_, DrawInstance.ModelLinkData modelLink_)
        {

            var distsqr = math.distancesq(targetpos_, campos_);


            var isCurrentNear = lodLink_.DrawModelEntityNear == modelLink_.DrawModelEntityCurrent;

            var limitDistanceSqrNear =
                math.select(lodLink_.LimitDistanceSqrNear, lodLink_.MarginDistanceSqrNear, isCurrentNear);

            if (limitDistanceSqrNear >= distsqr)
            {
                return lodLink_.DrawModelEntityNear;
            }


            var isCurrentFar = lodLink_.DrawModelEntityFar == modelLink_.DrawModelEntityCurrent;

            var limitDistanceSqrFar =
                math.select(lodLink_.LimitDistanceSqrFar, lodLink_.MarginDistanceSqrFar, isCurrentFar);

            if (limitDistanceSqrFar >= distsqr)
            {
                return lodLink_.DrawModelEntityFar;
            }


            return Entity.Null;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static Entity selectModel_WithAddTag_(
        //    EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex, Entity entity,
        //    float3 targetpos_, float3 campos_,
        //    DrawInstance.ModelLod2LinkData lodLink_, DrawInstance.ModelLinkData modelLink_)
        //{

        //    var distsqr = math.distancesq(targetpos_, campos_);


        //    var isCurrentNear = lodLink_.DrawModelEntityNear == modelLink_.DrawModelEntityCurrent;

        //    var limitDistanceSqrNear =
        //        math.select(lodLink_.LimitDistanceSqrNear, lodLink_.MarginDistanceSqrNear, isCurrentNear);

        //    if (limitDistanceSqrNear >= distsqr)
        //    {
        //        cmd.AddComponent<DrawInstance.LodCurrentIsNearTag>(uniqueIndex, entity);
        //        cmd.RemoveComponent<DrawInstance.LodCurrentIsFarTag>(uniqueIndex, entity);
        //        return lodLink_.DrawModelEntityNear;
        //    }


        //    var isCurrentFar = lodLink_.DrawModelEntityFar == modelLink_.DrawModelEntityCurrent;

        //    var limitDistanceSqrFar =
        //        math.select(lodLink_.LimitDistanceSqrFar, lodLink_.MarginDistanceSqrFar, isCurrentFar);

        //    if (limitDistanceSqrFar >= distsqr)
        //    {
        //        cmd.AddComponent<DrawInstance.LodCurrentIsFarTag>(uniqueIndex, entity);
        //        cmd.RemoveComponent<DrawInstance.LodCurrentIsNearTag>(uniqueIndex, entity);
        //        return lodLink_.DrawModelEntityFar;
        //    }


        //    return Entity.Null;
        //}
    }

}
