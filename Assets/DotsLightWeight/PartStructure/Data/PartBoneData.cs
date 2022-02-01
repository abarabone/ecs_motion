using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using System.Runtime.InteropServices;
using System;
using Unity.Physics;
using System.Runtime.CompilerServices;

using Collider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    static public partial class PartBone
    {
        //[InternalBufferCapacity(0)]
        //public struct ColliderInitializeData : IBufferElementData
        //{
        //    public Entity ChildPartEntity;
        //    public RigidTransform RigidTransform;
        //    public int PartId;
        //    public Entity DebrisPrefab;
        //}

        public struct LinkToMainData : IComponentData
        {
            public Entity MainEntity;
        }

        public struct LengthData : IComponentData
        {
            public int PartLength;
            //public int LivePartLength;
        }

        //[InternalBufferCapacity(0)]
        //public struct PartDestructionResourceData : IBufferElementData
        //{
        //    public int PartId;
        //    public CompoundCollider.ColliderBlobInstance ColliderInstance;
        //    public Entity DebrisPrefab;
        //}
        [InternalBufferCapacity(0)]
        public struct PartInfoData : IBufferElementData
        {
            public int PartId;
            public Entity DebrisPrefab;
        }
        [InternalBufferCapacity(0)]
        public struct PartColliderResourceData : IBufferElementData
        {
            public CompoundCollider.ColliderBlobInstance ColliderInstance;
        }
    }

    static public partial class PartBone
    {
        public struct TransformOnlyOnceTag : IComponentData
        {
            //public int count;// 暫定
            //public bool WithoutDisable;// 暫定　だめ　これをやると、結局無駄なＴＦが続いてしまう
        }
    }


    static public partial class PartBone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public PhysicsCollider BuildCompoundCollider(
            this DynamicBuffer<PartColliderResourceData> partColliderBuffer,
            LengthData info,
            Main.PartDestructionData destructions)
        {
            var option = NativeArrayOptions.UninitializedMemory;
            var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
                partColliderBuffer.Length, Allocator.Temp, option);

            for (var i = 0; i < partColliderBuffer.Length; i++)
            {
                dst[i] = partColliderBuffer[i].ColliderInstance;
            }

            var collider = new PhysicsCollider
            {
                Value = CompoundCollider.Create(dst),
            };

            dst.Dispose();
            return collider;
        }
    }
}
