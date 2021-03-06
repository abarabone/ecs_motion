﻿using System.Collections;
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
    using Unity.Physics;
    using Abarabone.Structure;
    using UnityEngine.Rendering;

    using Random = Unity.Mathematics.Random;
    using System;


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(WaponSelectorSystem))]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]//
    public class WaponTriggerSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);
            var selectors = this.GetComponentDataFromEntity<WaponHolder.SelectorData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithReadOnly(handles)
                .WithReadOnly(selectors)
                .ForEach(
                    (
                        ref FunctionUnit.TriggerData trigger,
                        in FunctionUnitWithWapon.TriggerSpecificData triggerType,
                        in FunctionUnit.OwnerLinkData mainLink
                    ) =>
                    {
                        var selector = selectors[mainLink.OwnerMainEntity];

                        //if (selector.CurrentWaponIndex != triggerType.WaponCarryId) return;
                        // ここでショートカットすると、カレントではなくなった場合にオフにならなくなってしまう

                        var isTriggeredCurrent = selector.CurrentWaponIndex == triggerType.WaponCarryId;

                        var act = handles[mainLink.OwnerMainEntity].ControlAction;

                        trigger.IsTriggered = triggerType.Type switch// いずれは配列インデックスで取得できるようにしたい
                        {
                            FunctionUnitWithWapon.TriggerType.main => isTriggeredCurrent & act.IsShooting,
                            FunctionUnitWithWapon.TriggerType.sub => isTriggeredCurrent & act.IsTriggerdSub,
                            _ => false,
                        };

                    }
                )
                .ScheduleParallel();

            ////var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);
            //var units = this.GetComponentDataFromEntity<FunctionUnitWithWapon.TriggerTypeData>

            //this.Entities
            //    .ForEach(
            //        (
            //            in MoveHandlingData handle,
            //            in WaponHolder.SelectorData selector,
            //            in DynamicBuffer<WaponHolder.LinkData> holders
            //        ) =>
            //        {

            //        }
            //    )
            //    .ScheduleParallel();

        }


        //    protected override void OnUpdate()
        //    {
        //        var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);
        //        var selectors = this.GetComponentDataFromEntity<WaponSelector.ToggleModeData>(isReadOnly: true);


        //        this.Entities
        //            .WithBurst()
        //            .WithReadOnly(handles)
        //            .WithReadOnly(selectors)
        //            .ForEach(
        //                (
        //                    ref FunctionUnit.TriggerData trigger,
        //                    in FunctionUnitWithWapon.WaponCarryIdData carryid,
        //                    in FunctionUnitWithWapon.TriggerTypeData triggerType,
        //                    in FunctionUnitWithWapon.SelectorLinkData selectorLink,
        //                    in FunctionUnit.OwnerLinkData mainLink
        //                ) =>
        //                {
        //                    if (selectorLink.SelectorEntity == Entity.Null) return;


        //                    var selector = selectors[selectorLink.SelectorEntity];
        //                    var isCurrentUsing = (selector.CurrentWaponCarryId == carryid.WaponCarryId);


        //                    var handle = handles[mainLink.OwnerMainEntity];

        //                    switch (triggerType.Type)// いずれは配列インデックスで取得できるようにしたい
        //                    {
        //                        case FunctionUnitWithWapon.TriggerType.main:

        //                            trigger.IsTriggered = isCurrentUsing && handle.ControlAction.IsShooting;

        //                            break;
        //                        case FunctionUnitWithWapon.TriggerType.sub:

        //                            trigger.IsTriggered = isCurrentUsing && handle.ControlAction.IsTriggerdSub;

        //                            break;
        //                    }

        //                }
        //            )
        //            .ScheduleParallel();

        //    }


    }

}