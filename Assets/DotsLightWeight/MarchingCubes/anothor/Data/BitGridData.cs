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


    public static partial class BitGrid
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
            public unsafe struct BitLinesData : IComponentData
            {
                public uint* p;

                public void Alloc(GridFillMode fillMode)
                {
                    this.Dispose();
                    p = (uint*)DotGrid.Allocater<DotGrid32x32x32>.Alloc(fillMode);
                }
                public void Dispose()
                {
                    if (p == null) return;
                    p = (uint*)DotGrid.Allocater<DotGrid32x32x32>.Dispose(p);
                }
            }
        }
        public static class _16x16x16
        {
            public unsafe struct BitLinesData : IComponentData
            {
                public uint* p;

                public void Alloc(GridFillMode fillMode)
                {
                    this.Dispose();
                    p = (uint*)DotGrid.Allocater<DotGrid16x16x16>.Alloc(fillMode);
                }
                public void Dispose()
                {
                    if (p == null) return;
                    p = (uint*)DotGrid.Allocater<DotGrid16x16x16>.Dispose(p);
                }
            }
        }
        public struct AmountData : IComponentData
        {
            public uint BitCount;
            public uint BitLineBufferSize;
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

}

