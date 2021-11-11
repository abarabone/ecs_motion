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


    public static class GridStocker
    {
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public struct DefaultGridData : IComponentData
        {
            public Entity BlankGridEntity;
            public Entity SolidGridEntity;
        }
        public struct GridPoolData : IComponentData
        {
            public UnsafeList<Entity> UnusingEntities;
        }
    }

}

