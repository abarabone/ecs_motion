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



    static public partial class DotGridArea
    {

        public struct InitializeData : IComponentData
        {
            public GridFillMode FillMode;
        }


        public unsafe struct BufferData : IComponentData
        {
            public UnsafeList<DotGrid32x32x32Unsafe> Grids;
        }

        public class ResourceData : IComponentData
        {
            public DotGridAreaResources Resources;

            public Material CubeMaterial;
            public ComputeShader GridCubeIdSetShader;
        }

        //public struct OutputCubesData : IComponentData
        //{
        //    public UnsafeList<GridInstanceData> GridInstances;
        //    public UnsafeList<CubeInstance> CubeInstances;
        //    //public UnsafeRingQueue<CubeInstance*> CubeInstances;
        //}

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            public int3 GridWholeLength;
        }
        public struct InfoWorkData : IComponentData
        {
            public int3 GridSpan;
        }

        public struct Mode2 : IComponentData
        { }
        public struct Parallel : IComponentData
        { }
    }







    static public partial class DotGridArea
    {


        static public ResourceData Init(
            this ResourceData res,
            int maxCubeInstances, int maxGridInstances,
            Material mat, ComputeShader cs)
        {
            res.Resources = new DotGridAreaResources(maxCubeInstances, maxGridInstances);
            //res.Resources.SetResourcesTo(mat, cs);
            res.GridCubeIdSetShader = cs;
            res.CubeMaterial = mat;

            return res;
        }




        ///// <summary>
        ///// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static public unsafe DotGrid32x32x32UnsafePtr GetGridFromArea
        //    (
        //        //ref this (DotGridArea.BufferData, DotGridArea.InfoWorkData) x,
        //        ref DotGridArea.BufferData areaGrids,
        //        ref DotGridArea.InfoWorkData areaInfo,
        //        int ix, int iy, int iz
        //    )
        //{
        //    //ref var areaGrids = ref x.Item1;
        //    //ref var areaInfo = ref x.Item2;
            
        //    var i3 = new int3(ix, iy, iz) + 1;
        //    var i = math.dot(i3, areaInfo.GridSpan);

        //    return new DotGrid32x32x32UnsafePtr { p = areaGrids.Grids.Ptr + i };
        //}
    }



    public struct DotGridAreaResources : IDisposable
    {
        public IndirectArgumentsBufferForInstancing ArgsBufferForInstancing;
        public IndirectArgumentsBufferForDispatch ArgsBufferForDispatch;

        public CubeIdInstancingShaderBuffer CubeInstances;
        public GridInstancesBuffer GridInstances;


        public DotGridAreaResources(int maxCubeInstances, int maxGridInstances) : this()
        {
            this.ArgsBufferForInstancing = IndirectArgumentsBufferForInstancing.Create();
            this.ArgsBufferForDispatch = IndirectArgumentsBufferForDispatch.Create();

            this.CubeInstances = CubeIdInstancingShaderBuffer.Create(maxCubeInstances);// 32 * 32 * 32 * maxGridLength);
            this.GridInstances = GridInstancesBuffer.Create(maxGridInstances);// 512);
        }

        public void Dispose()
        {
            this.ArgsBufferForInstancing.Dispose();
            this.ArgsBufferForDispatch.Dispose();

            this.CubeInstances.Dispose();
            this.GridInstances.Dispose();
        }
    }


    public struct IndirectArgumentsBufferForInstancing : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static IndirectArgumentsBufferForInstancing Create() => new IndirectArgumentsBufferForInstancing
        {
            Buffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    public struct IndirectArgumentsBufferForDispatch : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static IndirectArgumentsBufferForDispatch Create() => new IndirectArgumentsBufferForDispatch
        {
            Buffer = new ComputeBuffer(1, sizeof(int) * 3, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    public struct CubeIdInstancingShaderBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static CubeIdInstancingShaderBuffer Create(int maxCubeInstances) => new CubeIdInstancingShaderBuffer
        {
            Buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>()),
        };

        public void Dispose() => this.Buffer?.Release();
    }

    public struct GridInstancesBuffer : IDisposable
    {
        public ComputeBuffer Buffer { get; private set; }

        public static GridInstancesBuffer Create(int maxGridLength) => new GridInstancesBuffer
        {
            Buffer = new ComputeBuffer(maxGridLength, Marshal.SizeOf<float4>() * 2, ComputeBufferType.Constant),
        };

        public void Dispose() => this.Buffer?.Release();
    }

}

