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

        public class InitializeData : IComponentData
        {
            public ResourceGpuModeData ShaderResources;
            public ComputeShader GridToCubesShader;
            public Material CubeMaterial;
        }


        public class ResourceGpuModeData : IComponentData
        {
            public DotGridAreaResourcesForGpu ShaderResources;

            public ComputeShader GridToCubeShader;
            public Material CubeMaterial;
        }

        public struct ShaderInputData : IComponentData
        {
            public UnsafeList<GridInstraction> GridInstractions;
        }

    }





}

