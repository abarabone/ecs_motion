using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Physics;
using Unity.Physics.Extensions;

namespace DotsLite.HeightGrid
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class HeightGridsAllocationSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;

        BarrierDependency.Sender barfreedep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.barfreedep = BarrierDependency.Sender.Create<HeightGridsFreeSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var cmdscope = this.cmddep.WithDependencyScope();
            using var barfreeScope = this.barfreedep.WithDependencyScope();

            this.Entities
                .WithoutBurst()
                .WithAll<GridMaster.InitializeTag>()
                .ForEach((ref GridMaster.HeightFieldData heights, in GridMaster.DimensionData dim) =>
                {
                    heights.Alloc(dim.NumGrids, dim.UnitLengthInGrid);
                })
                .Schedule();
        }
    }
}