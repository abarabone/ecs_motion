using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    using DotsLite.ParticleSystem;
    using DotsLite.Utilities;

    [DisableAutoCreation]
    ////[UpdateBefore(typeof(DrawCullingSystem))]
    ////[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.DrawPrev.Lod))]
    public partial class DrawLodSelectorSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var campos = Camera.main.transform.position.As_float3();

            
            var postureDependency = this.Entities
                .WithName("Posture")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithReadOnly(poss)
                .WithNone<Translation>()
                .ForEach(
                        (
                            ref DrawInstance.ModelLinkData modelLink,
                            in DrawInstance.ModelLod2LinkData lodLink,
                            in DrawInstance.PostureLinkData posturelink
                        ) =>
                    {

                        var pos = poss[posturelink.PostureEntity];

                        modelLink.DrawModelEntityCurrent = selectModel_(pos.Value, campos, lodLink, modelLink);

                    }
                )
                .ScheduleParallel(this.Dependency);

            
            var translationDependency = this.Entities
                .WithName("SingleEntity")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .ForEach(
                        (
                            ref DrawInstance.ModelLinkData modelLink,
                            in DrawInstance.ModelLod2LinkData lodLink,
                            in Translation pos
                        ) =>
                    {

                        modelLink.DrawModelEntityCurrent = selectModel_(pos.Value, campos, lodLink, modelLink);

                    }
                )
                .ScheduleParallel(this.Dependency);


            this.Dependency = JobHandle.CombineDependencies(postureDependency, translationDependency);

        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Entity selectModel_
            (
                float3 targetpos_, float3 campos_,
                DrawInstance.ModelLod2LinkData lodLink_, DrawInstance.ModelLinkData modelLink_
            )
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
    }

}
