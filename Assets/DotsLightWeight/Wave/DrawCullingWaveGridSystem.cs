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
    using DotsLite.HeightGrid;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Culling))]
    public class DrawCullingWaveGridSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModel.InstanceCounterData>();


            var cam = Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)

                .WithNone<DrawInstance.BoneModelTag>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach((
                    ref DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData modellink,
                    in Height.GridData grid,
                    in Translation pos) =>
                {
                    if (modellink.DrawModelEntityCurrent == Entity.Null)
                    {
                        target.DrawInstanceId = -1;
                        return;
                    }


                    //var bbox = new AABB
                    //{
                    //    //Center = pos.Value,
                    //    //Extents = additional.Radius,
                    //};

                    //var isHit = viewFrustum.IsInside(bbox);

                    //if (!isHit)
                    //{
                    //    target.DrawInstanceId = -1;
                    //    return;
                    //}


                    var drawModelData = drawModels[modellink.DrawModelEntityCurrent];

                    target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                })
                .ScheduleParallel();// this.Dependency);


            //this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
