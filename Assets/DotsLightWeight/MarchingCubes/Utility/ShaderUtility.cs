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

}

