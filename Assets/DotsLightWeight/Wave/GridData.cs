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
    using DotsLite.Mathematics;


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

    public static class GridMaster
    {
        public class Data : IComponentData, IDisposable
        {
            public NativeArray<float> Nexts;
            public NativeArray<float> Currs;
            public NativeArray<float> Prevs;

            public Info Info;

            public void Dispose()
            {
                this.Nexts.Dispose();
                this.Currs.Dispose();
                this.Prevs.Dispose();
                Debug.Log("disposed");
            }
        }
        public struct Info
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

        public struct Emitting : IComponentData
        {
            public Entity SplashPrefab;
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


        static unsafe int2 calcLocation(ref this GridMaster.Info info, float* pHeight, float2 point)
        {
            var wxz = point - info.LeftTopLocation.xz;
            var i = wxz * info.UnitScaleRcp;

            var index2 = (int2)i;
            return index2;
        }
        static int calcSerialIndex(ref this GridMaster.Info info, int2 index2)
        {
            var serialIndex = index2.x + index2.y * info.TotalLength.x;

            return serialIndex;
        }


        /// <summary>
        /// 
        /// </summary>
        public static unsafe float CalcVerticalHeight(ref this GridMaster.Info info, float* pHeight, float2 point)
        {
            var wxz = point - info.LeftTopLocation.xz;
            var i = wxz * info.UnitScaleRcp;

            var index2 = (int2)i;
            if (math.any(index2 < int2.zero) || math.any(index2 >= info.TotalLength)) return float.NaN;

            var serialIndex = index2.x + index2.y * info.TotalLength.x;

            var i0 = serialIndex + 0;
            var i1 = serialIndex + 1;
            var i2 = serialIndex + info.TotalLength.x + 0;
            var i3 = serialIndex + info.TotalLength.x + 1;


            var lxz = i - index2;
            var is1 = lxz.x + lxz.y > 1.0f;

            var pH = pHeight;
            var h0_ = new float3(pH[i0], pH[i1], pH[i2]);
            var h1_ = new float3(pH[i3], pH[i2], pH[i1]);
            var h_ = math.select(h0_, h1_, is1);

            var lxz0_ = lxz;
            var lxz1_ = 1.0f - lxz;
            var lxz_ = math.select(lxz0_, lxz1_, is1);


            var uv = (h_.yz - h_.xx) * lxz_;
            var h = h_.x + uv.x + uv.y;

            Debug.DrawLine(point.x_y(-100.0f), point.x_y(h), Color.red);

            return h;
        }

        //p0 = 0, h0, 0

        //u = (1, h1, 0) - (0, h0, 0) => (1, h1-h0, 0)
        //v = (0, h2, 1) - (0, h0, 0) => (0, h2-h0, 1)

        //n_ = u x v
        //  = (h1-h0)*1 - 0*(h2-h0), 0*0 - 1*1, 1*(h2-h0) - (h1-h0)*0
        //  = h1-h0, -1, h2-h0

        // l = sqrt((h1-h0) * (h1-h0) + 1 + (h2-h0) * (h2-h0))
        // n = (h1-h0) / l, -1/l, (h2-h0) / l

        //d = 0 * h01 + h00 * -1 + 0 * h02 = -h00
        //pl = h01, -1, h02, -h00

        // (h1-h0)^2 = h1 * h1 - 2 * h1 * h0 + h0 * h0
        // (h2-h0)^2 = h2 * h2 - 2 * h2 * h0 + h0 * h0
        // h1*h1 - 2*h1*h0 + 2*h0*h0 + h2*h2 - 2*h2*h0

        // d = (0 * (h1-h0)/l + h0 * -1/l + 0 * (h2-h0)/l)
        //   = -h0 / l


        public static unsafe float RaycastHit(this GridMaster.Info info, float* pHeight, float3 start, float3 dir, float length)
        {
            

            return 0;
        }
        public static unsafe (bool isHit, float3 p) RaycastHit(this GridMaster.Info info, float* pHeight, float3 start, float3 end)
        {
            var wxz_st = start.xz - info.LeftTopLocation.xz;
            var ist = wxz_st * info.UnitScaleRcp;

            var wxz_ed = end.xz - info.LeftTopLocation.xz;
            var ied = wxz_ed * info.UnitScaleRcp;

            //var ist = math.min(ist_, ied_);
            //var ied = math.max(ist_, ied_);



            //Debug.Log($"ww {start.xz} {end.xz} {info.LeftTopLocation.xz}");
            //Debug.Log($"wxz {wxz_st} {wxz_ed}");
            //Debug.Log($"ist {ist} ied {ied}");

            var index2st = (int2)ist;
            var index2ed = (int2)ied;
            //if (math.any(index2 < int2.zero) || math.any(index2 >= info.TotalLength)) return float.NaN;
            
            var imin = math.min(index2ed, index2st);
            var imax = math.max(index2ed, index2st);
            var len = imax - imin + 1;


            //// h = a * p + b
            //// a = (h1 - h0) / (p1 - p0)
            //// b = h - a * p
            var lnax = (end.yy * info.UnitScaleRcp - start.yy * info.UnitScaleRcp) / (ied - ist);
            var lnbx = end.yy * info.UnitScaleRcp - lnax * ied;
            //Debug.Log($"ln outer {lnax} {lnbx} {start.yy * info.UnitScaleRcp - lnax * ist}");


            var i0 = index2st.x + 0;
            var i1 = index2st.x + 1;
            var i2 = index2st.y * info.TotalLength.x + 0;
            var i3 = index2st.y * info.TotalLength.x + 1;

            var pH = pHeight;

            var ibasest = ist - imin;
            var ibaseed = ied - imin;// st にあわせたい
            for (var iz = 0; iz < len.y; iz++)
            for (var ix = 0; ix < len.x; ix++)// 左右をつなげる処理まだやってない
            {
                    //Debug.Log($"{ix} {iz}");
                    var ofs = ix + iz * info.TotalLength.x;
                    var h0 = pH[i0 + ofs] * info.UnitScaleRcp;
                    var h1 = pH[i1 + ofs] * info.UnitScaleRcp;
                    var h2 = pH[i2 + ofs] * info.UnitScaleRcp;
                    var h3 = pH[i3 + ofs] * info.UnitScaleRcp;

                    var i = new float2(ix, iz);// * info.UnitScale;
                                                    //// i0 の点が xz の原点になるようにする
                                                    //Debug.Log($"{offset}");

                    //Debug.Log($"{start} {end} {i} {imin} {imax} {i + imin}");
                    // h = a * p + b
                    // a = (h1 - h0) / (p1 - p0)
                    // b = h - a * p
                    var lst = ibasest - i;
                    var led = ibaseed - i;
                    var lna = (end.yy * info.UnitScaleRcp - start.yy * info.UnitScaleRcp) / (led - lst) ;
            var lnb = end.yy * info.UnitScaleRcp - lna *led;
                    //Debug.Log($"lna:{lna} lnb:{lnb} {start.yy * info.UnitScaleRcp - lna * lst}");


                    var wvhA = new float3(h1, h0, h2);
                    var lnstA = lst;//ist - i;
                    var lnedA = led;// lst;// ied - i;
                    var resA = RaycastHit(wvhA, lnstA, lnedA, lna, lnb);// あとで近いものを採用するように
                    
                    if (resA.isHit) return (resA.isHit, resA.p);

                    var wvhB = new float3(h2, h3, h1);
                    var lnstB = 1.0f - lnstA;
                    var lnedB = 1.0f - lnedA;
                    var resB = RaycastHit(wvhB, lnstB, lnedB, lna, lnb);
                    
                    if (resB.isHit) return (resB.isHit, resB.p);
                }

            return (false, default);
        }
        public static unsafe (bool isHit, float3 p) RaycastHit(float3 wvh, float2 lnst, float2 lned, float2 lna, float2 lnb)
        {
            //Debug.Log($"ln {lnst} {lned} {lna} {lnb}");

            // wva = (wvh1or2 - wvh0) / (1 - 0)
            // wvb = wvh0 - wva * 0; wvh0 のとき
            // wvb = wvh1or2 - wva * 1; wvh1or2 のとき
            var wva = wvh.xz - wvh.yy;
            var wvb = wvh.yy;
            //Debug.Log($"wv {wvh} {wva} {wvb} {wvh.xz - wva}");

            // wvh = wva * wvp + wvb
            // lnh = lna * lnp + lnb
            // p = (lnb - wvb) / (lna - wva)
            // h = (wva * lnb - wvb * lna) / (lna - wva)
            //var darcp = 1.0f / (lna - wva);
            var darcp = math.rcp(lna - wva);
            var uv = (lnb - wvb) * darcp;
            var h = (wva * lnb - wvb * lna) * darcp;
            //Debug.Log($"uvh {uv} {h}");

            //var darcp2 = math.rcp(wva - lna);
            //var uv2 = (wvb - lnb) * darcp2;
            //var h2 = (wvb * lna - wva * lnb) * darcp2;
            ////Debug.Log($"uvh2 {uv2} {h2}");

            if (math.any(uv < 0.0f | uv > 1.0f)) return (false, default);
            Debug.Log("hit"); Debug.Log($"uvh {uv} {h}");

            var lu = new float3(uv.x, h.x, 0);
            var lv = new float3(0, h.y, uv.y);
            var l = (lu + lv) * 0.5f;

            return (true, l);
        }
        //public static unsafe float RaycastHit(float3 h_, float2 lst, float2 led)
        //{
        //    var lxz_ = new float4(lst, led);

        //    var duv = h_.yzyz - h_.xxxx;
        //    var uv = duv * lxz_;                // duv.xy : stuv, duv.zw : eduv
        //    var h = h_.xx + uv.xz + uv.yw;      // h.x : hst, h.y : hed

        //    var dsted = h.xxyy / lxz_;          // lst と led の傾き





        //    return 0;
        //}
        //static float calcLineUv(float3 st, float3 ed)
        //{
        //    var a = (ed.yy - st.yy) / (ed.xz - st.xz);
        //    var b = st.xz * a - st.yy;
        //}
        //static float calcPlaneHit()
        //{
        //    // ly = ua * lx + ub
        //    // ly = va * lz + vb

        //    // h1 = ua * uvx + 

        //}


        public static unsafe (bool isHit, float3 p) RaycastHit2(
            this GridMaster.Info info, float* pHeight, float3 start, float3 end)
        {
            var wxz_st = start.xz - info.LeftTopLocation.xz;
            var ist = wxz_st * info.UnitScaleRcp;

            var wxz_ed = end.xz - info.LeftTopLocation.xz;
            var ied = wxz_ed * info.UnitScaleRcp;

            //var ist = math.min(ist_, ied_);
            //var ied = math.max(ist_, ied_);



            //Debug.Log($"ww {start.xz} {end.xz} {info.LeftTopLocation.xz}");
            //Debug.Log($"wxz {wxz_st} {wxz_ed}");
            //Debug.Log($"ist {ist} ied {ied}");

            var index2st = (int2)ist;
            var index2ed = (int2)ied;
            //if (math.any(index2 < int2.zero) || math.any(index2 >= info.TotalLength)) return float.NaN;

            var imin = math.min(index2ed, index2st);
            var imax = math.max(index2ed, index2st);
            var len = imax - imin + 1;


            //// h = a * p + b
            //// a = (h1 - h0) / (p1 - p0)
            //// b = h - a * p
            var lnax = (end.yy * info.UnitScaleRcp - start.yy * info.UnitScaleRcp) / (ied - ist);
            var lnbx = end.yy * info.UnitScaleRcp - lnax * ied;
            //Debug.Log($"ln outer {lnax} {lnbx} {start.yy * info.UnitScaleRcp - lnax * ist}");


            var i0 = index2st.x + 0;
            var i1 = index2st.x + 1;
            var i2 = index2st.y * info.TotalLength.x + 0;
            var i3 = index2st.y * info.TotalLength.x + 1;

            var pH = pHeight;

            var ibasest = ist - imin;
            var ibaseed = ied - imin;// st にあわせたい
            for (var iz = 0; iz < len.y; iz++)
                for (var ix = 0; ix < len.x; ix++)// 左右をつなげる処理まだやってない
                {
                    //Debug.Log($"{ix} {iz}");
                    var ofs = ix + iz * info.TotalLength.x;
                    var h0 = pH[i0 + ofs];
                    var h1 = pH[i1 + ofs];
                    var h2 = pH[i2 + ofs];
                    var h3 = pH[i3 + ofs];

                    var i = new float2(ix, iz);
                                               //// i0 の点が xz の原点になるようにする
                                               //Debug.Log($"{offset}");

                    //Debug.Log($"{start} {end} {i} {imin} {imax} {i + imin}");
                    // h = a * p + b
                    // a = (h1 - h0) / (p1 - p0)
                    // b = h - a * p
                    var lst = ibasest - i;
                    var led = ibaseed - i;
                    var lna = (end.yy * info.UnitScaleRcp - start.yy * info.UnitScaleRcp) / (led - lst);
                    var lnb = end.yy * info.UnitScaleRcp - lna * led;


                    var wvhA = new float3(h1, h0, h2);
                    var lnstA = lst;
                    var lnedA = led;// あとで近いものを採用するように
                    var resA = RaycastHit2(info, wvhA, imin + i, imin + i + new float2(1, 0), imin + i + new float2(0, 1), start, end);// あとで近いものを採用するように

                    if (resA.isHit) return (resA.isHit, resA.p);

                    var wvhB = new float3(h2, h3, h1);
                    var lnstB = 1.0f - lnstA;
                    var lnedB = 1.0f - lnedA;
                    var resB = RaycastHit2(info, wvhB, imin + i + new float2(1, 1), imin + i + new float2(0, 1), imin + i + new float2(1, 0), start, end);

                    if (resB.isHit) return (resB.isHit, resB.p);
                }

            return (false, default);
        }
        public static unsafe (bool isHit, float3 p) RaycastHit2(
            GridMaster.Info info, float3 wvh, float2 i0, float2 i1, float2 i2, float3 st, float3 ed)
        {
            var p0 = info.LeftTopLocation + (i0 * info.UnitScale).x_y(wvh.y);
            var p1 = info.LeftTopLocation + (i1 * info.UnitScale).x_y(wvh.x);
            var p2 = info.LeftTopLocation + (i2 * info.UnitScale).x_y(wvh.z);
            var pl = new Plane(p0, p1, p2);

            var ray = new Ray(st, math.normalize(ed - st));
            var isHitPl = pl.Raycast(ray, out var t);
            var p = t * 1.0f * ray.direction.As_float3() + st;
            if (!isHitPl && t <= 0) return (false, default);
            if (t > math.dot(ray.direction, ed - st)) return (false, default);

            var c0 = math.cross(p - p0, p1 - p0);
            var c1 = math.cross(p - p1, p2 - p1);
            var c2 = math.cross(p - p2, p0 - p2);

            var isHit = math.sign(math.dot(c0, c1)) == math.sign(math.dot(c0, c2));
            //if (isHit) Debug.Log(p);
            
            return (isHit, p);
        }
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
        public plane(float3 p0, float3 p1, float3 p2)
        {
            var up = math.cross((p1 - p0), (p2 - p0));
            var n = math.normalize(up);
            this = new plane(p0, n);
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