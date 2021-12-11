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
    public class HeightBufferAllocationAndFreeSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var cmdscope = this.cmddep.WithDependencyScope();

            //var cmd = cmdscope.CommandBuffer;
            var em = this.EntityManager;

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<GridMaster.InitializeTag>()
                .ForEach((
                    Entity ent,
                    ref GridMaster.HeightFieldData heights,
                    in GridMaster.DimensionData dim) =>
                {
                    heights.Alloc(dim.NumGrids, dim.UnitLengthInGrid);

                    //cmd.RemoveComponent<GridMaster.InitializeTag>(ent);
                    em.RemoveComponent<GridMaster.InitializeTag>(ent);
                })
                //.Schedule();
                .Run();
        }

        protected override void OnDestroy()
        {

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((ref GridMaster.HeightFieldData heights) =>
                {
                    heights.Dispose();
                })
                //.Schedule();
                .Run();

        }
    }
}