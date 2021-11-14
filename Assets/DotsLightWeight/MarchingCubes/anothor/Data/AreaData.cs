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


    public static partial class BitGridArea
    {
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public unsafe struct GridLinkData : IComponentData
        {
            public Entity* pGrid3DArray;


            public void Alloc(int3 length)
            {
                this.Dispose();
                var size = length.x * length.y * length.z * sizeof(Entity);
                pGrid3DArray = (Entity*)UnsafeUtility.Malloc(size, 8, Allocator.Persistent);
            }
            public void Dispose()
            {
                if (pGrid3DArray == null) return;
                Debug.Log("area grid link dispose");
                this.pGrid3DArray = (Entity*)UnsafeUtilityEx.Free(this.pGrid3DArray, Allocator.Persistent);
            }
        }
        public struct a : IComponentData
        {

        }

        public struct DrawModelLinkData : IComponentData
        {
            public Entity DrawModelEntity;
        }
        public struct PoolLinkData : IComponentData
        {
            public Entity PoolEntity;
        }

        public struct UnitDimensionData : IComponentData
        {
            public float4 LeftTopFrontPosition;
            public float4 GridScaleR;
            public float4 UnitScaleR;
        }

        public struct DotGridPrefabData : IComponentData
        {
            public Entity Prefab;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
        }
        public struct InfoWorkData : IComponentData
        {
            public int3 GridSpan;
        }


        public struct InitializeData : IComponentData
        {
            public int3 gridLength;
        }
    }

}

