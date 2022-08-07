using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace DotsLite.Draw
{

    static public partial class Palette
    {
        public struct ColorPaletteData : IComponentData
        {
            public int BaseIndex;
        }
    }
}
