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
    using DotsLite.Particle;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    ////[UpdateBefore(typeof(DrawCullingSystem))]
    ////[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Lod))]
    public class DrawLodSelectorWithPostureSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var campos = Camera.main.transform.position.As_float3();

            
            this.Entities
                //.WithName("Posture")
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
                .ScheduleParallel();

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
