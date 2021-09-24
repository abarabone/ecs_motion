using System.Collections;
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


    public struct DotGridAreaGpuResources : IDisposable
    {
        public GridInstructionsBuffer GridInstructions;
        //public GridToCubesDispatchIndirectArgumentsBuffer GridToCubesDispatchArgs;

        public CubeInstancingShaderBuffer CubeInstances;
        public CubeInstancingIndirectArgumentsBuffer CubeInstancingArgs;


        public void Alloc(int maxCubeInstances, int maxGridInstructions)
        {
            this.GridInstructions = GridInstructionsBuffer.Create(maxGridInstructions);
            //this.GridToCubesDispatchArgs = GridToCubesDispatchIndirectArgumentsBuffer.Create();

            this.CubeInstances = CubeInstancingShaderBuffer.Create(maxCubeInstances);
            this.CubeInstancingArgs = CubeInstancingIndirectArgumentsBuffer.Create();
        }

        public void Dispose()
        {
            this.GridInstructions.Dispose();
            //this.GridToCubesDispatchArgs.Dispose();

            this.CubeInstances.Dispose();
            this.CubeInstancingArgs.Dispose();
        }

        public void SetResourcesTo(Material mat, ComputeShader cs)
        {
            mat.SetBuffer("cube_instances", this.CubeInstances.Buffer);

            mat.SetConstantBuffer_("grid_constant", this.GridInstructions.Buffer);

            cs?.SetBuffer(0, "cube_instances", this.CubeInstances.Buffer);
        }

        public void SetArgumentBuffer(Mesh mesh)
        {
            var iargparams = new IndirectArgumentsForInstancing(mesh, 1);
            this.CubeInstancingArgs.Buffer.SetData(ref iargparams);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GridInstraction
    {
        public Vector3 position;
        public int GridDynamicIndex;
        public NearGridIndex GridStaticIndex;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct NearGridIndex
    {
        public int left_home;
        public int left_down;
        public int left_rear;
        public int left_slant;
        public int right_home;
        public int right_down;
        public int right_rear;
        public int right_slant;
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


    public struct CubeInstancingIndirectArgumentsBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static CubeInstancingIndirectArgumentsBuffer Create() => new CubeInstancingIndirectArgumentsBuffer
        {
            Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
        };

        public void Dispose() => this.Buffer?.Release();
    }


    //public struct GridToCubesDispatchIndirectArgumentsBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static GridToCubesDispatchIndirectArgumentsBuffer Create() => new GridToCubesDispatchIndirectArgumentsBuffer
    //    {
    //        Buffer = new ComputeBuffer(1, sizeof(int) * 3, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}


    public struct CubeInstancingShaderBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static CubeInstancingShaderBuffer Create(int maxCubeInstances) => new CubeInstancingShaderBuffer
        {
            Buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>(), ComputeBufferType.Append),
        };

        public void Dispose() => this.Buffer?.Release();
    }


    public struct GridInstructionsBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static GridInstructionsBuffer Create(int maxGridLength) => new GridInstructionsBuffer
        {
            Buffer = new ComputeBuffer(maxGridLength, Marshal.SizeOf<float4>() * 2, ComputeBufferType.Constant),
        };

        public void Dispose() => this.Buffer?.Release();
    }

}

