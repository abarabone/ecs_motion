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


        public class ResourceGpuModeData : IComponentData
        {
            public DotGridAreaResourcesForGpu ShaderResources;

            public ComputeShader GridToCubeShader;
            public Material CubeMaterial;


            static public ResourceGpuModeData Create(
                int maxCubeInstances, int maxGridInstances, Material mat, ComputeShader cs)
            {
                return new ResourceGpuModeData
                {
                    ShaderResources = new DotGridAreaResourcesForGpu(maxCubeInstances, maxGridInstances),
                    GridToCubeShader = cs,
                    CubeMaterial = mat,
                };
            }
        }

        public struct ShaderInputData : IComponentData
        {
            public UnsafeList<GridInstraction> GridInstractions;
        }

    }





}

