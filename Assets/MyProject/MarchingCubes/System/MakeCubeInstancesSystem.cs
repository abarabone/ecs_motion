using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    using Abarabone.Draw;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    [UpdateBefore(typeof(BeginDrawCsBarier))]
    public class MakeCubeInstancesSystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;

        NativeList<>

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override void OnUpdate()
        {

            this.Entities
                .WithBurst()
                .ForEach(
                    (in CubeGridArray.BufferData buf) =>
                    {



                    }
                )
                .ScheduleParallel();

            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }
}
