﻿using System.Collections;
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
    
    using Abarabone.Misc;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Particle;
    

    [DisableAutoCreation]
    [UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Culling))]
    public class DrawCullingSystem : SystemBase
    {

        //BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証


        //protected override void OnStartRunning()
        //{
        //    this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        //    this.c = Camera.main;//
        //}

        //Camera c;//
        protected override void OnUpdate()
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModel.InstanceCounterData>();
            var bboxes = this.GetComponentDataFromEntity<DrawModel.BoundingBoxData>(isReadOnly: true);

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var scls = this.GetComponentDataFromEntity<NonUniformScale>(isReadOnly: true);


            var cam = Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            var dependsTRbone = this.Entities
            //this.Entities
                .WithName("Character")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)
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
                        if (modellink.DrawModelEntityCurrent == Entity.Null)
                        {
                            target.DrawInstanceId = -1;
                            return;
                        }


                        var bbox = bboxes[modellink.DrawModelEntityCurrent];
                        var rot = rots[posturelink.PostureEntity];
                        var pos = poss[posturelink.PostureEntity];

                        var isHit = viewFrustum.IsInside(bbox.localBbox, rot, pos);

                        if (!isHit)
                        {
                            target.DrawInstanceId = -1;
                            return;
                        }


                        var drawModelData = drawModels[modellink.DrawModelEntityCurrent];

                        target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                    }
                )
                .ScheduleParallel(this.Dependency );


            var dependsTR = this.Entities
            //this.Entities
                .WithName("MeshTRS")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)
                .WithReadOnly(bboxes)
                //.WithNone<NonUniformScale>()
                .WithNone<Particle.TranslationPtoPData>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData modellink,
                            in Rotation rot,
                            in Translation pos
                        ) =>
                        {
                            if (modellink.DrawModelEntityCurrent == Entity.Null)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            var bbox = bboxes[modellink.DrawModelEntityCurrent];

                            var isHit = viewFrustum.IsInside(bbox.localBbox, rot, pos);

                            if (!isHit)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            var drawModelData = drawModels[modellink.DrawModelEntityCurrent];

                            target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                        }
                )
                .ScheduleParallel(this.Dependency );


            var dependsParticle = this.Entities
            //this.Entities
                .WithName("PtopParticle")
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)
                //.WithNone<Rotation, Translation, NonUniformScale>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData modellink,
                            in Particle.TranslationPtoPData ptop,
                            in Particle.AdditionalData additional
                        ) =>
                        {
                            if (modellink.DrawModelEntityCurrent == Entity.Null)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            var bbox = new AABB
                            {
                                Center = (ptop.Start + ptop.End) * 0.5f,
                                Extents = math.abs(ptop.End - ptop.Start) * 0.5f + additional.Size,
                            };

                            var isHit = viewFrustum.IsInside(bbox);

                            if (!isHit)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }


                            var drawModelData = drawModels[modellink.DrawModelEntityCurrent];

                            target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                        }
                )
                .ScheduleParallel(this.Dependency);


            this.Dependency = JobHandle.CombineDependencies(dependsTRbone, dependsTR, dependsParticle);

            //this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
