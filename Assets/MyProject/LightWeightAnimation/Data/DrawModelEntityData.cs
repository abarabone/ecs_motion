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



    // メッシュごと -----------------------

    static public partial class DrawModel
    {
        //public struct BufferLinkerData : IComponentData
        //{
        //    public Entity BufferEntity;
        //}

        public struct BoneVectorSettingData : IComponentData
        {
            public int VectorLengthInBone;
            public int BoneLength;
        }

        public struct InstanceCounterData : IComponentData
        {
            public ThreadSafeCounter<Persistent> InstanceCounter;
        }
        public unsafe struct InstanceOffsetData : IComponentData
        {
            public float4* pVectorOffsetPerModelInBuffer;
            public int VectorOffsetPerModel;
            public int VectorOffsetPerInstance;
        }

        public class ComputeArgumentsBufferData : IComponentData
        {
            public ComputeBuffer InstanceArgumentsBuffer;
        }

        public class GeometryData : IComponentData
        {
            public Mesh Mesh;
            public Material Material;
        }
    }

}
