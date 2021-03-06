﻿using System.Collections;
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
using Abarabone.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.Structure
{
    using Abarabone.Utilities;





    static public partial class Structure
    {

        public struct MainTag : IComponentData
        { }

        public struct SleepingTag : IComponentData
        { }

        public struct ShowNearTag : IComponentData
        { }



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

            public void SetDestroyed( int id ) => this.Destructions[id >> 5] |= (uint)(1 << id);
            public void SetAlive( int id ) => this.Destructions[id >> 5] &= ~(uint)(1 << id);

            public uint IsDestroyed(int id) => (uint)( this.Destructions[id >> 5] & ~(1 << id) );
        }

        public struct PartLinkData : IComponentData
        {
            public Entity NextEntity;
        }


    }


    static public partial class StructurePart
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

    static public partial class StructurePartDebris
    {

        public struct Data : IComponentData
        {
            public float LifeTime;
        }

    }

}
