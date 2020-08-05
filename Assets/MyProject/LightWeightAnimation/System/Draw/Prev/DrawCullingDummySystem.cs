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
    

    [UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetBoneSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetMotionStreamSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawCullingDummySystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証


        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
            this.c = Camera.main;//
        }

        Camera c;//
        protected override void OnUpdate()
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModel.InstanceCounterData>();
            var bboxes = this.GetComponentDataFromEntity<DrawModel.BoundingBoxData>(isReadOnly: true);

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var scls = this.GetComponentDataFromEntity<NonUniformScale>(isReadOnly: true);


            var cam = this.c;// Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            var dependsTRbone = this.Entities
                .WithBurst( FloatMode.Fast, FloatPrecision.Standard )
                .WithNativeDisableParallelForRestriction( drawModels )
                .WithReadOnly(bboxes)
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithNone<Rotation, Translation, NonUniformScale>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData modellink,
                            in DrawInstance.PostureLinkData posturelink
                        ) =>
                    {

                        var bbox = bboxes[modellink.DrawModelEntity];
                        var rot = rots[posturelink.PostureEntity];
                        var pos = poss[posturelink.PostureEntity];

                        var isHit = viewFrustum.IsInside(bbox.localBbox, rot, pos);

                        //if (!isHit)
                        //{
                        //    target.DrawInstanceId = -1;
                        //    return;
                        //}


                        var drawModelData = drawModels[ modellink.DrawModelEntity ];

                        target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                    }
                )
                .ScheduleParallel( this.Dependency );


            var dependsTR = this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithReadOnly(bboxes)
                .WithNone<NonUniformScale>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData modellink,
                            in Rotation rot,
                            in Translation pos
                        ) =>
                        {

                            var bbox = bboxes[modellink.DrawModelEntity];

                            var isHit = viewFrustum.IsInside(bbox.localBbox, rot, pos);

                            if (!isHit)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            var drawModelData = drawModels[modellink.DrawModelEntity];

                            target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                        }
                )
                .ScheduleParallel( this.Dependency );


            var dependsParticle = this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithReadOnly(bboxes)
                .WithNone<Rotation, Translation, NonUniformScale>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData modellink,
                            in Particle.Particle.TranslationPtoPData ptop
                        ) =>
                        {

                            var bbox = bboxes[modellink.DrawModelEntity];

                            //var isHit = viewFrustum.IsInside(bbox.localBbox, rot, pos);

                            //if (!isHit)
                            //{
                            //    target.DrawInstanceId = -1;
                            //    return;
                            //}


                            var drawModelData = drawModels[modellink.DrawModelEntity];

                            target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                        }
                )
                .ScheduleParallel(this.Dependency);


            this.Dependency = JobHandle.CombineDependencies(dependsTRbone, dependsTR);

            this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
