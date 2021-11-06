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

    public static partial class DotGrid
    {


        public struct ParentAreaData : IComponentData
        {
            public Entity ParentArea;
        }
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }



        //public static Type TypeOf_UnitData<TGrid>() where TGrid : struct, IDotGrid<TGrid> =>
        //    new TGrid() switch
        //    {
        //        DotGrid32x32x32 _ => typeof(Unit32Data),
        //        DotGrid16x16x16 _ => typeof(Unit16Data),
        //        _ => default,
        //    };

        public interface IUnitData<TGrid>
            where TGrid : struct, IDotGrid<TGrid>
        {
            public TGrid Unit { get; set; }
        }

        public struct Unit32Data : IComponentData, IUnitData<DotGrid32x32x32>//, IDisposable
        {
            public DotGrid32x32x32 Unit { get; set; }

            //public void Dispose() => this.Unit.Dispose();
            public void Dispose()
            {
                Debug.Log("DotGrid.Unit32Data dispose");
                this.Unit.Dispose();
            }
        }
        public struct Unit16Data : IComponentData, IUnitData<DotGrid16x16x16>//, IDisposable
        {
            public DotGrid16x16x16 Unit { get; set; }

            //public void Dispose() => this.Unit.Dispose();
            public void Dispose()
            {
                Debug.Log("DotGrid.Unit16Data dispose");
                this.Unit.Dispose();
            }
        }

        public struct ScaleData : IComponentData
        {
            public float scale;
        }
        public struct IndexData : IComponentData
        {
            public GridIndex GridIndexInArea;
            public float scale;//
        }
        public struct GridIndex
        {
            public int4 value;

            public int3 index => value.xyz;
            public int serial => value.w;

            public GridIndex Set(int3 index, int3 span)
            {
                var serial = math.dot(index, span);
                this.value = new int4(index, serial);
                return this;
            }
            public GridIndex CloneNear(int3 offset, int3 span) =>
                new GridIndex().Set(this.index + offset, span);
        }


        public struct UpdateDirtyRangeData : IComponentData
        {
            public uint begin;
            public uint end;
        }
    }




}

