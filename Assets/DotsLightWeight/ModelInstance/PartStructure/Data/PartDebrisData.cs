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
    static public partial class PartDebris
    {

        public struct Data : IComponentData
        {
            public float LifeTime;
        }

    }

}
