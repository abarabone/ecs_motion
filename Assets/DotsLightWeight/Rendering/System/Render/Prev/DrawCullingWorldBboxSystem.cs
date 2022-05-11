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
    using DotsLite.ParticleSystem;


    //[DisableAutoCreation]
    ////[UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.DrawPrev.Culling))]
    public partial class DrawCullingWorldBboxSystem : SystemBase
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
                .WithNone<Psyllium.TranslationTailData>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                    (
                        ref DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData modellink,
                        in DrawInstance.WorldBbox wbbox
                    ) =>
                    {

                        if (modellink.DrawModelEntityCurrent == Entity.Null)
                        {
                            target.DrawInstanceId = -1;
                            return;
                        }


                        var isHit = viewFrustum.IsInside(wbbox.Bbox);

                        if (!isHit)
                        {
                            target.DrawInstanceId = -1;
                            return;
                        }


                        var drawModelData = drawModels[modellink.DrawModelEntityCurrent];

                        target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();
                    }
                )
                .ScheduleParallel();

            //this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
