using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace DotsLite.HeightGrid
{
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Common;
    using DotsLite.Utilities;


    [Serializable]
    public struct GridBinaryLength2
    {
        public binary_length_grid u;
        public binary_length_grid v;
        public int x { get => (int)this.u; set => this.u = (binary_length_grid)value; }
        public int y { get => (int)this.v; set => this.v = (binary_length_grid)value; }
        public static implicit operator int2(GridBinaryLength2 src) => new int2((int)src.u, (int)src.v);
    }
    public enum binary_length_grid
    {
        //length_1 = 1,
        //length_2 = 2,
        //length_4 = 4,
        length_8 = 8,
        length_16 = 16,
        length_32 = 32,
        length_64 = 64,
        length_128 = 128,
        length_256 = 256,
    }

    public static class Wave
    {
        public class GridMasterData : IComponentData, IDisposable
        {
            public NativeArray<float> Nexts;
            public NativeArray<float> Currs;
            public NativeArray<float> Prevs;

            public GridMasterInfo Info;

            public void Dispose()
            {
                this.Nexts.Dispose();
                this.Currs.Dispose();
                this.Prevs.Dispose();
                Debug.Log("disposed");
            }
        }
        public struct GridMasterInfo
        {
            public float3 LeftTopLocation;
            public int2 UnitLengthInGrid;
            public int2 NumGrids;
            public float UnitScale;
            public float Dumping;
            public float Constraint2;

            public int2 TotalLength;
            public float UnitScaleRcp;
        }
    }

    public static class Height
    {
        public struct GridLevel0Tag : IComponentData
        { }

        public struct GridData : IComponentData
        {
            public int LodLevel;
            public int2 GridId;
            public float UnitScaleOnLod;
        }

        public struct BoundingBox : IComponentData
        {
            public AABB WorldBbox;
        }
    }


    public static partial class HeightGridUtility
    {
        //public static int ToLinear(this Height.GridData grid, Wave.GridMasterData master)
        //{
        //    var wspan = master.UnitLengthInGrid.x * master.NumGrids.x;
        //    var i = grid.GridId;// >> grid.LodLevel;
        //    return i.x + i.y * wspan;
        //}

        //public static int2 CalcHitIndex(this Wave.GridMasterData master, float2 point)
        //{

        //    master.LeftTopPosition.

        //    return int2.zero;
        //}

        //public static int2 CalcHitIndex(this Wave.GridMasterData master, float3 point, float3 dir, float length)
        //{



        //    return int2.zero;
        //}

        public static unsafe float CalcWaveHeight(this Wave.GridMasterInfo info, float* pHeight, float2 point)
        {
            var xz = point - info.LeftTopLocation.xz;
            var i = xz * info.UnitScaleRcp;

            var index2 = (int2)i;

            var serialIndex = index2.x + index2.y * info.TotalLength.x;

            var i0 = serialIndex + 0;
            var i1 = serialIndex + 1;
            var i2 = serialIndex + info.TotalLength.x + 0;
            var i3 = serialIndex + info.TotalLength.x + 1;

            var h00 = pHeight[i0];
            var h01 = pHeight[i1];
            var h02 = pHeight[i2];

            var h10 = pHeight[i3];
            var h11 = pHeight[i2];
            var h12 = pHeight[i1];

            var lxz = i - index2;
            var lxzanti = 1 - lxz;


            //var _p0 = lxz;
            //var _pl0 = new float4(h01, -1, h02, h00);

            //var _p1 = 1 - lxz;
            //var _pl1 = new float4(h11, -1, h12, h10);

            //var is1 = lxz.x + lxz.y > 1.0f;
            //var _p = math.select(_p0, _p1, is1);
            //var _pl = math.select(_pl0, _pl1, is1);

            //var h = calc_(_p, _pl);
            //Debug.Log($"{lxz} {_pl.w:f2},{_pl.x:f2},{_pl.z:f2} {h:f2}");

            //return h;


            //float calc_(float2 _p, float4 _pl)
            //{
            //    var p = new float4(_p.x, _pl.w, _p.y, 1.0f);
            //    var n = math.normalize(_pl.xyz);
            //    var d = n.y * _pl.w;
            //    var pl = new float4(n, d);
            //    var h = n.y * math.dot(pl, p);
            //    return h;
            //}
            //float calc_(float h00, float h01, float h02)
            //{
            //    var p = new float4(lxz.x, h00, lxz.y, 1.0f);
            //    //var pl = new float4(h01, -1, h02, h00);
            //    var p00 = new float3(0, h00, 0);
            //    var p01 = new float3(1, h01, 0);
            //    var p02 = new float3(0, h02, 1);
            //    var u = p01 - p00;// math.normalize(p01);
            //    var v = p02 - p00;// math.normalize(p02);
            //    var n = math.normalize(math.cross(v, u));
            //    var pl = n.As_float4(math.dot(p00.xyz, n));
            //    var res = p00 + n * math.dot(pl, p);
            //    var h = res.y;

            //    Debug.Log($"{point} {lxz} {info.LeftTopLocation.xz} {xz} {index2} {info.TotalLength.x} {serialIndex} {h00:f2},{h01:f2},{h02:f2} {h:f2}");
            //    return h;
            //}

            float calc_(float h00, float h01, float h02)
            {
                var u = (h01 - h00) * lxz.x;
                var v = (h02 - h00) * lxz.y;
                var hf = (u + v) * 0.5f;
                Debug.Log($"{lxz} {h00:f2},{h01:f2},{h02:f2} {h00+hf+hf:f2}");
                return h00 + hf + hf;
            }
            var is1 = lxz.x + lxz.y > 1.0f;
            var h = is1
                ? calc_(h00, h01, h02)
                : calc_(h10, h02, h01);

            return h;
        }

        //p0 = 0, h00, 0

        //u1 = 1, h01, 0
        //v1 = 0, h02, 1

        //n = u1 x v1
        //  = h01*1 - 0*h02, 0*0 - 1*1, 1*h02 - h01*0
        //  = h01, -1, h02


        //d = 0 * h01 + h00 * -1 + 0 * h02 = -h00
        //pl = h01, -1, h02, -h00

    }

    public struct RightTriangle
    {
        public float3 BasePoint;
        public float2 UvLengthRcp;

        //public float3 GravityHit(float3 p, float length)
        //{

        //    var uv = p.xz - this.BasePoint.xz;

        //    var nmuv = uv * this.UvLengthRcp;

        //    var isHit = 1.0f >= nmuv.x + nmuv.y;
        //}
        //public float3 RaycastHit(float3 p, float3 dir, float length)
        //{

        //    var a = math.cross(dir, )

        //}
    }
}
namespace DotsLite.Mathematics
{
    using DotsLite.Common;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Utility;
    using DotsLite.Utilities;

    public struct triangle
    {
        public float3 BasePoint;
        public float2 UvLengthRcp;

        //public float3 RaycastHit(float3 p, float3 dir, float length)
        //{

        //    //var a = math.cross(dir, )

        //}
    }

    public struct plane
    {
        public float4 nd;
        public float4 value => this.nd;
        public float3 n => this.nd.xyz;
        public float d => this.nd.w;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public plane(float3 p, float3 n)
        {
            this.nd = n.As_float4(math.dot(p, n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFrontSide(float4 p) => this.distanceSigned(p) >= 0.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool isFrontSide(float3 p) => this.distanceSigned(p) >= 0.0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float distanceSigned(float4 p) => math.dot(this.nd, p);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float distanceSigned(float3 p) => this.distanceSigned(p.As_float4(1.0f));
    }
}