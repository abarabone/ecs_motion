using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.XR;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Structure;

    using Random = Unity.Mathematics.Random;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    public class WaponSwitchingSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();


            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        int entityInQueryIndex,
                        in Wapon.InitializeData init
                    ) =>
                    {

                        cmd.Instantiate(init.Prefabs.FunctionUnitPrefab0);

                    }
                )
                .ScheduleParallel();


            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

    }


}
