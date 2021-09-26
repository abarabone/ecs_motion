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
        public GridContentDataBuffer GridContentDataBuffer;
        public GridInstructionsBuffer GridInstructions;

        public CubeInstancingShaderBuffer CubeInstances;
        public CubeInstancingIndirectArgumentsBuffer CubeInstancingArgs;


        public void Alloc(int maxCubeInstances, int maxGrids, int maxGridInstructions = 63)// cs の dispatch は 65535 までなので、65535/1024
        {
            this.GridContentDataBuffer = GridContentDataBuffer.Create(maxGrids);
            this.GridInstructions = GridInstructionsBuffer.Create(maxGridInstructions);

            this.CubeInstances = CubeInstancingShaderBuffer.Create(maxCubeInstances);
            this.CubeInstancingArgs = CubeInstancingIndirectArgumentsBuffer.Create();

            var q =
                from i in Enumerable.Range(0, 32 * 32)
                select (i / 32) == 0 ? 0x55555555u : 0xaaaaaaaau
                //select (i & 1) == 0 ? 0x55555555u : 0xaaaaaaaau
                ;
            this.GridContentDataBuffer.Buffer.SetData(q.ToArray());
        }

        public void Dispose()
        {
            this.GridContentDataBuffer.Dispose();
            this.GridInstructions.Dispose();

            this.CubeInstances.Dispose();
            this.CubeInstancingArgs.Dispose();
        }

        public void SetResourcesTo(Material mat, ComputeShader cs)
        {
            cs?.SetBuffer(0, "dotgrids", this.GridContentDataBuffer.Buffer);
            cs?.SetBuffer(0, "cube_instances", this.CubeInstances.Buffer);

            mat.SetBuffer("cube_instances", this.CubeInstances.Buffer);
            mat.SetConstantBuffer_("grid_constant", this.GridInstructions.Buffer);
        }

        public void SetArgumentBuffer(Mesh mesh)
        {
            var iargparams = new IndirectArgumentsForInstancing(mesh, 1);// 1 はダミー、0 だと怒られる
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


    public struct CubeInstancingIndirectArgumentsBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static CubeInstancingIndirectArgumentsBuffer Create() => new CubeInstancingIndirectArgumentsBuffer
        {
            Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
        };

        public void Dispose() => this.Buffer?.Release();
    }


    public struct GridContentDataBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static GridContentDataBuffer Create(int maxGrids) => new GridContentDataBuffer
        {
            Buffer = new ComputeBuffer(32 * 32 * maxGrids, Marshal.SizeOf<uint>()),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    public struct GridInstructionsBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        // cs の dispatch は 65535 までなので、65535/1024
        public static GridInstructionsBuffer Create(int maxGridInstructions = 63) => new GridInstructionsBuffer
        {
            Buffer = new ComputeBuffer(maxGridInstructions, Marshal.SizeOf<float4>() * 2, ComputeBufferType.Constant),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    public struct CubeInstancingShaderBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static CubeInstancingShaderBuffer Create(int maxCubeInstances) => new CubeInstancingShaderBuffer
        {
            Buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>(), ComputeBufferType.Append),
        };

        public void Dispose() => this.Buffer?.Release();
    }


}

