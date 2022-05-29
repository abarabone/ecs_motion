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
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Draw
{
    using DotsLite.CharacterMotion;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.ParticleSystem;
    using DotsLite.Dependency;
    using DotsLite.Model;
    using DotsLite.HeightGrid;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public partial class HeightGridToModelBufferSystem : DependencyAccessableSystemBase
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

            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();
            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            
            var heightss = this.GetComponentDataFromEntity<GridMaster.HeightFieldData>(isReadOnly: true);
            var dims = this.GetComponentDataFromEntity<GridMaster.DimensionData>(isReadOnly: true);

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<HeightGrid.GridLv0Tag>()
                .WithNone<HeightGrid.WaveTransferTag>()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                //.WithReadOnly(heightss)
                .WithReadOnly(dims)
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in HeightGrid.GridData grid,
                    in HeightGrid.BlockBufferOnGpuData gpu,
                    in HeightGrid.AreaLinkData arealink,
                    in Translation pos) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    int vectorLength = BoneType.T.VectorLength();
                    var lengthOfInstance = offsetInfo.OptionalVectorLengthPerInstance + vectorLength;
                    var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;


                    var dim = dims[arealink.ParentAreaEntity];

                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                    var pDst = pModel + instanceBufferOffset;
                    var i = offsetInfo.OptionalVectorLengthPerInstance;

                    // [0] x: grid serial index, y: lv as int
                    // [1] x,y,z: pos, w: scale * lv

                    pDst[0] = new float4
                    {
                        x = math.asfloat(gpu.SerialIndex),
                        //x = math.asfloat(grid.SerialIndex),
                        y = math.asfloat(grid.LodLevel),
                    };

                    var lodUnitScale = dim.UnitScale * (1 << grid.LodLevel);
                    pDst[i] = pos.Value.As_float4(lodUnitScale);

                })
                .ScheduleParallel();
        }
    }

}
