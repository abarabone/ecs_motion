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



    public static class DotGrid
    {





        public struct UnitData : IComponentData, IDisposable
        {
            public int3 GridIndexInArea;
            public DotGrid32x32x32Unsafe Unit;


            public void Dispose() => this.Unit.Dispose();
        }

        //public struct NeargridData : IComponentData
        //{
        //    public NearGridIndex Index;
        //}

    }




}

