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
            (ref this DotGrid32x32x32Unsafe grid, int gridId, ref UnsafeList<uint4> outputCubeBits)
        {
            var gptr = (uint4*)grid.pUnits;

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
                    outputCubeBits.AddNoResize(cubes._98109810);
                    outputCubeBits.AddNoResize(cubes._ba32ba32);
                    outputCubeBits.AddNoResize(cubes._dc54dc54);
                    outputCubeBits.AddNoResize(cubes._fe76fe76);
                }
            }
        }
        //static public void add_(int gridid, int ix, int iy, int iz, ref UnsafeList<byte> output)
        //{
        //    output.Ptr
        //    var ix4 = ix + new int4(0 + 0, 0 + 8, 16 + 0, 16 + 8);
        //    var iy4 = new int4(iy, iy, iy, iy);
        //    var iz4 = new int4(iz, iz, iz, iz) + new int4(0, 2, 4, 6);
        //    outputCubes.AddNoResize(CubeUtility.ToCubeInstance(ix4, iy4, iz4, gridId, cubes._98109810));
        //}
        static public void makeAlterX_(ref UnsafeList<uint4> cubeBitwises)
        {
            var i = 0;
            for (var iy = 0; iy < 16; iy++)
            {
                for (var iz = 0; iz < 4; iz++)
                {
                    var _98109810 = cubeBitwises[i++];
                    var _ba32ba32 = cubeBitwises[i++];
                    var _dc54dc54 = cubeBitwises[i++];
                    var _fe76fe76 = cubeBitwises[i++];

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

                    //var y0z1 = ;
                    //var y0z2 = ;
                    //var y1z1 = ;
                    //var y1z2 = ;
                }
            }
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