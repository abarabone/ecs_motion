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

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Structure;

    using Random = Unity.Mathematics.Random;
    using TMPro;
    using System.Net;


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public partial class WaponSelectorSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var actions = this.GetComponentDataFromEntity<Control.ActionData>(isReadOnly: true);
            //var triggers = this.GetComponentDataFromEntity<FunctionUnit.TriggerData>();

            this.Entities
                .WithBurst()
                .WithReadOnly(actions)
                .ForEach(
                    (
                        ref WaponHolder.SelectorData selector,
                        in WaponHolder.StateLinkData link
                    ) =>
                    {
                        if (selector.Length == 0) return;

                        var action = actions[link.StateEntity];
                        if (!action.IsChangingWapon) return;


                        var newCurrentId = (selector.CurrentWaponIndex + 1) % selector.Length;

                        selector.CurrentWaponIndex = newCurrentId;
                    }
                )
                .ScheduleParallel();

        }

    }


}
