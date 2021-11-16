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

namespace DotsLite.MarchingCubes.another.Data
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
        public unsafe struct BitLinesData : IComponentData
        {
            public uint* p;

            public BitLinesData Alloc(uint bufferLength, GridFillMode fillMode)
            {
                this.Dispose();
                p = Tools.Alloc(bufferLength, fillMode);
                return this;
            }
            public void Dispose()
            {
                if (p == null) return;
                Debug.Log("bit grid dispose");
                p = Tools.Dispose(p);
            }
        }
        //public struct BufferLengthData : IComponentData
        //{
        //    public uint BitLineBufferLength;
        //}
        public struct AmountData : IComponentData
        {
            public uint BitCount;
        }

        public struct ParentAreaData : IComponentData
        {
            public Entity ParentAreaEntity;
        }
        public struct LocationInAreaData : IComponentData
        {
            public Tools.IndexInArea IndexInArea;
        }


        public struct UpdateDirtyRangeData : IComponentData
        {
            public uint begin;
            public uint end;
        }

    }

}

