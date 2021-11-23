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

namespace DotsLite.MarchingCubes
{

    using DotsLite.Draw;
    using DotsLite.Utilities;
    using DotsLite.MarchingCubes.Data;


    public static class GridStocker
    {
        public struct GridTypeData : IComponentData
        {
            public BitGridType GridType;
            //public int4 UnitOnEdge;
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

