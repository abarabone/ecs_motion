﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes
{

    using DotsLite.Draw;
    using DotsLite.Utilities;


    public struct DotGridAreaResourcesForBurst// : IDisposable
    {
        //public CubeInstancingIndirectArgumentsBuffer ArgsBufferForInstancing;
        //public GridToCubesDispatchIndirectArgumentsBuffer ArgsBufferForDispatch;

        //public CubeInstancingShaderBuffer CubeInstances;
        //public GridInstancesBuffer GridInstances;


        //public DotGridAreaResourcesForBurst(int maxCubeInstances, int maxGridInstances) : this()
        //{
        //    this.ArgsBufferForInstancing = CubeInstancingIndirectArgumentsBuffer.Create();
        //    this.ArgsBufferForDispatch = GridToCubesDispatchIndirectArgumentsBuffer.Create();

        //    this.CubeInstances = CubeInstancingShaderBuffer.Create(maxCubeInstances);// 32 * 32 * 32 * maxGridLength);
        //    this.GridInstances = GridInstancesBuffer.Create(maxGridInstances);// 512);
        //}

        //public void Dispose()
        //{
        //    this.ArgsBufferForInstancing.Dispose();
        //    this.ArgsBufferForDispatch.Dispose();

        //    this.CubeInstances.Dispose();
        //    this.GridInstances.Dispose();
        //}

        //public void SetResourcesTo(Material mat, ComputeShader cs)
        //{
        //    mat.SetBuffer("cube_instances", this.CubeInstances.Buffer);

        //    mat.SetConstantBuffer_("grid_constant", this.GridInstances.Buffer);
        //    //mat.SetConstantBuffer("grids", res.GridInstancesBuffer);

        //    cs?.SetBuffer(0, "src_instances", this.CubeInstances.Buffer);
        //}
    }


    //public struct CubeInstancingIndirectArgumentsBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static CubeInstancingIndirectArgumentsBuffer Create() => new CubeInstancingIndirectArgumentsBuffer
    //    {
    //        Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}


    //public struct GridToCubesDispatchIndirectArgumentsBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static GridToCubesDispatchIndirectArgumentsBuffer Create() => new GridToCubesDispatchIndirectArgumentsBuffer
    //    {
    //        Buffer = new ComputeBuffer(1, sizeof(int) * 3, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}


    //public struct CubeInstancingShaderBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static CubeInstancingShaderBuffer Create(int maxCubeInstances) => new CubeInstancingShaderBuffer
    //    {
    //        Buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>(), ComputeBufferType.Append),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}


    //public struct GridInstancesBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static GridInstancesBuffer Create(int maxGridLength) => new GridInstancesBuffer
    //    {
    //        Buffer = new ComputeBuffer(maxGridLength, Marshal.SizeOf<float4>() * 2, ComputeBufferType.Constant),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}

}
