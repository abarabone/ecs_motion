﻿using System.Collections;
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
    using MarchingCubes;
    using Abarabone.Draw;

    //[DisableAutoCreation]
    [UpdateAfter(typeof(BeginDrawCsBarier))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    public class DrawMarchingCubeCsSystem : SystemBase
    {
        protected override void OnUpdate()
        {



        }
    }
}