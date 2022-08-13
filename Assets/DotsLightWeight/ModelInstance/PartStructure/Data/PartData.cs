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
    static public partial class Part
    {

        public struct PartData : IComponentData
        {
            public int PartId;

            //public float Life;
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
}
