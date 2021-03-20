using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
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
    //[UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
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
            //    .WithName("WaponInitializeSelectorSystem")
            //    .WithBurst()
            //    .ForEach(
            //        (
            //            Entity entity, int entityInQueryIndex,
            //            ref WaponSelector.ToggleModeData selector,
            //            in WaponMessage.ReplaceWapon4MsgData msg
            //        ) =>
            //        {

            //            selector.WaponCarryLength = msg.NumPeplace;

            //        }
            //    )
            //    .ScheduleParallel();



            var ownerLinks = this.GetComponentDataFromEntity<FunctionUnit.OwnerLinkData>();
            var ids = this.GetComponentDataFromEntity<FunctionUnitWithWapon.WaponCarryIdData>();
            var selectorLinks = this.GetComponentDataFromEntity<FunctionUnitWithWapon.SelectorLinkData>();

            this.Entities
                .WithName("WaponInitializeFunctionSystem")
                .WithBurst()
                .WithNativeDisableParallelForRestriction(ownerLinks)
                .WithNativeDisableParallelForRestriction(ids)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        in DynamicBuffer<LinkedEntityGroup> entlinks,
                        in FunctionUnitWithWapon.InitializeData init
                    ) =>
                    {

                        for (var i = 0; i < entlinks.Length; i++)
                        {
                            initUnits_(init, entlinks[i], ownerLinks, ids, selectorLinks);
                        }
                        cmd.RemoveComponent<FunctionUnitWithWapon.InitializeData>(entityInQueryIndex, entity);

                        //setWaponLinkForSelector_
                        //    (init.WaponCarryId, init.SelectorEntity, entity, wapons0w, wapons1w, wapons2w, wapons3w);

                        return;


                        ////[MethodImpl(MethodImplOptions.AggressiveInlining)]
                        //static void setWaponLinkForSelector_
                        //    (
                        //        int waponCarryId_, Entity selectorEntity_, Entity waponEntity_,
                        //        ComponentDataFromEntity<WaponSelector.WaponLink0> wapons0w_,
                        //        ComponentDataFromEntity<WaponSelector.WaponLink1> wapons1w_,
                        //        ComponentDataFromEntity<WaponSelector.WaponLink2> wapons2w_,
                        //        ComponentDataFromEntity<WaponSelector.WaponLink3> wapons3w_
                        //    )
                        //{
                        //    switch (waponCarryId_)
                        //    {
                        //        case 0: wapons0w_[selectorEntity_] = new WaponSelector.WaponLink0 { WaponEntity = waponEntity_ }; break;
                        //        case 1: wapons1w_[selectorEntity_] = new WaponSelector.WaponLink1 { WaponEntity = waponEntity_ }; break;
                        //        case 2: wapons2w_[selectorEntity_] = new WaponSelector.WaponLink2 { WaponEntity = waponEntity_ }; break;
                        //        case 3: wapons3w_[selectorEntity_] = new WaponSelector.WaponLink3 { WaponEntity = waponEntity_ }; break;
                        //    }
                        //}
                    }
                )
                .ScheduleParallel();

        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void initUnits_
            (
                FunctionUnitWithWapon.InitializeData init_, LinkedEntityGroup unitLink_,
                ComponentDataFromEntity<FunctionUnit.OwnerLinkData> ownerLinks_,
                ComponentDataFromEntity<FunctionUnitWithWapon.WaponCarryIdData> ids_,
                ComponentDataFromEntity<FunctionUnitWithWapon.SelectorLinkData> selectorLinks_
            )
        {
            var unit = unitLink_.Value;
            if (!ownerLinks_.HasComponent(unit)) return;

            var ownerlink = new FunctionUnit.OwnerLinkData
            {
                OwnerMainEntity = init_.OwnerMainEntity,
                MuzzleBodyEntity = init_.MuzzleBodyEntity,
            };
            var id = new FunctionUnitWithWapon.WaponCarryIdData
            {
                WaponCarryId = init_.WaponCarryId,
            };
            var selectorLink = new FunctionUnitWithWapon.SelectorLinkData
            {
                SelectorEntity = init_.SelectorEntity,
            };

            ownerLinks_[unit] = ownerlink;
            ids_[unit] = id;
            selectorLinks_[unit] = selectorLink;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void setWaponLinkForSelector_
        //    (
        //        EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_,
        //        int waponCarryId_, Entity selectorEntity_, Entity waponEntity_
        //    )
        //{
        //    switch (waponCarryId_)
        //    {
        //        case 0: cmd_.SetComponent(uniqueIndex_, selectorEntity_, new WaponSelector.WaponLink0 { WaponEntity = waponEntity_ }); break;
        //        case 1: cmd_.SetComponent(uniqueIndex_, selectorEntity_, new WaponSelector.WaponLink1 { WaponEntity = waponEntity_ }); break;
        //        case 2: cmd_.SetComponent(uniqueIndex_, selectorEntity_, new WaponSelector.WaponLink2 { WaponEntity = waponEntity_ }); break;
        //        case 3: cmd_.SetComponent(uniqueIndex_, selectorEntity_, new WaponSelector.WaponLink3 { WaponEntity = waponEntity_ }); break;
        //    }
        //}


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void replaceWapon_
        //    (
        //        EntityCommandBuffer.ParallelWriter cmd_, int uniqueIndex_,
        //        Entity oldWaponEntity_, Entity newWaponPrefab_,
        //        int carryId_, Entity ownerMainEntity_, Entity muzzleBodyEntity_, Entity selectorEntity_
        //    )
        //{

        //    if (oldWaponEntity_ != Entity.Null)
        //    {
        //        cmd_.DestroyEntity(uniqueIndex_, oldWaponEntity_);
        //    }


        //    if (newWaponPrefab_ == Entity.Null) return;

        //    var newWapon = cmd_.Instantiate(uniqueIndex_, newWaponPrefab_);

        //    cmd_.AddComponent(uniqueIndex_, newWapon,
        //        new FunctionUnitWithWapon.InitializeData
        //        {
        //            WaponCarryId = carryId_,
        //            OwnerMainEntity = ownerMainEntity_,
        //            MuzzleBodyEntity = muzzleBodyEntity_,
        //            SelectorEntity = selectorEntity_,
        //        }
        //    );
        //}

    }


}
