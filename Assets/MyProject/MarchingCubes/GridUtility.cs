using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{

    public unsafe interface ICubeInstanceWriter
    {
        void Add(CubeInstance ci);
        void AddRange(CubeInstance* pCi, int length);
    }
    public unsafe struct InstanceCubeByList : ICubeInstanceWriter
    {
        [WriteOnly]
        public NativeList<CubeInstance> list;
        public void Add(CubeInstance ci) => list.AddNoResize(ci);
        public void AddRange(CubeInstance* pCi, int length) => list.AddRangeNoResize(pCi, length);
    }
    public unsafe struct InstanceCubeByParaList : ICubeInstanceWriter
    {
        [WriteOnly]
        public NativeList<CubeInstance>.ParallelWriter list;
        public void Add(CubeInstance ci) => list.AddNoResize(ci);
        public void AddRange(CubeInstance* pCi, int length) => list.AddRangeNoResize(pCi, length);
    }
    public unsafe struct InstanceCubeByParaQueue : ICubeInstanceWriter
    {
        [WriteOnly]
        public NativeQueue<CubeInstance>.ParallelWriter queue;
        public void Add(CubeInstance ci) => queue.Enqueue(ci);
        public void AddRange(CubeInstance* pCi, int length) => queue.Enqueue(*pCi);// キューは範囲追加ムリ
    }
    public unsafe struct InstanceCubeByTempMem : ICubeInstanceWriter
    {
        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        [NativeDisableParallelForRestriction]
        public CubeInstance* p;
        public int length;
        public void Add(CubeInstance ci) => p[length++] = ci;
        public void AddRange(CubeInstance* pCi, int length) =>
            UnsafeUtility.MemCpy(p, pCi, length * sizeof(CubeInstance));
    }

    public unsafe struct InstanceCubeByUnsafeList : ICubeInstanceWriter
    {
        [WriteOnly]
        public UnsafeList<CubeInstance> list;
        public void Add(CubeInstance ci) => list.AddNoResize(ci);
        public void AddRange(CubeInstance* pCi, int length) => list.AddRangeNoResize(pCi, length);
    }



    static public unsafe class GridUtility
    {



        public struct AdjacentGrids
        {
            public int gridId;
            public HalfGridUnit L;
            public HalfGridUnit R;

            public struct HalfGridUnit
            {
                public DotGrid32x32x32UnsafePtr x;
                public DotGrid32x32x32UnsafePtr y;
                public DotGrid32x32x32UnsafePtr z;
                public DotGrid32x32x32UnsafePtr w;
            }
        }


        static unsafe DotGrid32x32x32UnsafePtr toPtr(in this DotGridArea.BufferData gridArea, int i) =>
            new DotGrid32x32x32UnsafePtr
            {
                p = gridArea.Grids.Ptr + i,
            };



        /// <summary>
        /// 
        /// </summary>
        static public unsafe AdjacentGrids getGridSet_
            (in this DotGridArea.BufferData gridArea, int ix, int iy, int iz, int3 gridSpan) =>
            gridArea.getGridSet_(new int3(ix, iy, iz), gridSpan);

        static public unsafe AdjacentGrids getGridSet_
            (in this DotGridArea.BufferData gridArea, int3 index, int3 gridSpan)
        {
            var i = math.dot(index, gridSpan);

            return new AdjacentGrids
            {
                L =
                {
                    x = gridArea.toPtr( i + 0 ),
                    y = gridArea.toPtr( i + gridSpan.y + 0 ),
                    z = gridArea.toPtr( i + gridSpan.z + 0 ),
                    w = gridArea.toPtr( i + gridSpan.y + gridSpan.z + 0 ),
                },
                R =
                {
                    x = gridArea.toPtr( i + 1 ),
                    y = gridArea.toPtr( i + gridSpan.y + 1 ),
                    z = gridArea.toPtr( i + gridSpan.z + 1 ),
                    w = gridArea.toPtr( i + gridSpan.y + gridSpan.z + 1 ),
                },
            };
        }

        public struct GridCounts
        {
            public int4 L, R;
        }
        static public unsafe GridCounts getEachCount(in this AdjacentGrids g)
        {
            var gridCount = new int4
            (
                g.L.x.p->CubeCount,
                g.L.y.p->CubeCount,
                g.L.z.p->CubeCount,
                g.L.w.p->CubeCount
            );
            var gridCount_right = new int4
            (
                g.R.x.p->CubeCount,
                g.R.y.p->CubeCount,
                g.R.z.p->CubeCount,
                g.R.w.p->CubeCount
            );
            return new GridCounts { L = gridCount, R = gridCount_right };
        }


        static public bool isNeedDraw_(int4 gridCount, int4 gridCount_right)
        {
            var addvalue = gridCount + gridCount_right;
            var isZero = !math.any(addvalue);
            var isFull = math.all(addvalue == 0x8000 << 1);
            return !(isZero | isFull);
        }

        /// <summary>
        /// グリッドとその右グリッドが、同じフィルなら描画の必要はない。
        /// 上下と前後はやらなくていいのか？
        /// </summary>
        static public bool isNeedDraw_(in this GridCounts gridCounts)
        {
            var addvalue = gridCounts.L + gridCounts.R;
            var isBlank = !math.any(addvalue);
            var isSolid = math.all(addvalue == 0x8000 << 1);
            return !(isBlank | isSolid);
        }








        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void SampleAllCubes<TCubeInstanceWriter>
        //    (ref AdjacentGrids g, int gridId, ref TCubeInstanceWriter outputCubes)
        //    where TCubeInstanceWriter : ICubeInstanceWriter
        //{

        //    var gcount = g.getEachCount();

        //    SampleAllCubes(ref g, ref gcount, gridId, ref outputCubes);

        //}


        // ------------------------------------------------------------------

        /// <summary>
        /// native contener 化必要、とりあえずは配列で動作チェック
        /// あとでＹＺカリングもしたい
        /// </summary>
        // xyz各32個目のキューブは1bitのために隣のグリッドを見なくてはならず、効率悪いしコードも汚くなる、なんとかならんか？
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SampleAllCubes
            (ref this AdjacentGrids g, ref GridCounts gcount, int gridId, uint *pOutput, ref int outputCounter)
        {
            var g0or1L = math.min(gcount.L & 0x7fff, new int4(1, 1, 1, 1));
            var g0or1R = math.min(gcount.R & 0x7fff, new int4(1, 1, 1, 1));

            var g0or1x = g0or1L.x | g0or1R.x | (g.L.x.p->IsFull ^ g.R.x.p->IsFull).AsByte();
            //var g0or1y = g0or1L.y | g0or1R.y | ( g.L.y.p->IsFull ^ g.R.y.p->IsFull ).AsByte();
            //var g0or1z = g0or1L.z | g0or1R.z | ( g.L.z.p->IsFull ^ g.R.z.p->IsFull ).AsByte();

            var g0or1Lxxxx = g0or1L.xxxx;
            var g0or1Rxxxx = g0or1R.xxxx;
            var g0or1Lxxzz = g0or1L.xxzz;
            var g0or1Rxxzz = g0or1R.xxzz;
            var g0or1Lxyxy = g0or1L.xyxy;
            var g0or1Rxyxy = g0or1R.xyxy;
            var g0or1Lxyzw = g0or1L;
            var g0or1Rxyzw = g0or1R;

            for (var iy = 0; iy < 31; iy++)
            {
                for (var iz = 0; iz < (31 * g0or1x & ~0x3); iz += 4)
                {
                    var c = getXLine_(iy, iz, g0or1Lxxxx, g.L.x, g.L.x, g.L.x, g.L.x);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g0or1Rxxxx, g.R.x, g.R.x, g.R.x, g.R.x);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    cubes.storeTo(pOutput, ref outputCounter, gridId, iy, iz);
                }
                {
                    const int iz = 31 & ~0x3;

                    var c = getXLine_(iy, iz, g0or1Lxxzz, g.L.x, g.L.x, g.L.z, g.L.z);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g0or1Rxxzz, g.R.x, g.R.x, g.R.z, g.R.z);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    cubes.storeTo(pOutput, ref outputCounter, gridId, iy, iz);
                }
            }
            {
                const int iy = 31;
                for (var iz = 0; iz < (31 & ~0x3); iz += 4)
                {
                    var c = getXLine_(iy, iz, g0or1Lxyxy, g.L.x, g.L.y, g.L.x, g.L.y);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g0or1Rxyxy, g.R.x, g.R.y, g.R.x, g.R.y);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    cubes.storeTo(pOutput, ref outputCounter, gridId, iy, iz);
                }
                {
                    const int iz = 31 & ~0x3;

                    var c = getXLine_(iy, iz, g0or1Lxyzw, g.L.x, g.L.y, g.L.z, g.L.w);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g0or1Rxyzw, g.R.x, g.R.y, g.R.z, g.R.w);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    cubes.storeTo(pOutput, ref outputCounter, gridId, iy, iz);
                }
            }

        }


        public struct CubeXLineBitwise// タプルだと burst 利かないので
        {
            public uint4 _98109810, _a921a921, _ba32ba32, _cb43cb43, _dc54dc54, _ed65ed65, _fe76fe76, _0f870f87;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CubeXLineBitwise
            (
                uint4 _98109810, uint4 _a921a921, uint4 _ba32ba32, uint4 _cb43cb43,
                uint4 _dc54dc54, uint4 _ed65ed65, uint4 _fe76fe76, uint4 _0f870f87
            )
            {
                this._98109810 = _98109810;
                this._a921a921 = _a921a921;
                this._ba32ba32 = _ba32ba32;
                this._cb43cb43 = _cb43cb43;
                this._dc54dc54 = _dc54dc54;
                this._ed65ed65 = _ed65ed65;
                this._fe76fe76 = _fe76fe76;
                this._0f870f87 = _0f870f87;
            }
        }

        public struct CubeNearXLines// タプルだと burst 利かないので
        {
            public uint4 y0z0, y0z1, y1z0, y1z1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CubeNearXLines(uint4 y0z0, uint4 y0z1, uint4 y1z0, uint4 y1z1)
            {
                this.y0z0 = y0z0;
                this.y0z1 = y0z1;
                this.y1z0 = y1z0;
                this.y1z1 = y1z1;
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void storeTo
            (ref this CubeXLineBitwise cubes, uint* pOutput, ref int outputCounter, int gridid, int iy, int iz)
        {
            storeCubeInstances(pOutput, ref outputCounter, cubes._98109810, gridid, ix: 0, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._a921a921, gridid, ix: 1, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._ba32ba32, gridid, ix: 2, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._cb43cb43, gridid, ix: 3, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._dc54dc54, gridid, ix: 4, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._ed65ed65, gridid, ix: 5, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._fe76fe76, gridid, ix: 6, iy, iz);
            storeCubeInstances(pOutput, ref outputCounter, cubes._0f870f87, gridid, ix: 7, iy, iz);
        }


        //static int 


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe CubeNearXLines getXLine_(
            int iy, int iz, int4 index0or1,
            DotGrid32x32x32UnsafePtr gx, DotGrid32x32x32UnsafePtr gy,
            DotGrid32x32x32UnsafePtr gz, DotGrid32x32x32UnsafePtr gw
        )
        {
            //y0  -> ( iy + 0 & 31 ) * 32/4 + ( iz>>2 + 0 & 31>>2 );
            //y1  -> ( iy + 1 & 31 ) * 32/4 + ( iz>>2 + 0 & 31>>2 );
            //y0r -> ( iy + 0 & 31 ) * 32 + ( iz + 1<<2 & 31 );
            //y1r -> ( iy + 1 & 31 ) * 32 + ( iz + 1<<2 & 31 );
            var iy_ = iy;
            var iz_ = new int4(iz >> 2, iz >> 2, iz, iz);
            var yofs = new int4(0, 1, 0, 1);
            var zofs = new int4(0, 0, 1 << 2, 1 << 2);
            var ymask = 31;
            var zmask = new int4(31 >> 2, 31 >> 2, 31, 31);
            var yspan = new int4(32 / 4, 32 / 4, 32, 32);

            var _i = (iy_ + yofs & ymask) * yspan + (iz_ + zofs & zmask);
            var i = _i;// * index0or1;
            var y0 = ((uint4*)gx.p->pUnits)[i.x];
            var y1 = ((uint4*)gy.p->pUnits)[i.y];
            //var y0 = index0or1.x > 0 ? ( (uint4*)gx.p->pUnits )[ i.x ] : (uint)(gx.p->IsEmpty.AsByte() - 1);
            //var y1 = index0or1.y > 0 ? ( (uint4*)gy.p->pUnits )[ i.y ] : (uint)(gy.p->IsEmpty.AsByte() - 1);
            var y0z0 = y0;
            var y1z0 = y1;

            y0.x = gz.p->pUnits[i.z];
            y1.x = gw.p->pUnits[i.w];
            //y0.x = index0or1.z > 0 ? gz.p->pUnits[ i.z ] : (uint)( gz.p->IsEmpty.AsByte() - 1 );
            //y1.x = index0or1.w > 0 ? gw.p->pUnits[ i.w ] : (uint)( gw.p->IsEmpty.AsByte() - 1 );
            var y0z1 = y0.yzwx;
            var y1z1 = y1.yzwx;

            return new CubeNearXLines(y0z0, y0z1, y1z0, y1z1);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // あらかじめ共通段階（キューブ手前）までビット操作しておいたほうが速くなるかも、でも余計なエリアにストアするから、逆効果の可能性もある
        static CubeXLineBitwise bitwiseCubesXLine_(uint4 y0z0, uint4 y0z1, uint4 y1z0, uint4 y1z1)
        {
            if (!math.any(y0z0 | y0z1 | y1z0 | y1z1)) return new CubeXLineBitwise();

            // fedcba9876543210fedcba9876543210

            var m1100 = 0b_11001100_11001100_11001100_11001100u;
            var m0011 = m1100 >> 2;
            // --dc--98--54--10--dc--98--54--10
            // dc--98--54--10--dc--98--54--10--
            // fe--ba--76--32--fe--ba--76--32--
            // --fe--ba--76--32--fe--ba--76--32
            var y0_dc985410 = y0z0 & m0011 | (y0z1 & m0011) << 2;
            var y0_feba7632 = (y0z0 & m1100) >> 2 | y0z1 & m1100;
            var y1_dc985410 = y1z0 & m0011 | (y1z1 & m0011) << 2;
            var y1_feba7632 = (y1z0 & m1100) >> 2 | y1z1 & m1100;
            // dcdc989854541010dcdc989854541010
            // fefebaba76763232fefebaba76763232
            // dcdc989854541010dcdc989854541010
            // fefebaba76763232fefebaba76763232

            var mf0 = 0x_f0f0_f0f0u;
            var m0f = 0x_0f0f_0f0fu;
            // ----9898----1010----9898----1010
            // dcdc----5454----dcdc----5454----
            // ----baba----3232----baba----3232
            // fefe----7676----fefe----7676----
            var _98109810 = y0_dc985410 & m0f | (y1_dc985410 & m0f) << 4;
            var _dc54dc54 = (y0_dc985410 & mf0) >> 4 | y1_dc985410 & mf0;
            var _ba32ba32 = y0_feba7632 & m0f | (y1_feba7632 & m0f) << 4;
            var _fe76fe76 = (y0_feba7632 & mf0) >> 4 | y1_feba7632 & mf0;
            // 98989898101010109898989810101010
            // dcdcdcdc54545454dcdcdcdc54545454
            // babababa32323232babababa32323232
            // fefefefe76767676fefefefe76767676

            var m55 = 0x_5555_5555u;
            var maa = 0x_aaaa_aaaau;
            var _a921a921 = (_ba32ba32 & m55) << 1 | (_98109810 & maa) >> 1;
            var _cb43cb43 = (_dc54dc54 & m55) << 1 | (_ba32ba32 & maa) >> 1;
            var _ed65ed65 = (_fe76fe76 & m55) << 1 | (_dc54dc54 & maa) >> 1;
            var __f870f87 = (_98109810 >> 8 & 0x_55_5555u) << 1 | (_fe76fe76 & maa) >> 1;
            // a9a9a9a921212121a9a9a9a921212121
            // cbcbcbcb43434343cbcbcbcb43434343
            // edededed65656565edededed65656565
            // -f-f-f-f878787870f0f0f0f87878787

            return new CubeXLineBitwise
                (_98109810, _a921a921, _ba32ba32, _cb43cb43, _dc54dc54, _ed65ed65, _fe76fe76, __f870f87);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint4 bitwiseLastHalfCubeXLine_(uint4 y0z0r, uint4 y0z1r, uint4 y1z0r, uint4 y1z1r)
        {
            return (y0z0r & 1) << 25 | (y0z1r & 1) << 27 | (y1z0r & 1) << 29 | (y1z1r & 1) << 31;
        }


        // ------------------------------------------------------------------





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void storeCubeInstances
            (uint* pOutput, ref int outputCounter, uint4 cube4x4, int gridid, int ix, int iy, int iz)
        {
            StoreCubeInstances(pOutput, ref outputCounter, cube4x4, gridid, ix, iy, iz + new int4(0, 1, 2, 3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe void StoreCubeInstances
            (uint* pOutput, ref int outputCounter, uint4 cube4x4, int gridid, int ix, int iy, int4 iz4)
        {
            //if (!math.any(cube4x4)) return;
            if (!math.any(cube4x4) | !math.any(~cube4x4)) return;

            var c0 = cube4x4 & 0xff;
            var c1 = (cube4x4 >> 8) & 0xff;
            var c2 = (cube4x4 >> 16) & 0xff;
            var c3 = (cube4x4 >> 24) & 0xff;

            var ix4 = new int4(ix, ix, ix, ix);
            var iy4 = new int4(iy, iy, iy, iy);
            var ci0 = CubeUtility.ToCubeInstance(ix4 + 0, iy4, iz4, gridid, c0);
            var ci1 = CubeUtility.ToCubeInstance(ix4 + 8, iy4, iz4, gridid, c1);
            var ci2 = CubeUtility.ToCubeInstance(ix4 + 16, iy4, iz4, gridid, c2);
            var ci3 = CubeUtility.ToCubeInstance(ix4 + 24, iy4, iz4, gridid, c3);

            store_(pOutput, ci0, ref outputCounter, c0 != 0 & c0 != 0xff);
            store_(pOutput, ci1, ref outputCounter, c1 != 0 & c1 != 0xff);
            store_(pOutput, ci2, ref outputCounter, c2 != 0 & c2 != 0xff);
            store_(pOutput, ci3, ref outputCounter, c3 != 0 & c3 != 0xff);

            return;


            static void store_(uint* pOutput, uint4 ci, ref int idx, bool4 isOutput)
            {
                idx = math.compress(pOutput, idx, ci, isOutput);

                // 遅い、倍以上
                //switch (math.bitmask(isOutput))
                //{
                //    case 0b_0000: break;

                //    case 0b_0001: *((uint*)(pOutput + idx++)) = ci.x; break;
                //    case 0b_0010: *((uint*)(pOutput + idx++)) = ci.y; break;
                //    case 0b_0100: *((uint*)(pOutput + idx++)) = ci.z; break;
                //    case 0b_1000: *((uint*)(pOutput + idx++)) = ci.w; break;

                //    case 0b_0011: *((uint2*)(pOutput + idx)) = ci.xy; idx += 2; break;
                //    case 0b_0101: *((uint2*)(pOutput + idx)) = ci.xz; idx += 2; break;
                //    case 0b_1001: *((uint2*)(pOutput + idx)) = ci.xw; idx += 2; break;
                //    case 0b_0110: *((uint2*)(pOutput + idx)) = ci.yz; idx += 2; break;
                //    case 0b_1010: *((uint2*)(pOutput + idx)) = ci.yw; idx += 2; break;
                //    case 0b_1100: *((uint2*)(pOutput + idx)) = ci.zw; idx += 2; break;

                //    case 0b_1110: *((uint3*)(pOutput + idx)) = ci.yzw; idx += 3; break;
                //    case 0b_0111: *((uint3*)(pOutput + idx)) = ci.xyz; idx += 3; break;
                //    case 0b_1101: *((uint3*)(pOutput + idx)) = ci.xzw; idx += 3; break;
                //    case 0b_1011: *((uint3*)(pOutput + idx)) = ci.xyw; idx += 3; break;

                //    case 0b_1111: *((uint4*)(pOutput + idx)) = ci; idx += 4; break;
                //};
            }
        }
    }
}