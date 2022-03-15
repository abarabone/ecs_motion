using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using DotsLite.Geometry;

namespace DotsLite.Draw
{

    static public partial class StretchBox
    {
        public struct SizeData : IComponentData
        {
            public float4 Size;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct UvIndexData : IComponentData
        {
            [FieldOffset(0)]
            public byte top, left, forward, dummy1;
            [FieldOffset(4)]
            public byte bottom, right, back, dummy2;

            [FieldOffset(0)]
            public float2 as_float2;
        }
    }
}
