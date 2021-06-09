using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Structure;
    using DotsLite.Dependency;


    static public class ColorExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int4 to_int4(this Color32 color) => new int4(color.r, color.g, color.b, color.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Color32 ToColor32(this int4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float4 to_float4(this Color32 color) => new float4(color.r, color.g, color.b, color.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Color32 ToColor32(this float4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Color32 ApplyAlpha(this Color32 color, float newAlpha)
        {
            color.a = (byte)(newAlpha * 255);
            return color;
        }
    }
}

