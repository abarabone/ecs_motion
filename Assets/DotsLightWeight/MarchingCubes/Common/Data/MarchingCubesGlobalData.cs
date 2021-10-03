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


    namespace Global
    {
        public class InitializeData : IComponentData
        {
            public int maxFreeGrids;
            public int maxGridInstances;
            public MarchingCubeAsset asset;
        }
    }


    public class MarchingCubeGlobalData : IComponentData, IDisposable
    {
        public NativeArray<DotGrid32x32x32Unsafe> DefaultGrids;
        //public FreeStockList FreeStocks;

        public GlobalShaderResources ShaderResources;


        public void Alloc(int maxFreeGrids, MarchingCubeAsset asset, int maxGridInstances)
        {
            var defaultGrids = new NativeArray<DotGrid32x32x32Unsafe>(2, Allocator.Persistent);
            defaultGrids[(int)GridFillMode.Blank] = DotGridAllocater.Alloc(GridFillMode.Blank);
            defaultGrids[(int)GridFillMode.Solid] = DotGridAllocater.Alloc(GridFillMode.Solid);

            this.DefaultGrids = defaultGrids;
            //this.FreeStocks = new FreeStockList(maxFreeGrids);
            this.ShaderResources.Alloc(asset, maxGridInstances);
        }

        public void Dispose()
        {
            this.ShaderResources.Dispose();

            this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
            this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

            //this.FreeStocks.Dispose();
            this.DefaultGrids.Dispose();
            Debug.Log("mc global disposed");
        }
    }

    //static public partial class Resource
    //{
    //    public class Initialize : IComponentData
    //    {
    //        public MarchingCubeAsset Asset;
    //        public int MaxGridLengthInShader;
    //    }

    //}




}

