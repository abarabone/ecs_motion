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
using System.Runtime.CompilerServices;

namespace DotsLite.Draw
{
    
    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Particle;
    using DotsLite.Utilities;
    

    ////[UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.DrawPrev.Culling))]
    public class DrawCullingLineParticleSystem : SystemBase
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
                .WithAll<DrawInstance.LineParticleTag>()
                .WithNone<DrawInstance.BoneModelTag>()
                .WithNone<DrawInstance.PostureLinkData>()
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModelLinkData modellink,
                            in DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                            //in Translation pos,
                            in Particle.AdditionalData additional
                        ) =>
                        {
                            if (modellink.DrawModelEntityCurrent == Entity.Null)
                            {
                                target.DrawInstanceId = -1;
                                return;
                            }

                            var minmax = calcMinMax(tails);
                            var min = minmax.min;
                            var max = minmax.max;
                            var bbox = new AABB
                            {
                                Center = (min + max).xyz * 0.5f,
                                Extents = math.abs(max - min).xyz * 0.5f + additional.Radius,
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

        struct minmax
        {
            public float4 min;
            public float4 max;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static minmax calcMinMax(DynamicBuffer<LineParticle.TranslationTailLineData> tails)
        {
            var min = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new float3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var tail in tails)
            {
                min = math.min(min, tail.Position);
                max = math.max(max, tail.Position);
            }

            return new minmax
            {
                min = min.As_float4(),
                max = max.As_float4(),
            };
        }

    }

}
