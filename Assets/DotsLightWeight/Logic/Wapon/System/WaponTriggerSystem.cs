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
    //[UpdateAfter(typeof(PlayerMoveDirectionSystem))]//
    public class WaponTriggerSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var actions = this.GetComponentDataFromEntity<Control.ActionData>(isReadOnly: true);
            var selectors = this.GetComponentDataFromEntity<WaponHolder.SelectorData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithReadOnly(actions)
                .WithReadOnly(selectors)
                .ForEach(
                    (
                        ref FunctionUnit.TriggerData trigger,
                        in FunctionUnitInWapon.TriggerSpecificData triggerType,
                        in FunctionUnit.StateLinkData slink,
                        in FunctionUnit.holderLinkData hlink
                    ) =>
                    {
                        var selector = selectors[hlink.WaponHolderEntity];

                        //if (selector.CurrentWaponIndex != triggerType.WaponCarryId) return;
                        // ここでショートカットすると、カレントではなくなった場合にオフにならなくなってしまう

                        var isTriggeredCurrent = selector.CurrentWaponIndex == triggerType.WaponCarryId;

                        var action = actions[slink.StateEntity];

                        trigger.IsTriggered = triggerType.Type switch// いずれは配列インデックスで取得できるようにしたい
                        {
                            FunctionUnitInWapon.TriggerType.main => isTriggeredCurrent & action.IsShooting,
                            FunctionUnitInWapon.TriggerType.sub => isTriggeredCurrent & action.IsTriggerdSub,
                            _ => false,
                        };
                    }
                )
                .ScheduleParallel();

        }



    }

}