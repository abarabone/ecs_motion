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



    public static partial class BillboadModel
    {
        public struct UvInformationData : IComponentData
        {
            public uint2 Division;
        }

        public struct IndexToUvData : IComponentData
        {
            public float2 CellSpan;
        }
    }



    public static partial class Particle
    {

        // 破棄時刻指定
        public struct LifeTimeData : IComponentData
        {
            public float StartTime;
            //public float EndTime;
        }
        public struct LifeTimeSpecData : IComponentData
        {
            public float DurationSec;
        }
        public struct LifeTimeInitializeTag : IComponentData
        { }

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
        public struct UvAnimationWorkData : IComponentData
        {
            public float NextAnimationTime;
        }
        //public struct UvAnimationInitializeTag : IComponentData
        //{ }

        public struct SizeAnimationData : IComponentData
        {
            public float StartSize;
            public float EndSize;
            public float MaxTimeSpanR;
        }

        public struct UvAnimationData : IComponentData
        {
            public float TimeSpan;
            public float TimeSpanR;
            public int CursorAnimationMask;
            public int AnimationIndexMax;
        }
        public struct CursorToUvIndexData : IComponentData
        {
            public int IndexOffset;
            public byte UCellUsage;
            public byte VCellUsage;
            public byte UMask;
            public byte VShift;
        }

        // 二次元回転を、up の方向ベクトル x, y で表す
        // 回転行列は下記（ x=cosΘ, y=sinΘ とみなせるため）
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

        public static void ProgressCursorAnimation(
            ref this UvCursorData cursor, UvAnimationWorkData awork, UvAnimationData anim, double currentTime)
        {
            if (currentTime < awork.NextAnimationTime) return;

            cursor.CurrentIndex = ++cursor.CurrentIndex & anim.CursorAnimationMask;
        }

        public static int CalcUvIndex(this UvCursorData cursor, CursorToUvIndexData touv)
        {
            var i = cursor.CurrentIndex + touv.IndexOffset;
            var u = i & touv.UMask;
            var v = i >> touv.VShift;

            return wrap_((byte)u, (byte)v, touv.UCellUsage, touv.VCellUsage);

            int wrap_(byte iu, byte iv, byte usageu, byte usagev) =>
                (iu << 0) | (iv << 8) | (usageu << 16) | (usagev << 24);
        }

        //public static float2 CalcSpan(uint2 division) =>
        //    new float2(1.0f, 1.0f) / division;

        //public static uint CalcUMask(uint2 division) => division.x - 1;
        //public static int CalcVShift(uint2 division) => math.countbits(CalcUMask(division));
        

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
