using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes.another
{

    using DotsLite.Draw;
    using DotsLite.Utilities;


    public static class DotGridArea
    {
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public unsafe struct GridLinkData : IComponentData
        {
            public Entity* pGrid3DArray;
        }
        public struct ModelLinkData : IComponentData
        {
            public Entity ModelEntity;
        }
        public struct PoolLinkData : IComponentData
        {
            public Entity PoolEntity;
        }
    }

}

