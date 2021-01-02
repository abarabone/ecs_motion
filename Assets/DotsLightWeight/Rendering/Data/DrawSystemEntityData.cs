using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.Draw
{

    using Abarabone.Geometry;
    using Abarabone.Misc;



    // シングルトン -----------------------

    static public partial class DrawSystem
    {
        public class ComputeTransformBufferData : IComponentData
        {
            public ComputeBuffer Transforms;
        }

        public struct NativeTransformBufferData : IComponentData
        {
            public SimpleNativeBuffer<float4, TempJob> Transforms;
            //public NativeArray<float4> aaa;
        }
    }

}
