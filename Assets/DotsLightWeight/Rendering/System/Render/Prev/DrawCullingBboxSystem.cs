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
    

    ////[UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.DrawPrev.Culling))]
    public class DrawCullingWorldBboxSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModel.InstanceCounterData>();
            var bboxes = this.GetComponentDataFromEntity<DrawModel.BoundingBoxData>(isReadOnly: true);


            var cam = Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)
                .WithReadOnly(bboxes)
                .WithNone<Psyllium.TranslationTailData>()
                .WithNone<DrawInstance.PostureLinkData>()
                .WithNone<DrawInstance.ModelLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            ref DrawModel.InstanceCounterData counter,
                            in DrawInstance.WorldBbox wbbox
                        ) =>
                        {

                            var isHit = viewFrustum.IsInside(wbbox.Bbox);

                            if (!isHit)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            target.DrawInstanceId = counter.InstanceCounter.GetSerial();

                        }
                )
                .ScheduleParallel();

            //this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
