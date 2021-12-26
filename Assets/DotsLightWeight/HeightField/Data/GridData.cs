using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.HeightGrid
{

    public static partial class HeightGrid
    {
        public struct GridLv0Tag : IComponentData
        { }

        public struct GridData : IComponentData
        {
            public int2 GridId;
            public int SerialIndex;
            public int LodLevel;
            public float UnitScaleOnLod;
        }

        public struct BlockBufferOnGpuData : IComponentData
        {
            public int SerialIndex;
        }

        public struct WaveTransferTag : IComponentData
        { }

        public struct BoundingBox : IComponentData
        {
            public AABB WorldBbox;
        }

        public struct AreaLinkData : IComponentData
        {
            public Entity ParentAreaEntity;
        }
    }

}