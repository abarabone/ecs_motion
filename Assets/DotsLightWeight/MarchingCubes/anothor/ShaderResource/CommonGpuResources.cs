//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System;
//using UnityEngine.Rendering;
//using System.Runtime.InteropServices;
//using System.Runtime.CompilerServices;
//using System.IO;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.MarchingCubes.another
//{

//    using DotsLite.Draw;
//    using DotsLite.Utilities;


//    public static partial class Global
//    {

//        public struct CommonShaderResources : IDisposable
//        {
//            public CubeGeometryConstantBuffer CubeGeometryConstants;


//            public void Alloc(MarchingCubesAsset asset)
//            {
//                Debug.Log($"CommonShaderResources alloc");
//                this.CubeGeometryConstants = CubeGeometryConstantBuffer.Create(asset);
//            }

//            public void Dispose()
//            {
//                Debug.Log($"CommonShaderResources disposed");
//                this.CubeGeometryConstants.Dispose();
//            }

//            public void SetResourcesTo(Material mat)
//            {
//                // せつめい - - - - - - - - - - - - - - - - -

//                //uint4 cube_patterns[ 254 ][2];
//                // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
//                // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

//                //uint4 cube_vtxs[ 12 ];
//                // x: near vertex index (x>>0 | y>>8 | z>>16)
//                // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
//                // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
//                // w: pos(x>>0 | y>>8 | z>>16)

//                // - - - - - - - - - - - - - - - - - - - - -

//                mat.SetConstantBuffer_("static_data", this.CubeGeometryConstants.Buffer);
//            }
//        }


//        public struct WorkingShaderResources : IDisposable
//        {
//            public GridCubeIdShaderBufferTexture GridCubeIds;


//            public void Alloc(int maxGridInstances, int unitOnEdge)
//            {
//                Debug.Log($"WorkingShaderResources alloc");
//                this.GridCubeIds = GridCubeIdShaderBufferTexture.Create(maxGridInstances, unitOnEdge);
//            }

//            public void Dispose()
//            {
//                Debug.Log($"WorkingShaderResources disposed");
//                this.GridCubeIds.Dispose();
//            }

//            public void SetResourcesTo(Material mat, ComputeShader cs)
//            {
//                mat.SetTexture("grid_cubeids", this.GridCubeIds.Texture);

//                cs?.SetTexture(0, "dst_grid_cubeids", this.GridCubeIds.Texture);
//            }
//        }
//    }




//}

