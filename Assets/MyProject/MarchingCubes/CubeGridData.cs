using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.Experimental;

namespace MarchingCubes
{
    public unsafe partial struct CubeGridArrayUnsafe
    {

        //public unsafe struct CubeGrid32x32x32UnsafePtr
        //{
        //    [NativeDisableUnsafePtrRestriction]
        //    public CubeGrid32x32x32Unsafe* p;

        //    public uint this[int ix, int iy, int iz]
        //    {
        //        get => (*this.p)[ix, iy, iz];
        //        set => (*this.p)[ix, iy, iz] = value;
        //    }
        //}

        //public struct NearCubeGrids
        //{
        //    public int gridId;
        //    public HalfGridUnit L;
        //    public HalfGridUnit R;
        //    public struct HalfGridUnit
        //    {
        //        public CubeGrid32x32x32UnsafePtr x;
        //        public CubeGrid32x32x32UnsafePtr y;
        //        public CubeGrid32x32x32UnsafePtr z;
        //        public CubeGrid32x32x32UnsafePtr w;
        //    }
        //}



    }
}
