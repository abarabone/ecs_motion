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
using Unity.Physics;
using System;
using System.Runtime.InteropServices;

namespace Abarabone.Arms
{

    static public partial class FunctionHolder
    {

        [InternalBufferCapacity(4)]
        public struct LinkData : IBufferElementData
        {
            public Entity FunctionEntity;
        }

    }

    static public partial class WaponHolder
    {

        public struct SelectorData : IComponentData
        {
            public int Length;
            public int CurrentWaponIndex;
        }

        [InternalBufferCapacity(4)]
        public struct LinkData : IBufferElementData
        {
            public Entity FunctionEntity0;
            public Entity FunctionEntity1;
        }

    }


}
