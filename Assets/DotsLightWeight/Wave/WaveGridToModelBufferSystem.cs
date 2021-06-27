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
    using DotsLite.WaveGrid;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class WaveGridToModelBufferSystem : DependencyAccessableSystemBase
    {

        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);

        }


        WaveGridMasterData gridMaster;

        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<WaveGridMasterData>();

            if (!this.HasSingleton<WaveGridMasterData>()) return;
            this.gridMaster = this.GetSingleton<WaveGridMasterData>();

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

            var lengthInGrid = this.gridMaster.UnitLengthInGrid;
            var wspan = this.gridMaster.NumGrids.x;
            var unitScale = this.gridMaster.UnitScale;
            var units = this.gridMaster.Units;

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)

                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in WaveGridData grid,
                    in Translation pos) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    const int vectorLength = (int)BoneType.T;
                    var lengthOfInstance = offsetInfo.VectorOffsetPerInstance + vectorLength;
                    var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                    var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                    var i = instanceBufferOffset + offsetInfo.VectorOffsetPerInstance;
                    var p = pModel + i;

                    var lodUnitScale = unitScale * (1 << grid.LodLevel);

                    *p++ = new float4(lodUnitScale);



                })
                .ScheduleParallel();
        }
    }

}
