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
using Abarabone.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.Structure
{
    using Abarabone.Utilities;


    static public partial class Structure
    {

        /// <summary>
        /// 破壊したパーツのビットフラグがオンになる
        /// 生存がオンでないのは直感的ではないかもだが、初期化がゼロで済むし、パーツがない部分も「壊れていない」で済ませられるため
        /// </summary>
        public unsafe struct PartDestractionData : IComponentData
        {
            public fixed uint Destractions[512 / 32];

            public void SetDestroyed( int id ) => this.Destractions[id >> 5] |= (uint)(1 << id);
            public void SetAlive( int id ) => this.Destractions[id >> 5] &= ~(uint)(1 << id);

            public uint IsDestroyed(int id) => (uint)( this.Destractions[id >> 5] & ~(1 << id) );
        }

        public struct PartLinkData : IComponentData
        {
            public Entity NextEntity;
        }

        public struct SleepingTag : IComponentData
        { }

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

    }

}
