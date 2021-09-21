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



    public class MarchingCubeGlobalData : IComponentData//, IDisposable
    {
        public NativeArray<DotGrid32x32x32Unsafe> DefaultGrids;
        public FreeStockList FreeStocks;

        public GlobalResources ShaderResources;


        public static MarchingCubeGlobalData Create(int maxFreeGrids, int maxGridInstances, MarchingCubeAsset asset)
        {
            var defaultGrids = new NativeArray<DotGrid32x32x32Unsafe>(2, Allocator.Persistent);
            defaultGrids[(int)GridFillMode.Blank] = DotGridAllocater.Alloc(GridFillMode.Blank);
            defaultGrids[(int)GridFillMode.Solid] = DotGridAllocater.Alloc(GridFillMode.Solid);

            return new MarchingCubeGlobalData
            {
                DefaultGrids = defaultGrids,
                FreeStocks = new FreeStockList(maxFreeGrids),
                ShaderResources = new GlobalResources { },
            };
        }

        public void Dispose()
        {
            this.ShaderResources.Dispose();

            this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
            this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

            this.FreeStocks.Dispose();
            this.DefaultGrids.Dispose();
        }
    }

    static public partial class Resource
    {
        public class Initialize : IComponentData
        {
            public MarchingCubeAsset Asset;
            public int MaxGridLengthInShader;
        }

    }




}

