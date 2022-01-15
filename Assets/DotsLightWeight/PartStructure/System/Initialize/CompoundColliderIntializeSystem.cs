using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;


    public class CompoundColliderInitializeSystem : SystemBase
    {

        

        protected override void OnUpdate()
        {
            this.Entities
                .WithAll<Main.ColliderInitializeTag>()
                .ForEach((
                    ref Main.PartDestructionResourceData res) =>
                {

                })
                .ScheduleParallel();
        }

    }

}
