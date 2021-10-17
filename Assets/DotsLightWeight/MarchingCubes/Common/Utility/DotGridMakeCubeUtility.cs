using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using Unity.Entities;

namespace DotsLite.MarchingCubes
{


    [StructLayout(LayoutKind.Sequential)]
    public struct CubeInstance
    {
        public uint instance;
        static public implicit operator CubeInstance(uint cubeInstance) => new CubeInstance { instance = cubeInstance };
    }


    public unsafe struct DotGrid32x32x32UnsafePtr
    {
        [NativeDisableUnsafePtrRestriction]
        public DotGrid32x32x32Unsafe* p;

        public uint this[int ix, int iy, int iz]
        {
            get => (*this.p)[ix, iy, iz];
            //set => (*this.p)[ix, iy, iz] = value;
            set
            {
                var grid = *this.p;
                grid[ix, iy, iz] = value;
                *this.p = grid;
            }
        }
    }





    public static unsafe partial class MakeCube
    {

        public struct NearDotGrids
        {
            public HalfGridUnit L;
            public HalfGridUnit R;
            public struct HalfGridUnit
            {
                public uint* x;
                public uint* y;
                public uint* z;
                public uint* w;
            }
        }

        public interface ICubeInstanceWriter
        {
            void Add(int x, int y, int z, uint cubeid);
        }
        public struct MeshWriter : ICubeInstanceWriter, IDisposable
        {
            public float3 gridpos;
            public NativeList<float3> vtxs;
            public NativeList<int3> tris;
            public CollisionFilter filter;

            public BlobAssetReference<MarchingCubesBlobAsset> mcdata;

            public void Add(int x, int y, int z, uint cubeid)
            {

            }
            public BlobAssetReference<Collider> CreateMesh() =>
                MeshCollider.Create(this.vtxs, this.tris, this.filter);
            public void Dispose()
            {
                this.vtxs.Dispose();
                this.tris.Dispose();
            }
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void SampleAllCubes<TCubeInstanceWriter>
        //    (ref NearDotGrids g, int gridId, ref TCubeInstanceWriter outputCubes)
        //    where TCubeInstanceWriter : ICubeInstanceWriter
        //{

        //    var gcount = getEachCount(ref g);

        //    SampleAllCubes(ref g, ref gcount, gridId, ref outputCubes);

        //}


        // ------------------------------------------------------------------

        /// <summary>
        /// native contener 化必要、とりあえずは配列で動作チェック
        /// あとでＹＺカリングもしたい
        /// </summary>
        // xyz各32個目のキューブは1bitのために隣のグリッドを見なくてはならず、効率悪いしコードも汚くなる、なんとかならんか？
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SampleAllCubes<TCubeInstanceWriter>(in NearDotGrids g, ref TCubeInstanceWriter outputCubes)
            where TCubeInstanceWriter : ICubeInstanceWriter
        {
            UnityEngine.Debug.Log($"{g.L.x != null} {g.L.y != null} {g.L.z != null} {g.L.w != null}");
            UnityEngine.Debug.Log($"{g.R.x != null} {g.R.y != null} {g.R.z != null} {g.R.w != null}");
            return;
            for (var iy = 0; iy < 31; iy++)
            {
                for (var iz = 0; iz < (31 & ~0x3); iz += 4)
                {
                    var c = getXLine_(iy, iz, g.L.x, g.L.x, g.L.x, g.L.x);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g.R.x, g.R.x, g.R.x, g.R.x);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    addCubeFromXLine_(ref cubes, iy, iz, ref outputCubes);
                }
                //if ()
                {
                    const int iz = 31 & ~0x3;

                    var c = getXLine_(iy, iz, g.L.x, g.L.x, g.L.z, g.L.z);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g.R.x, g.R.x, g.R.z, g.R.z);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    addCubeFromXLine_(ref cubes, iy, iz, ref outputCubes);
                }
            }
            //if ()
            {
                const int iy = 31;
                for (var iz = 0; iz < (31 & ~0x3); iz += 4)
                {
                    var c = getXLine_(iy, iz, g.L.x, g.L.y, g.L.x, g.L.y);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g.R.x, g.R.y, g.R.x, g.R.y);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    addCubeFromXLine_(ref cubes, iy, iz, ref outputCubes);
                }
                {
                    const int iz = 31 & ~0x3;

                    var c = getXLine_(iy, iz, g.L.x, g.L.y, g.L.z, g.L.w);
                    var cubes = bitwiseCubesXLine_(c.y0z0, c.y0z1, c.y1z0, c.y1z1);

                    var cr = getXLine_(iy, iz, g.R.x, g.R.y, g.R.z, g.R.w);
                    cubes._0f870f87 |= bitwiseLastHalfCubeXLine_(cr.y0z0, cr.y0z1, cr.y1z0, cr.y1z1);

                    addCubeFromXLine_(ref cubes, iy, iz, ref outputCubes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isBlankZ_(NearDotGrids g) =>
            g.L.z == null ||
            g.R.x == null || g.R.y == null || g.R.z == null || g.R.w == null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool isBlankY_(NearDotGrids g) =>
            g.L.y == null || g.L.z == null || g.L.w == null ||
            g.R.x == null || g.R.y == null || g.R.z == null || g.R.w == null;


        public struct CubeXLineBitwise// タプルだと burst 利かないので
        {
            public uint4 _98109810, _a921a921, _ba32ba32, _cb43cb43, _dc54dc54, _ed65ed65, _fe76fe76, _0f870f87;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CubeXLineBitwise(
                uint4 _98109810, uint4 _a921a921, uint4 _ba32ba32, uint4 _cb43cb43,
                uint4 _dc54dc54, uint4 _ed65ed65, uint4 _fe76fe76, uint4 _0f870f87)
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
        static unsafe CubeNearXLines getXLine_(int iy, int iz, uint* pgx, uint* pgy, uint* pgz, uint* pgw)
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
            var i = _i;
            var y0 = ((uint4*)pgx)[i.x];
            var y1 = ((uint4*)pgy)[i.y];
            var y0z0 = y0;
            var y1z0 = y1;

            y0.x = pgz[i.z];
            y1.x = pgw[i.w];
            var y0z1 = y0.yzwx;
            var y1z1 = y1.yzwx;

            return new CubeNearXLines(y0z0, y0z1, y1z0, y1z1);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void addCubeFromXLine_<TCubeInstanceWriter>(
            ref CubeXLineBitwise cubes,
            int iy, int iz, ref TCubeInstanceWriter outputCubes_
        )
            where TCubeInstanceWriter : ICubeInstanceWriter
        {
            var i = 0;
            var ix = 0;
            var iz_ = new int4(iz + 0, iz + 1, iz + 2, iz + 3);
            for (var ipack = 0; ipack < 32 / 8; ipack++)// 8 は 1cube の 8bit
            {// レジスタ１つずつ見ていったほうがいいのでは
                addCubeIfVisible_(cubes._98109810 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._a921a921 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._ba32ba32 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._cb43cb43 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._dc54dc54 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._ed65ed65 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._fe76fe76 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                addCubeIfVisible_(cubes._0f870f87 >> i & 0xff, ix++, iy, iz_, ref outputCubes_);
                i += 8;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void addCubeIfVisible_<TCubeInstanceWriter>
            (uint4 cubeId, int4 ix, int4 iy, int4 iz, ref TCubeInstanceWriter outputCubes)
            where TCubeInstanceWriter : ICubeInstanceWriter
        {
            var _0or255to0 = cubeId + 1 & 0xfe;
            if (!math.any(_0or255to0)) return;// すべての cubeId が 0 か 255 なら何もしない

            if (_0or255to0.x != 0) outputCubes.Add(ix.x, iy.x, iz.x, cubeId.x);
            if (_0or255to0.y != 0) outputCubes.Add(ix.y, iy.y, iz.y, cubeId.y);
            if (_0or255to0.z != 0) outputCubes.Add(ix.z, iy.z, iz.z, cubeId.z);
            if (_0or255to0.w != 0) outputCubes.Add(ix.w, iy.w, iz.w, cubeId.w);
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




    }
}
