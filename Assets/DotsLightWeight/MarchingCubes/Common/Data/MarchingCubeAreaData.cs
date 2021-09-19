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


        //public unsafe struct BufferData : IComponentData
        //{
        //    public UnsafeList<DotGrid32x32x32Unsafe> Grids;
        //}

        public class ResourceGpuModeData : IComponentData
        {
            public DotGridAreaResourcesForGpu Resources;

            public ComputeShader GridToCubeShader;
            public Material CubeMaterial;
        }

        public struct ShaderInputData
        {
            public UnsafeList<GridInstanceData> GridInstances;
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

        //public struct Mode2 : IComponentData
        //{ }
        //public struct Parallel : IComponentData
        //{ }
    }







    static public partial class DotGridArea
    {


        static public ResourceGpuModeData Init(
            this ResourceGpuModeData res,
            int maxCubeInstances, int maxGridInstances,
            Material mat, ComputeShader cs)
        {
            res.Resources = new DotGridAreaResourcesForGpu(maxCubeInstances, maxGridInstances);
            //res.Resources.SetResourcesTo(mat, cs);
            res.GridToCubeShader = cs;
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




}

