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

using System;

namespace DotsLite.Draw
{
    using DotsLite.CharacterMotion;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.Dependency;
    using DotsLite.Model;
    using DotsLite.Utilities;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public class SpringParticleToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.LineParticleTag>()
                .WithAll<Spring.StatesData>()//
                .WithNone<DrawInstance.MeshTag>()
                .WithNone<BillBoad.UvCursorData, BillBoad.CursorToUvIndexData>()
                .ForEach(
                    (
                        in DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Particle.AdditionalData additional
                    )
                =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        var vectorLength = tails.Length + 1;
                        var lengthOfInstance = offsetInfo.OptionalVectorLengthPerInstance + vectorLength;
                        var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                        var i = instanceBufferOffset + offsetInfo.OptionalVectorLengthPerInstance;

                        var size = additional.Radius;
                        var color = math.asfloat(additional.BlendColor.ToUint());

                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;

                        pModel[i++] = new float4(size);

                        for (var j = 0; j < tails.Length; j++)
                        {
                            //pModel[i + j] = tails[j].PositionAndColor;
                            pModel[i + j] = tails[j].Position.As_float4(math.asfloat(additional.BlendColor.ToUint()));// 暫定
                        }
                    }
                )
                .ScheduleParallel();
        }
    }

}
