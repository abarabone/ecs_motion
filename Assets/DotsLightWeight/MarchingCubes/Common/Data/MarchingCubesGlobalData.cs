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
            public MarchingCubesAsset asset;
        }


        // 32/16 別と強要に分ける必要あり
        public class MainData<TGrid> : IComponentData, IDisposable
            where TGrid : struct, IDotGrid<TGrid>
        {
            public NativeArray<TGrid> DefaultGrids;
            //public FreeStockList FreeStocks;

            public GlobalShaderResources ShaderResources;

            public BlobAssetReference<MarchingCubesBlobAsset> Assset;


            public void Alloc(MarchingCubesAsset asset, int maxFreeGrids, int maxGridInstances)
            {
                var defaultGrids = new NativeArray<TGrid>(2, Allocator.Persistent);
                //defaultGrids[(int)GridFillMode.Blank] = TGrid.Allocater.Alloc(GridFillMode.Blank);
                //defaultGrids[(int)GridFillMode.Solid] = TGrid.Allocater.Alloc(GridFillMode.Solid);
                defaultGrids[(int)GridFillMode.Blank] = new TGrid().CreateDefault(GridFillMode.Blank);
                defaultGrids[(int)GridFillMode.Solid] = new TGrid().CreateDefault(GridFillMode.Solid);

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

                this.Assset.Dispose();

                Debug.Log("mc global disposed");
            }
        }
    }

    public unsafe partial struct DotGrid32x32x32
    {
        public DotGrid32x32x32 CreateDefault(GridFillMode fillmode) => CreateDefaultCube(fillmode);
    }

    public unsafe partial struct DotGrid16x16x16
    {
        public DotGrid16x16x16 CreateDefault(GridFillMode fillmode) => CreateDefaultCube(fillmode);
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

