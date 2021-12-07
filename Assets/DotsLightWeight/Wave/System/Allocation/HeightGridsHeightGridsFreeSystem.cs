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
    public class HeightGridsFreeSystem : SystemBase, BarrierDependency.IRecievable
    {

        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();



        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }


        protected override unsafe void OnUpdate()
        {

            this.Entities
                .WithoutBurst()
                .ForEach((ref GridMaster.HeightFieldData heights) =>
                {
                    heights.Dispose();
                })
                .Schedule();

        }

    }
}