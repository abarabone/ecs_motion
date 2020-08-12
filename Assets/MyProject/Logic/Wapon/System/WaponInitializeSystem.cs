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

            var cmd1 = this.cmdSystem.CreateCommandBuffer();

            var selectors = this.GetComponentDataFromEntity<WaponSelector.ToggleModeData>();
            var links = this.GetComponentDataFromEntity<WaponSelector.LinkData>(isReadOnly: true);

            var selectorDependency = this.Entities
                .WithName("WaponSelectorInitializeSystem")
                .WithBurst()
                //.WithNativeDisableParallelForRestriction(toggles)
                .ForEach(
                    (
                        Entity entity,
                        in WaponMessage.CreateMsgData msg
                    ) =>
                    {
                        var selector = selectors[msg.WaponSelectorEntity];
                        var link = links[msg.WaponSelectorEntity];


                        var newWapon = cmd1.Instantiate(msg.WaponPrefab);

                        cmd1.AddComponent(newWapon,
                            new FunctionUnitWithWapon.InitializeData
                            {
                                WaponCarryId = msg.WaponCarryId,
                                OwnerMainEntity = link.OwnerMainEntity,
                                MuzzleBodyEntity = link.muzzleBodyEntity,
                                SelectorEntity = msg.WaponSelectorEntity,
                            }
                        );


                        if (selector.WaponCarryLength == 0)
                        {
                            cmd1.RemoveComponent<Disabled>(entity);
                        }
                        selector.WaponCarryLength++;
                        selectors[msg.WaponSelectorEntity] = selector;


                        cmd1.DestroyEntity(entity);
                        cmd1.RemoveComponent<Disabled>(msg.WaponSelectorEntity);
                    }
                )
                .Schedule(this.Dependency);


            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

            var ownerLinks = this.GetComponentDataFromEntity<FunctionUnit.OwnerLinkData>();
            var ids = this.GetComponentDataFromEntity<FunctionUnitWithWapon.WaponCarryIdData>();
            var selectorLinks = this.GetComponentDataFromEntity<FunctionUnitWithWapon.SelectorLinkData>();

            var unitDependency = this.Entities
                .WithName("WaponFunctionInitializeSystem")
                .WithBurst()
                .WithNativeDisableParallelForRestriction(ownerLinks)
                .WithNativeDisableParallelForRestriction(ids)
                .WithNativeDisableParallelForRestriction(selectorLinks)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in DynamicBuffer<LinkedEntityGroup> entlinks,
                        in FunctionUnitWithWapon.InitializeData init
                    ) =>
                    {

                        for(var i=0; i<entlinks.Length; i++)
                        {
                            var unit = entlinks[i].Value;
                            if (!ownerLinks.HasComponent(unit)) continue;

                            var ownerlink = new FunctionUnit.OwnerLinkData
                            {
                                OwnerMainEntity = init.OwnerMainEntity,
                                MuzzleBodyEntity = init.MuzzleBodyEntity,
                            };
                            ownerLinks[unit] = ownerlink;

                            var id = new FunctionUnitWithWapon.WaponCarryIdData
                            {
                                WaponCarryId = init.WaponCarryId,
                            };
                            ids[unit] = id;

                            var selectorLink = new FunctionUnitWithWapon.SelectorLinkData
                            {
                                SelectorEntity = init.SelectorEntity,
                            };
                            selectorLinks[unit] = selectorLink;
                        }

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
