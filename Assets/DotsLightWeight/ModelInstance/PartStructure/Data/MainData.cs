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

        public struct CompoundColliderTag : IComponentData
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

        public struct PartLengthData : IComponentData
        {
            public int TotalPartLength;
            public int BoneLength;
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


}
