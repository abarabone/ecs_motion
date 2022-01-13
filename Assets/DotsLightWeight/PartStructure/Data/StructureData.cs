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

using Collider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Utilities;





    static public partial class Main
    {

        public struct MainTag : IComponentData
        { }

        public struct SleepingTag : IComponentData
        { }


        // draw instance の lod タグと組み合わせて、切り替え時だけシステムが走るようにする
        public struct NearTag : IComponentData
        { }
        public struct FarTag : IComponentData
        { }
        //public struct NoShowTag : IComponentData
        //{ }

        // 暫定　後で直したい
        public struct BinderLinkData : IComponentData
        {
            public Entity BinderEntity;
        }



        public struct PartInfoData : IComponentData
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


        /// <summary>
        /// 破壊したパーツのビットフラグがオンになる
        /// 生存がオンでないのは直感的ではないかもだが、初期化がゼロで済むし、パーツがない部分も「壊れていない」で済ませられるため
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct PartDestructionData : IComponentData
        {

            [FieldOffset(0)]
            public fixed uint Destructions[512 / 32];

            [FieldOffset(0)]  public uint4 _values0;
            [FieldOffset(16)] public uint4 _values1;
            [FieldOffset(32)] public uint4 _values2;
            [FieldOffset(48)] public uint4 _values3;

            public void SetDestroyed( int id ) => this.Destructions[id >> 5] |= (uint)(1 << (id & 0b11111));
            public void SetAlive( int id ) => this.Destructions[id >> 5] &= ~(uint)(1 << (id & 0b11111));

            public bool IsDestroyed(int id) => (this.Destructions[id >> 5] & (uint)(1 << (id & 0b11111))) != 0;
        }

        //public struct PartLinkData : IComponentData
        //{
        //    public Entity NextEntity;
        //}

        public struct SleepTimerData : IComponentData
        {
            public float4 PrePositionAndTime;
            public float3 PrePosition => this.PrePositionAndTime.xyz;
            public float StillnessTime => this.PrePositionAndTime.w;

            public static float Margin => 2.0f;
        }

    }


    static public partial class Bone
    {
        public struct TransformOnlyOnceTag : IComponentData
        {
            //public int count;// 暫定
            //public bool WithoutDisable;// 暫定　だめ　これをやると、結局無駄なＴＦが続いてしまう
        }
    }


    static public partial class Part
    {

        public struct PartData : IComponentData
        {
            public int PartId;

            public float Life;
        }

        public struct LocalPositionData : IComponentData
        {
            public float3 Translation;
            public quaternion Rotation;
        }

        public struct DebrisPrefabData : IComponentData
        {
            public Entity DebrisPrefab;

        }

        //public struct DestructedTag : IComponentData
        //{ }

    }

    static public partial class PartDebris
    {

        public struct Data : IComponentData
        {
            public float LifeTime;
        }

    }

}
