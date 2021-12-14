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
    using DotsLite.Particle;
    using DotsLite.Dependency;
    using DotsLite.Model;
    using DotsLite.HeightGrid;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public class HeightGridToModelBufferSystem : DependencyAccessableSystemBase
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
                .WithAll<Height.GridLv0Tag>()
                .WithNone<Height.WaveTransferTag>()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                //.WithReadOnly(heightss)
                .WithReadOnly(dims)
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Height.GridData grid,
                    in Height.AreaLinkData arealink,
                    in Translation pos) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    const int vectorLength = (int)BoneType.T;
                    var lengthOfInstance = offsetInfo.OptionalVectorLengthPerInstance + vectorLength;
                    var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;


                    //var heights = heightss[arealink.ParentAreaEntity];

                    //// length はセグメント数、頂点は + 1 個送る

                    var dim = dims[arealink.ParentAreaEntity];
                    //var srcw = dim.UnitLengthInGrid.x;
                    //var srcww = dim.NumGrids.x * dim.UnitLengthInGrid.x;
                    //var srch = dim.UnitLengthInGrid.y;
                    //var srcwwh = srcww * srch;

                    ////var lengthInGrid = this.gridMaster.UnitLengthInGrid * sizeof(float);
                    //var srcspan = srcww * sizeof(float);
                    //var dstspan = (srcw + 1) * sizeof(float);
                    //var count = srch + 1;
                    //var unitScale = dim.UnitScale;


                    //var pUnit = heights.p;
                    //var pSrc = pUnit + (grid.GridId.x * srcw + grid.GridId.y * srcwwh);

                    //var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                    var pDst = pModel + instanceBufferOffset;
                    var i = offsetInfo.OptionalVectorLengthPerInstance;

                    pDst[0] = new float4
                    {
                        x = math.asfloat(grid.GridId.y * dim.UnitLengthInGrid.y * dim.TotalLength.x + grid.GridId.x * dim.UnitLengthInGrid.x),
                        y = math.asfloat(grid.LodLevel),
                    };

                    var lodUnitScale = dim.UnitScale * (1 << grid.LodLevel);
                    pDst[i] = pos.Value.As_float4(lodUnitScale);

                })
                .ScheduleParallel();
        }
    }

}
