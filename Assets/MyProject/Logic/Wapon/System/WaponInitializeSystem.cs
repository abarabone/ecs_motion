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
using System.Runtime.CompilerServices;
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


            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        int entityInQueryIndex,
                        in Wapon.InitializeData init
                    ) =>
                    {
                        var main = init.CharacterMainEntity;
                        var muzzle = init.MuzzleBodyEntity;

                        var prefab0 = init.Prefabs.FunctionUnitPrefab0;
                        createFunctionUnitInstance_(cmd, entityInQueryIndex, prefab0, 0, main, muzzle);

                        var prefab1 = init.Prefabs.FunctionUnitPrefab1;
                        if(prefab1 != Entity.Null)
                            createFunctionUnitInstance_(cmd, entityInQueryIndex, prefab1, 1, main, muzzle);

                        var prefab2 = init.Prefabs.FunctionUnitPrefab2;
                        if (prefab2 != Entity.Null)
                            createFunctionUnitInstance_(cmd, entityInQueryIndex, prefab2, 2, main, muzzle);

                        var prefab3 = init.Prefabs.FunctionUnitPrefab3;
                        if (prefab3 != Entity.Null)
                            createFunctionUnitInstance_(cmd, entityInQueryIndex, prefab3, 3, main, muzzle);

                    }
                )
                .ScheduleParallel();


            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void createFunctionUnitInstance_
            (EntityCommandBuffer.ParallelWriter cmd_, int uniqueId_, Entity prefab_, int carryId_, Entity main_, Entity muzzle_)
        {
            var instance = cmd_.Instantiate(uniqueId_, prefab_);

            cmd_.SetComponent(uniqueId_, prefab_,
                new FunctionUnit.WaponCarryIdData
                {
                    WaponCarryId = carryId_,
                }
            );
            cmd_.SetComponent(uniqueId_, prefab_,
                new FunctionUnit.OwnerLinkData
                {
                    MainEntity = main_,
                    MuzzleBodyEntity = muzzle_,
                }
            );

        }

    }


}
