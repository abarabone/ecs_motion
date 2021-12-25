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

namespace DotsLite.MarchingCubes.Data
{

    using DotsLite.Draw;
    using DotsLite.Utilities;


    public static partial class BitGridArea
    {
        public struct GridTypeData : IComponentData
        {
            public BitGridType GridType;
            //public int4 UnitOnEdge;
        }
        public unsafe struct GridLinkData : IComponentData
        {
            public Entity* pGrid3dArray;


            public void Alloc(int3 length)
            {
                this.Dispose();
                var size = length.x * length.y * length.z * sizeof(Entity);
                pGrid3dArray = (Entity*)UnsafeUtility.Malloc(size, 32, Allocator.Persistent);
                UnsafeUtility.MemClear(this.pGrid3dArray, size);
            }
            public void Dispose()
            {
                if (pGrid3dArray == null) return;
                Debug.Log("area grid link dispose");
                this.pGrid3dArray = (Entity*)UnsafeUtilityEx.Free(this.pGrid3dArray, Allocator.Persistent);
            }
        }
        public unsafe struct GridInstructionIdData : IComponentData
        {
            public int* pId3dArray;


            public void Alloc(int3 length)
            {
                this.Dispose();
                var size = length.x * length.y * length.z * sizeof(int);
                pId3dArray = (int*)UnsafeUtility.Malloc(size, 4, Allocator.Persistent);
                UnsafeUtility.MemSet(this.pId3dArray, 0xff, size);
            }
            public void Dispose()
            {
                if (pId3dArray == null) return;
                Debug.Log("area grid link dispose");
                this.pId3dArray = (int*)UnsafeUtilityEx.Free(this.pId3dArray, Allocator.Persistent);
            }
        }

        //public struct DrawModelLinkData : IComponentData
        //{
        //    public Entity DrawModelEntity;
        //}
        //public struct PoolLinkData : IComponentData
        //{
        //    public Entity PoolEntity;
        //}

        public struct UnitDimensionData : IComponentData
        {
            public float4 LeftTopFrontPosition;
            public float4 GridScaleR;
            public float4 UnitScaleR;
            public int4 GridSpan;
            public int4 GridLength;
            public int4 UnitOnEdge;
        }

        public struct BitGridPrefabData : IComponentData
        {
            public Entity Prefab;
            public int BitLineBufferLength;
            //public int BitLineBufferOffset;
            public Entity PoolEntity;
            public Entity DrawModelEntity;

            //public Entity DefaultGridEntity;// Žb’è
        }

        //public struct InfoData : IComponentData
        //{
        //    public int3 GridLength;
        //}
        //public struct InfoWorkData : IComponentData
        //{
        //    public int3 GridSpan;
        //}

        //[InternalBufferCapacity(0)]
        //public struct UnitCubeColliderAssetData : IBufferElementData
        //{
        //    public BlobAssetReference<Unity.Physics.Collider> Collider;
        //    public quaternion Rotation;
        //}


        public struct InitializeData : IComponentData
        {
            public int3 gridLength;
        }
    }

}

