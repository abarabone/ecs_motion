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
            public int LivePartLength;
        }

        [InternalBufferCapacity(0)]
        public struct PartDestructionResourceData : IBufferElementData
        {
            public int PartId;
            public CompoundCollider.ColliderBlobInstance ColliderInstance;
            public Entity DebrisPrefab;
        }
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
            //public int count;// ébíË
            //public bool WithoutDisable;// ébíËÅ@ÇæÇﬂÅ@Ç±ÇÍÇÇ‚ÇÈÇ∆ÅAåãã«ñ≥ë Ç»ÇsÇeÇ™ë±Ç¢ÇƒÇµÇ‹Ç§
        }
    }


    static public partial class PartBone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public PhysicsCollider BuildCompoundCollider(
            this DynamicBuffer<PartDestructionResourceData> ress,
            LengthData info,
            Main.PartDestructionData destructions)
        {
            var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
                info.LivePartLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < info.LivePartLength; i++)
            {
                dst[i] = ress[i].ColliderInstance;
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
