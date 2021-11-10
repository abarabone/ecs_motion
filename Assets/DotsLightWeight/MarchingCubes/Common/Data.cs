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



    //common
    //work
    //init


    //type
    //dimension
    //link to grid
    //info
    //infowork
    //resource
    //prefab
    //rot
    //pos
    //init

    public static partial class DotGrid
    {

        public struct DefaultTag : IComponentData
        { }
        public struct UnusingTag : IComponentData
        { }

        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public static class _32x32x32
        {
            public unsafe struct ContentsData : IComponentData
            {
                public uint* p;
            }
        }
        public static class _16x16x16
        {
            public unsafe struct ContentsData : IComponentData
            {
                public uint* p;
            }
        }
        public struct AmountData : IComponentData
        {
            public uint DotCount;
        }

        public struct ParentAreaData : IComponentData
        {
            public Entity ParentAreaEntity;
        }
        public struct LocationInAreaData : IComponentData
        {
            public Index IndexInArea;
            
            public struct Index
            {
                public int4 value;

                public int3 index => value.xyz;
                public int serial => value.w;

                public Index Set(int3 index, int3 span)
                {
                    var serial = math.dot(index, span);
                    this.value = new int4(index, serial);
                    return this;
                }
                public Index CloneNear(int3 offset, int3 span) =>
                    new Index().Set(this.index + offset, span);
            }
        }


        public struct UpdateDirtyRangeData : IComponentData
        {
            public uint begin;
            public uint end;
        }
    }

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

    public static class DrawModel
    {
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public class MakeCubesShaderResourceData : IComponentData
        {
            public ComputeShader MakeCubesShader;
            public ComputeBuffer DotContents;
            public Texture CubeIds;
            public ComputeBuffer CubeInstances;
        }
    }

    public static class Common
    {
        public class DrawShaderResourceData : IComponentData
        {
            public ComputeBuffer GeometryElementData;
        }
    }
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

