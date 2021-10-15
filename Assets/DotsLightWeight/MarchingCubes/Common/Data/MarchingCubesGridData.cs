using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace DotsLite.MarchingCubes
{
    using Utility;
    using Utilities;


    public static class DotGrid
    {


        public struct ParentAreaData : IComponentData
        {
            public Entity ParentArea;
        }



        public struct UnitData : IComponentData, IDisposable
        {
            public GridIndex GridIndexInArea;
            public DotGrid32x32x32Unsafe Unit;
            public float scale;

            //public void Dispose() => this.Unit.Dispose();
            public void Dispose()
            {
                Debug.Log("a");
                this.Unit.Dispose();
            }
        }
        public struct GridIndex
        {
            public int4 value;

            public int3 index => new int3(value.x, value.y, value.z);
            public int serial => value.w;

            public GridIndex Set(int3 index, int3 span)
            {
                var serial = math.dot(index, span);
                this.value = new int4(index.x, index.y, index.z, serial);
                return this;
            }
            public GridIndex CloneNear(int3 offset, int3 span) =>
                new DotGrid.GridIndex().Set(this.index + offset, span);
        }


        public struct UpdateDirtyRangeData : IComponentData
        {
            public uint begin;
            public uint end;
        }
    }




}

