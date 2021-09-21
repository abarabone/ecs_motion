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

        //public unsafe struct BufferData : IComponentData
        //{
        //    public UnsafeList<DotGrid32x32x32Unsafe> Grids;
        //}

        public class ResourceBurstModeData : IComponentData
        {
            public DotGridAreaResourcesForBurst ShaderResources;

            public ComputeShader GridToCubeShader;
            public Material CubeMaterial;


            //static public ResourceGpuModeData Create(
            //    int maxCubeInstances, int maxGridInstances, Material mat, ComputeShader cs)
            //{
            //    return new ResourceGpuModeData
            //    {
            //        ShaderResources = new DotGridAreaResourcesForGpu(maxCubeInstances, maxGridInstances),
            //        GridToCubeShader = cs,
            //        CubeMaterial = mat,
            //    };
            //}
        }

        //public struct OutputCubesData : IComponentData
        //{
        //    public UnsafeList<GridInstanceData> GridInstances;
        //    public UnsafeList<CubeInstance> CubeInstances;
        //    //public UnsafeRingQueue<CubeInstance*> CubeInstances;
        //}

        //public struct Mode2 : IComponentData
        //{ }
        //public struct Parallel : IComponentData
        //{ }
    }





}

