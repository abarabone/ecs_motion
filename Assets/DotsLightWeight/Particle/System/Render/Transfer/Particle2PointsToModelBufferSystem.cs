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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class Particle2PointsToModelBufferSystem : DependencyAccessableSystemBase
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


            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.ParticleTag>()
                .WithNone<DrawInstance.MeshTag, BillBoad.UvCursorData, BillBoad.CursorToUvIndexData>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Particle.AdditionalData additional,
                        in Particle.TranslationPtoPData pos
                    )
                =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        const int vectorLength = (int)BoneType.PtoP;
                        var lengthOfInstance = offsetInfo.VectorOffsetPerInstance + vectorLength;
                        var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                        var i = instanceBufferOffset + offsetInfo.VectorOffsetPerInstance;

                        var size = additional.Size;
                        var color = math.asfloat(additional.Color.ToUint());

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[i + 0] = new float4(pos.Start, size);
                        pModel[i + 1] = new float4(pos.End, color);
                    }
                )
                .ScheduleParallel();
        }
    }

}
