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

namespace Abarabone.Draw
{
    using Abarabone.Utilities;


    static public partial class Structure
    {

        public struct PartLinkData : IComponentData
        {
            public Entity NextEntity;
        }

        public struct SleepingTag : IComponentData
        { }

    }

    static public partial class StructurePart
    {

        public struct LocalPositionData : IComponentData
        {
            public float3 Translation;
            public quaternion Rotation;
        }

    }

}
