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



    public static partial class ParticleModel
    {
        public struct UvParam : IComponentData
        {
            public uint2 Division;
            public float2 UvOffset;
        }

        public struct UvCalcParam : IComponentData
        {
            public float2 UnitSpan;
        }
    }



    public static partial class Particle
    {

        // 破棄時刻指定
        public struct LifeTimeData : IComponentData
        {
            public float EndTime;
        }
        public struct LifeTimeSpecData : IComponentData
        {
            public float DurationSec;
        }

        //// 減ってく
        //public struct DurationData : IComponentData
        //{
        //    public float DurationSec;
        //}
    }


    static public partial class BillBoad
    {

        public struct UvCursorData : IComponentData
        {
            public int CurrentIndex;
        }
        //public struct UvCursorParam : IComponentData
        //{
        //    public int IndexPrevMask;
        //    public int IndexAfterOffset;
        //}

        public struct IndexToUvData : IComponentData
        {
            public uint UMask;
            public int VShift;
            public uint Offset;
            public int UnitUsage;
        }
        //public struct AdditionalData : IComponentData
        //{
        //    public Color Color;
        //}

        // 二次元回転を、up の方向ベクトル x, y で表す
        // 回転行列は下記（ x=cos, y=sin とみなせるため）
        // | x, -y |
        // | y,  x |
        public struct RotationData : IComponentData
        {
            public float2 Direction;
        }
    }

    //public static partial class BillBoadCustom
    //{
    //    public struct UvCursor : IComponentData
    //    {
    //        public int2 CurrentId;
    //        public int2 Length;
    //    }

    //    //public struct UvInfo : IComponentData
    //    //{
    //    //    //public uint2 Division;
    //    //    //public float2 UvWH;
    //    //    //public float2 UvOffset;
    //    //    public float2 Span;
    //    //}
    //}


    static public partial class BillBoad
    {
        //public static float2 CalcUv(this UvCursor cursor, UvParam uvinfo) =>
        //    CalcUv(cursor.CurrentIndex, uvinfo.Span, uvinfo.UMask, uvinfo.VShift);


        public static float2 CalcSpan(uint2 division) =>
            new float2(1.0f, 1.0f) / division;

        public static uint CalcUMask(uint2 division) => division.x - 1;
        public static int CalcVShift(uint2 division) => math.countbits(CalcUMask(division));
        

        //public static float2 CalcUv(int id, float2 span, uint umask, int vshift)
        //{
        //    var iu = id & umask;
        //    var iv = id >> vshift;
        //    var uv = span * new float2(iu, iv);
        //    return uv;
        //}
        //public static float2 CalcUv(int id, float2 span, uint umask, int vshift, float2 uvOffset)
        //{
        //    var iu = id & umask;
        //    var iv = id >> vshift;
        //    var uv = span * new float2(iu, iv);
        //    return uvOffset + uv;
        //}

        // 計算方法のリファレンスとして
        public static float2 CalcUv(int id, uint2 division)
        {
            var span = new float2(1,1) / division;

            var umask = division.x - 1;
            var vshift = math.countbits(umask);

            var iu = id & umask;
            var iv = id >> vshift;
            var uv = span * new float2(iu, iv);

            return uv;
        }
    }


}
