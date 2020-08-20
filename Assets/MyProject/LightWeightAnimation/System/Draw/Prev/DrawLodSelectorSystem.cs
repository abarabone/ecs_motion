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

namespace Abarabone.Draw
{
    using Abarabone.Authoring;
    using Abarabone.Misc;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Particle;
    using System.Runtime.CompilerServices;

    //[DisableAutoCreation]
    [UpdateBefore(typeof(DrawCullingSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawLodSelectorSystem : SystemBase
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
                            ref DrawInstance.ModeLinkData modelLink,
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
                            ref DrawInstance.ModeLinkData modelLink,
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
                DrawInstance.ModelLod2LinkData lodLink_
            )
        {

            var distsqr = math.distancesq(targetpos_, campos_);


            if (lodLink_.LimitDistanceSqrNear >= distsqr)
            {
                return lodLink_.DrawModelEntityNear;
            }


            if (lodLink_.LimitDistanceSqrFar >= distsqr)
            {
                return lodLink_.DrawModelEntityFar;
            }


            // no draw
            {
                return Entity.Null;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Entity selectModel_
            (
                float3 targetpos_, float3 campos_,
                DrawInstance.ModelLod2LinkData lodLink_, DrawInstance.ModeLinkData modelLink_
            )
        {

            var distsqr = math.distancesq(targetpos_, campos_);


            var isCurrentNear = lodLink_.DrawModelEntityNear == modelLink_.DrawModelEntityCurrent;

            var limitDistanceSqrNear =
                math.select(lodLink_.LimitDistanceSqrRecoveryNear, lodLink_.LimitDistanceSqrNear, isCurrentNear);

            if (limitDistanceSqrNear >= distsqr)
            {
                return lodLink_.DrawModelEntityNear;
            }


            if (lodLink_.LimitDistanceSqrFar >= distsqr)
            {
                return lodLink_.DrawModelEntityFar;
            }


            // no draw
            {
                return Entity.Null;
            }

        }
    }

}
