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
    using TMPro;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    public class WaponSwitchingSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            //var triggers = this.GetComponentDataFromEntity<FunctionUnit.TriggerData>();
            //var wapons = this.GetComponentDataFromEntity<Wapon.UnitsHolderData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                //.WithReadOnly(wapons)
                .ForEach(
                    (
                        int entityInQueryIndex,
                        ref WaponSelector.ToggleModeData selector,
                        in WaponSelector.LinkData link
                    ) =>
                    {

                        var currentId = (selector.CurrentWaponCarryId + 1) % selector.WaponCarryLength;

                        selector.CurrentWaponCarryId = currentId;


                    }
                )
                .ScheduleParallel();

        }

    }


}
