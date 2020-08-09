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


            //this.Entities
            //    .ForEach(
            //        (
            //            in WaponSelector.
            //        ) =>
            //        {

            //}
            //    )
            //    ;

        }

    }


    static class WaponExtension
    {

        /// <summary>
        /// 
        /// </summary>
        static public void CreateWapon<TWaponEntity>
            (
                this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                Entity selectorEntity, Entity newWaponPrefab
            )
            where TWaponEntity : WaponSelector.IWaponEntityHolder
        {

            var newWaponEntity = cmd.Instantiate(uniqueIndex, newWaponPrefab);

            cmd.SetComponent(uniqueIndex, selectorEntity,
                new WaponSelector.WaponEntity0
                {
                    WaponEntity = newWaponEntity,
                }
            );

        }


        /// <summary>
        /// 
        /// </summary>
        static public void DestroyWaponInstance<TWaponEntity>
            (
                this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                Entity selectorEntity, TWaponEntity wapon
            )
            where TWaponEntity:WaponSelector.IWaponEntityHolder
        {

            cmd.DestroyEntity(uniqueIndex, wapon.GetWaponEntity);

            cmd.SetComponent(uniqueIndex, selectorEntity,
                new WaponSelector.WaponEntity0
                {
                    WaponEntity = Entity.Null,
                }
            );

        }


        /// <summary>
        /// 
        /// </summary>
        static public void DestroyAndCreateWapon<TWaponEntity>
            (
                this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                Entity selectorEntity, TWaponEntity wapon, Entity newWaponPrefab
            )
            where TWaponEntity : WaponSelector.IWaponEntityHolder
        {

            cmd.DestroyEntity(uniqueIndex, wapon.GetWaponEntity);

            cmd.CreateWapon<TWaponEntity>(uniqueIndex, selectorEntity, newWaponPrefab);
        }

    }


}
