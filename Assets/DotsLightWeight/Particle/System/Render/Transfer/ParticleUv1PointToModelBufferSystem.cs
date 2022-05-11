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
    public partial class ParticleUv1PointToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawBufferToShaderDataSystem>(this);

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


            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.BillBoadTag>()
                .WithNone<DrawInstance.MeshTag>()
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Particle.OptionalData additional,
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

                    var size = additional.Radius;
                    var blendcolor = math.asfloat(additional.BlendColor.ToUint());
                    var addcolor = math.asfloat(additional.AdditiveColor.ToUint());
                    var dir = rotdir.Direction;
                    var uvindex = math.asfloat(cursor.CalcUvIndex(touv));

                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                    //pModel[i + 0] = new float4(pos.Value, 1.0f);
                    //pModel[i + 1] = new float4(dir * size, uvindex, color);
                    pModel[i + 0] = new float4(pos.Value, uvindex);
                    pModel[i + 1] = new float4(dir * size, addcolor, blendcolor);
                })
                .ScheduleParallel();
        }
    }

}
