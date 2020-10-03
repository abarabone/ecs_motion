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

namespace Abarabone.MarchingCubes
{
    public unsafe partial struct DotGridArrayUnsafe
    {

        //public unsafe struct DotGrid32x32x32UnsafePtr
        //{
        //    [NativeDisableUnsafePtrRestriction]
        //    public DotGrid32x32x32Unsafe* p;

        //    public uint this[int ix, int iy, int iz]
        //    {
        //        get => (*this.p)[ix, iy, iz];
        //        set => (*this.p)[ix, iy, iz] = value;
        //    }
        //}

        //public struct NearDotGrids
        //{
        //    public int gridId;
        //    public HalfGridUnit L;
        //    public HalfGridUnit R;
        //    public struct HalfGridUnit
        //    {
        //        public DotGrid32x32x32UnsafePtr x;
        //        public DotGrid32x32x32UnsafePtr y;
        //        public DotGrid32x32x32UnsafePtr z;
        //        public DotGrid32x32x32UnsafePtr w;
        //    }
        //}



    }
}
