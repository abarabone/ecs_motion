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
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;
using System.Runtime.InteropServices;

namespace DotsLite.Geometry
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;

    public static partial class ColorUtility
    {
        static public unsafe uint ToUint(this Color32 c) => new Color32Convertor { Color = c }.Uint;// (uint)(c.r << 24 | c.g << 16 | c.b << 8 | c.a << 0);

        static public Color32 ToColor32(this uint u) => new Color32Convertor { Uint = u }.Color;

        [StructLayout(LayoutKind.Explicit)]
        public struct Color32Convertor
        {
            [FieldOffset(0)] public Color32 Color;
            [FieldOffset(0)] public uint Uint;
        }
    }
}