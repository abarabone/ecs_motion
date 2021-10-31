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
        public GridContentDataBuffer GridDotContentDataBuffer;
        //public GridInstructionsBuffer GridInstructions;

        public CubeInstancingShaderBuffer CubeInstances;
        //public CubeInstancingIndirectArgumentsBuffer CubeInstancingArgs;


        public void Alloc(int maxCubeInstances, int maxGrids, int maxGridInstructions = 63)// cs の dispatch は 65535 までなので、65535/1024
        {
            this.GridDotContentDataBuffer = GridContentDataBuffer.Create(maxGrids);
            //this.GridInstructions = GridInstructionsBuffer.Create(maxGridInstructions);

            this.CubeInstances = CubeInstancingShaderBuffer.Create(maxCubeInstances);
            //this.CubeInstancingArgs = CubeInstancingIndirectArgumentsBuffer.Create();
        }

        public void Dispose()
        {
            this.GridDotContentDataBuffer.Dispose();
            //this.GridInstructions.Dispose();

            this.CubeInstances.Dispose();
            //this.CubeInstancingArgs.Dispose();
        }

        public void SetResourcesTo(Material mat, ComputeShader cs)
        {
            cs?.SetBuffer(0, "dotgrids", this.GridDotContentDataBuffer.Buffer);
            cs?.SetBuffer(0, "cube_instances", this.CubeInstances.Buffer);
            //cs?.SetBuffer(0, "grid_instructions", this.GridInstructions.Buffer);

            mat.SetBuffer("cube_instances", this.CubeInstances.Buffer);
            //mat.SetBuffer("grid_instructions", this.GridInstructions.Buffer);
            //mat.SetConstantBuffer_("grid_constant", this.GridInstructions.Buffer);
        }

        //public void SetArgumentBuffer(Mesh mesh)
        //{
        //    var iargparams = new IndirectArgumentsForInstancing(mesh, 1);// 1 はダミー、0 だと怒られる
        //    this.CubeInstancingArgs.Buffer.SetData(ref iargparams);
        //}
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GridInstraction
    {
        public float3 position;
        public int GridDynamicIndex;
        public NearGridIndex GridStaticIndex;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct NearGridIndex
    {
        [FieldOffset(0)] public GridindexUnit left;
        [FieldOffset(16)] public GridindexUnit right;

        [FieldOffset(0)] public int4 lPack4;
        [FieldOffset(16)] public int4 rPack4;
    }
    public struct GridindexUnit
    {
        public int home;
        public int rear;
        public int down;
        public int slant;
    }
    // left_home;   // 0, 0, 0
    // left_rear;   // 0, 0, 1
    // left_down;   // 0, 1, 0
    // left_slant;  // 0, 1, 1
    // right_home;  // 1, 0, 0
    // right_rear;  // 1, 0, 1
    // right_down;  // 1, 1, 0
    // right_slant; // 1, 1, 1


    //public struct CubeInstancingIndirectArgumentsBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    public static CubeInstancingIndirectArgumentsBuffer Create() => new CubeInstancingIndirectArgumentsBuffer
    //    {
    //        Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
    //    };

    //    public void Dispose() => this.Buffer?.Release();
    //}


    public struct GridContentDataBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static GridContentDataBuffer Create(int maxGrids) => new GridContentDataBuffer
        {
            Buffer = new ComputeBuffer(32 * 32 * maxGrids, Marshal.SizeOf<uint>()),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    //public struct GridInstructionsBuffer : IDisposable
    //{
    //    public ComputeBuffer Buffer { get; private set; }

    //    // cs の dispatch は 65535 までなので、65535/1024
    //    public static GridInstructionsBuffer Create(int maxGridInstructions = 63) => new GridInstructionsBuffer
    //    {
    //        Buffer = new ComputeBuffer(maxGridInstructions, Marshal.SizeOf<GridInstraction>()),//, ComputeBufferType.Constant),
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


}

