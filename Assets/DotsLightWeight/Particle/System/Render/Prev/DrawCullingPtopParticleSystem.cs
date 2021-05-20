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

namespace DotsLite.Draw
{
    
    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Particle;
    

    ////[UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Culling))]
    public class DrawCullingPtopParticleSystem : SystemBase
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
            //var bboxes = this.GetComponentDataFromEntity<DrawModel.BoundingBoxData>(isReadOnly: true);

            //var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            //var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            //var scls = this.GetComponentDataFromEntity<NonUniformScale>(isReadOnly: true);


            var cam = Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithNativeDisableParallelForRestriction(drawModels)
                .WithNativeDisableContainerSafetyRestriction(drawModels)
                //.WithNone<Rotation, Translation, NonUniformScale>()
                .WithNone<DrawInstance.BoneModelTag>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModelLinkData modellink,
                            //in Particle.TranslationPtoPData ptop,
                            in Particle.TranslationTailData tail,
                            in Translation pos,
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
                                Center = (pos.Value + tail.Position) * 0.5f,
                                Extents = math.abs(pos.Value - tail.Position) * 0.5f + additional.Size,
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
                .ScheduleParallel();// this.Dependency);


            //this.presentationBarier.AddJobHandleForProducer( this.Dependency );
        }

    }

}
