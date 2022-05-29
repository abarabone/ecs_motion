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
    using DotsLite.ParticleSystem;
    using DotsLite.Dependency;
    using DotsLite.Model;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public partial class Particle2PointsToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawBufferToShaderDataSystem>(this);
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
                .WithAll<DrawInstance.PsylliumTag>()
                .WithNone<DrawInstance.MeshTag>()
                .WithNone<BillBoad.UvCursorData, BillBoad.CursorToUvIndexData>()
                .ForEach(
                    (
                        ref Psyllium.TranslationTailData tail,
                        in Translation pos,
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Particle.OptionalData additional
                    )
                =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        int vectorLength = BoneType.PtoP.VectorLength();
                        var lengthOfInstance = offsetInfo.OptionalVectorLengthPerInstance + vectorLength;
                        var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                        var i = instanceBufferOffset + offsetInfo.OptionalVectorLengthPerInstance;

                        var size = additional.Radius;
                        var color = math.asfloat(additional.BlendColor.ToUint());

                        tail.Size = size;

                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                        pModel[i + 0] = tail.PositionAndSize;
                        pModel[i + 1] = new float4(pos.Value, color);
                    }
                )
                .ScheduleParallel();
        }
    }

}
