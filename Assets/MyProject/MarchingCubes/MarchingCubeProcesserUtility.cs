using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace Abarabone.MarchingCubes
{
    using sh = math.ShuffleComponent;

    static class ProcesserUtility
    {





        // ------------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SampleAlternateCubes<TCubeInstanceWriter>
            (ref this DotGrid32x32x32Unsafe grid, int gridId, uint4 *pDst, uint *pOutput)
        {
            var gptr = (uint4*)grid.pUnits;
            var i = 0;

            for (var iy = 0; iy < 31; iy += 1*2)
            {
                for (var iz = 0; iz < 31; iz += 4*2)
                {
                    var iy4 = new int4(iy, iy, iy, iy);
                    var iy4_ = iy4 + new int4(0, 0, 1, 1);
                    var iz4 = new int4(iz, iz, iz, iz);
                    var iz4_ = iz4 + new int4(0, 4, 0, 4);
                    var i4 = iy4_ * 32 + iz4_;

                    var y0z0123 = gptr[i4.x];
                    var y0z4567 = gptr[i4.y];
                    var y1z0123 = gptr[i4.z];
                    var y1z4567 = gptr[i4.w];
                    var y0z0 = math.shuffle(y0z0123, y0z4567, sh.LeftX, sh.LeftZ, sh.RightX, sh.RightZ);
                    var y0z1 = math.shuffle(y0z0123, y0z4567, sh.LeftY, sh.LeftW, sh.RightY, sh.RightW);
                    var y1z0 = math.shuffle(y1z0123, y1z4567, sh.LeftX, sh.LeftZ, sh.RightX, sh.RightZ);
                    var y1z1 = math.shuffle(y1z0123, y1z4567, sh.LeftY, sh.LeftW, sh.RightY, sh.RightW);

                    var cubes = bitwiseCubesBase_(y0z0, y0z1, y1z0, y1z1);

                    // x:(_98109810,iy,iz+0), y:(_98109810,iy,iz+2), z:(_98109810,iy,iz+4), w:(_98109810,iy,iz+6)
                    // x:(_ba32ba32,iy,iz+0), y:(_ba32ba32,iy,iz+2), z:(_ba32ba32,iy,iz+4), w:(_ba32ba32,iy,iz+6)
                    // x:(_dc54dc54,iy,iz+0), y:(_dc54dc54,iy,iz+2), z:(_dc54dc54,iy,iz+4), w:(_dc54dc54,iy,iz+6)
                    // x:(_fe76fe76,iy,iz+0), y:(_fe76fe76,iy,iz+2), z:(_fe76fe76,iy,iz+4), w:(_fe76fe76,iy,iz+6)
                    pDst[i++] = cubes._98109810;
                    pDst[i++] = cubes._ba32ba32;
                    pDst[i++] = cubes._dc54dc54;
                    pDst[i++] = cubes._fe76fe76;

                    var gix = int4.zero;
                    var giz = iz + new int4(0, 2, 4, 6);
                    var giy = iy + int4.zero;
                    storeCubeInstances(pOutput, cubes._98109810, gridId, gix + 0, giy, giz);
                    storeCubeInstances(pOutput, cubes._ba32ba32, gridId, gix + 2, giy, giz);
                    storeCubeInstances(pOutput, cubes._dc54dc54, gridId, gix + 4, giy, giz);
                    storeCubeInstances(pOutput, cubes._fe76fe76, gridId, gix + 6, giy, giz);
                }
            }
        }
        static unsafe void storeCubeInstances(uint *pDst, uint4 cube4x4, int gridid, int4 gix, int4 giy, int4 giz)
        {
            var c0 = cube4x4 & 0xff;
            var c1 = cube4x4 >> 8 & 0xff;
            var c2 = cube4x4 >> 16 & 0xff;
            var c3 = cube4x4 >> 24 & 0xff;

            var ci0 = CubeUtility.ToCubeInstance(gix + 0, giy, giz, gridid, c0);
            var ci1 = CubeUtility.ToCubeInstance(gix + 8, giy, giz, gridid, c1);
            var ci2 = CubeUtility.ToCubeInstance(gix + 16, giy, giz, gridid, c2);
            var ci3 = CubeUtility.ToCubeInstance(gix + 24, giy, giz, gridid, c3);

            store_(ci0);
            store_(ci1);
            store_(ci2);
            store_(ci3);

            return;


            void store_(uint4 ci)
            {
                switch(math.bitmask(ci != 0))
                {
                    case 0b_0000: break;

                    case 0b_0001: *((uint*)pDst) = ci.x; break;
                    case 0b_0010: *((uint*)pDst) = ci.y; break;
                    case 0b_0100: *((uint*)pDst) = ci.z; break;
                    case 0b_1000: *((uint*)pDst) = ci.w; break;

                    case 0b_0011: *((uint2*)pDst) = ci.xy; break;
                    case 0b_0101: *((uint2*)pDst) = ci.xz; break;
                    case 0b_1001: *((uint2*)pDst) = ci.xw; break;
                    case 0b_0110: *((uint2*)pDst) = ci.yz; break;
                    case 0b_1010: *((uint2*)pDst) = ci.yw; break;
                    case 0b_1100: *((uint2*)pDst) = ci.zw; break;

                    case 0b_1110: *((uint3*)pDst) = ci.yzw; break;
                    case 0b_0111: *((uint3*)pDst) = ci.xyz; break;
                    case 0b_1101: *((uint3*)pDst) = ci.xzw; break;
                    case 0b_1011: *((uint3*)pDst) = ci.xyw; break;

                    case 0b_1111: *((uint4*)pDst) = ci; break;
                };
            }
        }
        static public void MakeAlterCube()
        {

        }
        static unsafe void makeAlterX(uint4 *pSrc, uint4 *pDst)
        {
            for(var i4 = 0; i4 < 16 * 16; i4 += 4)
            {
                var x_8080 = pSrc[i4 + 0];
                var x_a2a2 = pSrc[i4 + 2];
                var x_c4c4 = pSrc[i4 + 1];
                var x_e6e6 = pSrc[i4 + 3];
                var x_8080_= new uint4(0,0,0,0);

                var x9191 = bitwise_x_(x_a2a2, x_8080);
                var xb3b3 = bitwise_x_(x_c4c4, x_a2a2);
                var xd5d5 = bitwise_x_(x_e6e6, x_c4c4);
                var xf7f7 = bitwise_x_(rot_(x_8080, x_8080_), x_e6e6);
                // a9a9a9a921212121a9a9a9a921212121
                // cbcbcbcb43434343cbcbcbcb43434343
                // edededed65656565edededed65656565
                // -f-f-f-f878787870f0f0f0f87878787

                pDst[i4 + 0] = x9191;
                pDst[i4 + 2] = xb3b3;
                pDst[i4 + 1] = xd5d5;
                pDst[i4 + 3] = xf7f7;

                static uint4 bitwise_x_(uint4 v0, uint4 v1) => (v0 & 0x_5555_5555u) << 1 | (v1 & 0x_aaaa_aaaau) >> 1;
                static uint4 rot_(uint4 l, uint4 r) => l >> 8 & 0x_00ff_ffffu | r << (32 - 8) & 0x_ff00_0000u;
            }
        }
        static unsafe void makeAlterZ(uint4* pSrc, uint4* pDst)
        {
            for(var zofs = 0; zofs < 16 * 16; zofs += 16)
            {
                for(var i4 = 0; i4 < 4; i4++)
                {
                    var z0_0246 = pSrc[i4 + 0 + zofs];
                    var z0_8ace = pSrc[i4 + 4 + zofs];
                    var z1_0246 = pSrc[i4 + 8 + zofs];
                    var z1_8ace = pSrc[i4 + 12+ zofs];
                    var z0_0_ = 0u;

                    var z0_1357 = bitwise_z_(z0_0246, z0_8ace);
                    var z0_9bdf = bitwise_z_(z0_8ace, z1_0246);
                    var z1_1357 = bitwise_z_(z1_0246, z1_8ace);
                    var z1_9bdf = bitwise_z1_(z1_8ace, z0_0_);

                    pSrc[i4 + 0 + zofs] = z0_1357;
                    pSrc[i4 + 4 + zofs] = z0_9bdf;
                    pSrc[i4 + 8 + zofs] = z1_1357;
                    pSrc[i4 + 12 + zofs] = z1_9bdf;

                    static uint4 bitwise_z_(uint4 v0, uint4 v1) => (v0 & 0x_3333_3333u) << 2 | (shz_(v0, v1.x) & 0x_cccc_ccccu) >> 2;
                    static uint4 bitwise_z1_(uint4 v0, uint x1) => (v0 & 0x_3333_3333u) << 2 | (shz_(v0, x1) & 0x_cccc_ccccu) >> 2;
                    static uint4 shz_(uint4 cur, uint next)
                    {
                        var tmp = new uint4(cur);
                        tmp.x = next;
                        return tmp.yzwx;
                    }
                }
            }
        }
        static unsafe void makeAlterY(uint4* pSrc, uint4* pDst)
        {
            var i4 = 0;
            for (; i4 < 16 * 16 - 16; i4++)
            {
                var y0 = pSrc[i4 + 0];
                var y2 = pSrc[i4 + 16];

                var y1 = bitwise_y_(y0, y2);

                pDst[i4 + 0] = y1;
            }
            for (; i4 < 16 * 16; i4++)
            {
                var y0 = pSrc[i4 + 0];
                var y2 = new uint4(0, 0, 0, 0);

                var y1 = bitwise_y_(y0, y2);

                pDst[i4 + 0] = y1;
            }
            static uint4 bitwise_y_(uint4 v0, uint4 v1) => (v0 & 0x_0f0f_0f0fu) << 4 | (v1 & 0x_f0f0_f0f0u) >> 4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CubeXLineBitwise// タプルだと burst 利かないので
        {
            public uint4 _98109810, _ba32ba32, _dc54dc54, _fe76fe76;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CubeXLineBitwise
            (
                uint4 _98109810, uint4 _ba32ba32, uint4 _dc54dc54, uint4 _fe76fe76
            )
            {
                this._98109810 = _98109810;
                this._ba32ba32 = _ba32ba32;
                this._dc54dc54 = _dc54dc54;
                this._fe76fe76 = _fe76fe76;
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
        static CubeXLineBitwise bitwiseCubesBase_(uint4 y0z0, uint4 y0z1, uint4 y1z0, uint4 y1z1)
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

            return new CubeXLineBitwise
                (_98109810, _ba32ba32, _dc54dc54, _fe76fe76);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint4 bitwiseLastHalfCubeXLine_(uint4 y0z0r, uint4 y0z1r, uint4 y1z0r, uint4 y1z1r)
        {
            return (y0z0r & 1) << 25 | (y0z1r & 1) << 27 | (y1z0r & 1) << 29 | (y1z1r & 1) << 31;
        }


        // ------------------------------------------------------------------

    }
}