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
    public class ParticleUv1PointToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);

        }
        protected override void OnStartRunning()
        {
            this.Entities
                .WithoutBurst()
                .ForEach((
                    in BillboadModel.IndexToUvData touv,
                    in DrawModel.GeometryData geom) =>
                {
                    var span = touv.CellSpan;
                    var p = new float4(span, 0, 0);

                    geom.Material.SetVector("UvParam", p);
                })
                .Run();
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
                .WithNone<DrawInstance.MeshTag>()
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Particle.AdditionalData additional,
                    in BillBoad.UvCursorData cursor,
                    in BillBoad.CursorToUvIndexData touv,
                    in BillBoad.RotationData rotdir,
                    in Translation pos) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    const int vectorLength = (int)BoneType.P1uv;
                    var lengthOfInstance = vectorLength;//offsetInfo.VectorOffsetPerInstance + vectorLength;
                    var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                    var i = instanceBufferOffset;// + offsetInfo.VectorOffsetPerInstance;

                    var size = additional.Size;
                    var color = math.asfloat(additional.Color.ToUint());
                    var dir = rotdir.Direction;
                    var uvindex = math.asfloat(cursor.CalcUvIndex(touv));

                    var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                    pModel[i + 0] = new float4(pos.Value, 1.0f);
                    pModel[i + 1] = new float4(dir * size, uvindex, color);
                })
                .ScheduleParallel();
        }
    }

}