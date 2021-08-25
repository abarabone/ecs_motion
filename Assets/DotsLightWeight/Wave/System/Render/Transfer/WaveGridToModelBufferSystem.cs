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
    public class WaveGridToModelBufferSystem : DependencyAccessableSystemBase
    {

        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);

            this.RequireSingletonForUpdate<GridMaster.Data>();
        }


        GridMaster.Data gridMaster;

        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<GridMaster.Data>();

            if (!this.HasSingleton<GridMaster.Data>()) return;
            this.gridMaster = this.GetSingleton<GridMaster.Data>();

            //this.Entities
            //    .WithoutBurst()
            //    .ForEach((
            //        in BillboadModel.IndexToUvData touv,
            //        in DrawModel.GeometryData geom) =>
            //    {
            //        var span = touv.CellSpan;
            //        var p = new float4(span, 0, 0);

            //        geom.Material.SetVector("UvParam", p);
            //    })
            //    .Run();
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();

            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();
            

            // length �̓Z�O�����g���A���_�� + 1 ����

            var gridinfo = this.gridMaster.Info;
            var srcw = gridinfo.UnitLengthInGrid.x;
            var srcww = gridinfo.NumGrids.x * gridinfo.UnitLengthInGrid.x;
            var srch = gridinfo.UnitLengthInGrid.y;
            var srcwwh = srcww * srch;

            //var lengthInGrid = this.gridMaster.UnitLengthInGrid * sizeof(float);
            var srcspan = srcww * sizeof(float);
            var dstspan = (srcw + 1) * sizeof(float);
            var count = srch + 1;
            var unitScale = gridinfo.UnitScale;
            var units = this.gridMaster.Nexts;

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<Height.GridLevel0Tag>()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Height.GridData grid,
                    in Translation pos) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    const int vectorLength = (int)BoneType.T;
                    var lengthOfInstance = offsetInfo.OptionalVectorLengthPerInstance + vectorLength;
                    var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;



                    var pUnit = (float*)units.GetUnsafeReadOnlyPtr();
                    var pSrc = pUnit + (grid.GridId.x * srcw + grid.GridId.y * srcwwh);

                    //var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                    var pDst = pModel + instanceBufferOffset;

                    var i = offsetInfo.OptionalVectorLengthPerInstance;


                    pDst[i - 1] = float4.zero;// ��������Ȃ��ƁA�V�F�[�_�[�́@dot(vh, mask) �ŕs��l�������Ă��܂�

                    var elementSize = dstspan;
                    UnsafeUtility.MemCpyStride(pDst, dstspan, pSrc, srcspan, elementSize, count);

                    var lodUnitScale = unitScale * (1 << grid.LodLevel);
                    //((float*)(pDst + i))[-1] = lodUnitScale;

                    pDst[i] = pos.Value.As_float4(lodUnitScale);

                })
                .ScheduleParallel();
        }
    }

}