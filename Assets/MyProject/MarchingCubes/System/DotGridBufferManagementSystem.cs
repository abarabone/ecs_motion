using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    using MarchingCubes;
    using Abarabone.Draw;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DotGridBufferManagementSystem : SystemBase
    {


        //EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            //this.RequireSingletonForUpdate<Resource.Initialize>();
            this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
            this.Enabled = false;
        }

        protected unsafe override void OnUpdate()
        { }



        protected override unsafe void OnDestroy()
        {
            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();

            globaldata.Dispose();
        }

    }
}
