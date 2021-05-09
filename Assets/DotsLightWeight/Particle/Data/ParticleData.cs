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

namespace DotsLite.Particle
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;



    static public partial class BillBoad
    {

        public struct UvCursor : IComponentData
        {
            public int CurrentIndex;
        }
        public struct UvCursorParam : IComponentData
        {
            public int IndexOffset;
            public int IndexMask;
        }

        public struct UvParam : IComponentData
        {
            //public uint2 Division;
            //public float2 UvWH;
            //public float2 UvOffset;
            public float2 Span;

            public uint UMask;
            public int VShift;
        }
        //public struct AdditionalData : IComponentData
        //{
        //    public Color Color;
        //}


        public static float2 CalcUv(this UvCursor cursor, UvParam uvinfo) =>
            CalcUv(cursor.CurrentIndex, uvinfo.Span, uvinfo.UMask, uvinfo.VShift);


        public static float2 CalcSpan(uint2 division) =>
            new float2(1.0f, 1.0f) / division;

        public static float2 CalcSpan(uint2 division, float2 uvWidthHeight) =>
            uvWidthHeight / division;

        public static (uint umask, int vshift) CalcUvParameter(uint2 division)
        {
            var umask = division.x - 1;
            var vshift = math.countbits(umask);
            return (umask, vshift);
        }

        public static float2 CalcUv(int id, float2 span, uint umask, int vshift)
        {
            var iu = id & umask;
            var iv = id >> vshift;
            var uv = span * new float2(iu, iv);
            return uv;
        }
        public static float2 CalcUv(int id, float2 span, uint umask, int vshift, float2 uvOffset)
        {
            var iu = id & umask;
            var iv = id >> vshift;
            var uv = span * new float2(iu, iv);
            return uvOffset + uv;
        }

        // 計算方法のリファレンスとして
        public static float2 CalcUv(int id, uint2 division, float2 uvWidthHeight)
        {
            var span = uvWidthHeight / division;

            var umask = division.x - 1;
            var vshift = math.countbits(umask);

            var iu = id & umask;
            var iv = id >> vshift;
            var uv = span * new float2(iu, iv);

            return uv;
        }
    }


    public static partial class BillBoadCustom
    {
        public struct UvCursor : IComponentData
        {
            public int2 CurrentId;
            public int2 Length;
        }

        public struct UvInfo : IComponentData
        {
            //public uint2 Division;
            //public float2 UvWH;
            //public float2 UvOffset;
            public float2 Span;
        }
    }

}
