using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace DotsLite.MarchingCubes
{

    static public class ComputeShaderUtility
    {
        //static public ComputeBuffer CreateIndirectArgumentsBufferForInstancing() =>
        //    new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable);

        //static public ComputeBuffer CreateIndirectArgumentsBufferForDispatch() =>
        //    new ComputeBuffer(1, sizeof(int) * 3, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable);

        static public void SetConstantBuffer_(this Material mat, string name, ComputeBuffer buffer) =>
            mat.SetConstantBuffer(name, buffer, 0, buffer.stride * buffer.count);
        //static public void SetConstantBuffer(this Material mat, string name, ComputeBuffer buffer) =>
        //    mat.SetConstantBuffer(name, buffer, 0, buffer.stride * buffer.count);
        //static public void SetConstantBuffer(this Material mat, string name, ComputeBuffer buffer)
        //{
        //    //Debug.Log($"{buffer.stride}");
        //    var arr = new Vector4[buffer.stride / Marshal.SizeOf<Vector4>() * buffer.count];
        //    buffer.GetData(arr);
        //    mat.SetVectorArray(name, arr);
        //}
    }


    public static class DrawResourceExtension_
    {

        public static void SetResourcesTo(this GlobalResources res, Material mat, ComputeShader cs)
        {
            // ‚¹‚Â‚ß‚¢ - - - - - - - - - - - - - - - - -

            //uint4 cube_patterns[ 254 ][2];
            // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
            // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

            //uint4 cube_vtxs[ 12 ];
            // x: near vertex index (x>>0 | y>>8 | z>>16)
            // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
            // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
            // w: pos(x>>0 | y>>8 | z>>16)

            //uint3 grids[ 512 ][2];
            // [0] : position as float3
            // [1] : near grid id
            // { x: prev(left>>0 | up>>9 | front>>18)  y: next(right>>0 | down>>9 | back>>18)  z: current }

            // - - - - - - - - - - - - - - - - - - - - -

            mat.SetConstantBuffer_("static_data", res.CubeGeometryConstants.Buffer);

            mat.SetTexture("grid_cubeids", res.GridCubeIds.Texture);
            //mat.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );

            cs?.SetTexture(0, "dst_grid_cubeids", res.GridCubeIds.Texture);
        }


        public static void SetResourcesTo(this DotGridAreaResourcesForGpu res, Material mat, ComputeShader cs)
        {
            mat.SetBuffer("cube_instances", res.CubeInstances.Buffer);

            mat.SetConstantBuffer_("grid_constant", res.GridInstances.Buffer);
            //mat.SetConstantBuffer("grids", res.GridInstancesBuffer);

            cs?.SetBuffer(0, "src_instances", res.CubeInstances.Buffer);
        }

    }


}

