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

namespace DotsLite.Draw
{

    using DotsLite.Geometry;
    using DotsLite.Misc;



    // メッシュごと -----------------------

    static public partial class DrawModel
    {

        // struct ----------------

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

        public struct BoundingBoxData : IComponentData
        {
            public AABB localBbox;
        }


        public struct SortSettingData : IComponentData
        {
            public bool IsSortAsc;
        }



        // class object -------------
        // ジョブからは使えないので、基本的には DrawMesh() 関連で使用

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
