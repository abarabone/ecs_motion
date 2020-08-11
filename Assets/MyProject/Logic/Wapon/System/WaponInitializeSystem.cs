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
    public class WaponInitializeSystem : SystemBase
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


            var selectorDependency = this.Entities
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref WaponSelector.ToggleModeData selector,
                        in WaponSelector.LinkData link,
                        in WaponSelector.CreateNewWaponData init
                    ) =>
                    {

                        var newWapon = cmd.Instantiate(entityInQueryIndex, init.WaponPrefab);

                        cmd.AddComponent(entityInQueryIndex, newWapon,  
                            new FunctionUnitWithWapon.InitializeData
                            {
                                OwnerMainEntity = link.OwnerMainEntity,
                                MuzzleBodyEntity = link.muzzleBodyEntity,
                            }
                        );


                        if (selector.WaponCarryLength == 0)
                        {
                            cmd.RemoveComponent<Disabled>(entityInQueryIndex, entity);
                        }
                        selector.WaponCarryLength++;


                        cmd.RemoveComponent<WaponSelector.CreateNewWaponData>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel(this.Dependency);


            var unitDependency = this.Entities
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref FunctionUnit.OwnerLinkData ownerlink,
                        in FunctionUnitWithWapon.InitializeData init
                    ) =>
                    {

                        ownerlink.OwnerMainEntity = init.OwnerMainEntity;
                        ownerlink.MuzzleBodyEntity = init.MuzzleBodyEntity;


                        cmd.RemoveComponent<FunctionUnitWithWapon.InitializeData>(entityInQueryIndex, entity);
                    }
                )
                .ScheduleParallel(this.Dependency);


            this.Dependency = JobHandle.CombineDependencies(selectorDependency, unitDependency);

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

    }


}
