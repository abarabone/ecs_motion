using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public unsafe partial struct DotGrid16x16x16 : IDotGrid<DotGrid16x16x16>, IDisposable
    {
        //public const int dotNum = 16 * 16 * 16;
        //public const int xlineInGrid = 1 * 16 * 16;
        //public const int shiftNum = 4;
        //public const int maxbitNum = 16;

        public int UnitOnEdge => 16;
        public int XLineBufferLength => 16 * 16 / 2;


        public uint* pXline { get; private set; }
        public int CubeCount;// DotCount に変更　あとで



        //public bool IsFullOrEmpty => (this.CubeCount & (dotNum - 1) ) == 0;
        //public bool IsFull => this.CubeCount == dotNum;
        //public bool IsEmpty => this.CubeCount == 0;

        //public GridFillMode FillModeBlankOrSolid => (GridFillMode)(this.CubeCount >> (16 - 1));
        //public GridFillMode FillMode
        //{
        //    get
        //    {
        //        var notfilled = math.select(-1, 0, (this.CubeCount & (dotNum - 1)) != 0);
        //        var solid = this.CubeCount >> (16 - 1);
        //        return (GridFillMode)( notfilled | solid );
        //    }
        //}


        public DotGrid16x16x16(GridFillMode fillmode) : this() => this.Alloc(fillmode);

        public DotGrid16x16x16 Alloc(GridFillMode fillmode)
        {
            var p = DotGrid.Allocater<DotGrid16x16x16>.Alloc(fillmode);//, size);
            this.pXline = (uint*)p;
            this.CubeCount = 16 * 16 * 16;
            return this;
        }

        public void Dispose()
        {
            if( this.pXline == null ) return;// struct なので、複製された場合はこのチェックも意味がない

            Debug.Log("dgrid 16 dispose");
            DotGrid.Allocater<DotGrid16x16x16>.Dispose(this.pXline);
            this.pXline = null;
        }

    }


}
