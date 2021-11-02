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


    public static partial class Global
    {
        public class InitializeData : IComponentData
        {
            public int maxFreeGrids;
            public int maxGridInstances;
            public MarchingCubesAsset asset;
        }


        public class CommonData : IComponentData//, IDisposable
        {
            public CommonShaderResources ShaderResources;

            public BlobAssetReference<MarchingCubesBlobAsset> Assset;


            public void Alloc(MarchingCubesAsset asset)
            {
                Debug.Log("mc global common alloc");

                this.ShaderResources.Alloc(asset);
            }

            public void Dispose()
            {
                this.ShaderResources.Dispose();

                this.Assset.Dispose();

                Debug.Log("mc global common disposed");
            }
        }


        // 32/16 別と強要に分ける必要あり
        //public class MainData<TGrid> : IComponentData, IDisposable
        //    where TGrid : struct, IDotGrid<TGrid>
        public class Work32Data : WorkData<DotGrid32x32x32>, IComponentData//, IDisposable
        { }
        public class Work16Data : WorkData<DotGrid16x16x16>, IComponentData//, IDisposable
        { }
        //{
        //    public NativeArray<DotGrid32x32x32> DefaultGrids;
        //    //public FreeStockList FreeStocks;

        //    public WorkingShaderResources ShaderResources;

        //    public BlobAssetReference<MarchingCubesBlobAsset> Assset;


        //    public void Alloc(int maxGridInstances)
        //    {
        //        var defaultGrids = new NativeArray<DotGrid32x32x32>(2, Allocator.Persistent);
        //        //defaultGrids[(int)GridFillMode.Blank] = TGrid.Allocater.Alloc(GridFillMode.Blank);
        //        //defaultGrids[(int)GridFillMode.Solid] = TGrid.Allocater.Alloc(GridFillMode.Solid);
        //        defaultGrids[(int)GridFillMode.Blank] = DotGrid32x32x32.CreateDefaultGrid(GridFillMode.Blank);
        //        defaultGrids[(int)GridFillMode.Solid] = DotGrid32x32x32.CreateDefaultGrid(GridFillMode.Solid);

        //        this.DefaultGrids = defaultGrids;
        //        //this.FreeStocks = new FreeStockList(maxFreeGrids);
        //        this.ShaderResources.Alloc<DotGrid32x32x32>(maxGridInstances);
        //    }

        //    public void Dispose()
        //    {
        //        this.ShaderResources.Dispose();

        //        this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
        //        this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

        //        //this.FreeStocks.Dispose();
        //        this.DefaultGrids.Dispose();

        //        this.Assset.Dispose();

        //        Debug.Log("mc global work32 disposed");
        //    }
        //}
        public class WorkData<TGrid> : IComponentData//, IDisposable
            where TGrid : struct, IDotGrid<TGrid>
        {
            public NativeArray<TGrid> DefaultGrids;
            //public FreeStockList FreeStocks;

            public WorkingShaderResources ShaderResources;


            public void Alloc(int maxGridInstances)
            {
                Debug.Log($"mc global work{new TGrid().UnitOnEdge} alloc");
                var defaultGrids = new NativeArray<TGrid>(2, Allocator.Persistent);
                //defaultGrids[(int)GridFillMode.Blank] = TGrid.Allocater.Alloc(GridFillMode.Blank);
                //defaultGrids[(int)GridFillMode.Solid] = TGrid.Allocater.Alloc(GridFillMode.Solid);
                defaultGrids[(int)GridFillMode.Blank] = new TGrid().Alloc(GridFillMode.Blank);
                defaultGrids[(int)GridFillMode.Solid] = new TGrid().Alloc(GridFillMode.Solid);

                this.DefaultGrids = defaultGrids;
                //this.FreeStocks = new FreeStockList(maxFreeGrids);
                this.ShaderResources.Alloc<TGrid>(maxGridInstances);
            }

            public void Dispose()
            {
                if (!this.DefaultGrids.IsCreated) return;

                this.ShaderResources.Dispose();

                this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
                this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

                //this.FreeStocks.Dispose();
                this.DefaultGrids.Dispose();

                Debug.Log($"mc global work{new TGrid().UnitOnEdge} disposed");
            }
        }
    }


    //public unsafe partial struct DotGrid32x32x32
    //{
    //    public DotGrid32x32x32 CreateDefault(GridFillMode fillmode) => CreateDefaultGrid(fillmode);
    //}

    //public unsafe partial struct DotGrid16x16x16
    //{
    //    public DotGrid16x16x16 CreateDefault(GridFillMode fillmode) => CreateDefaultGrid(fillmode);
    //}

    //static public partial class Resource
    //{
    //    public class Initialize : IComponentData
    //    {
    //        public MarchingCubeAsset Asset;
    //        public int MaxGridLengthInShader;
    //    }

    //}




}

