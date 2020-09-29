using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.Experimental;

namespace Abarabone.MarchingCubes
{

    /// グリッド配列は本体へのポインタのみ持ち、all 0 / all 1 のグリッドはデフォルトとして使いまわされる。
    static public unsafe class CubeGridGlobalData
    {

        static public UnsafePtrList FreeBlankGridStock;
        static public UnsafePtrList FreeSolidGridStock;
        static public UnsafeList FreeDefaultGridStock;

        static public CubeGrid32x32x32Unsafe DefaultBlankGrid => FreeDefaultGridStock.AsRef<CubeGrid32x32x32Unsafe>(0);
        static public CubeGrid32x32x32Unsafe DefaultSolidGrid => FreeDefaultGridStock.AsRef<CubeGrid32x32x32Unsafe>(1);


        static public void Init(int maxGridLength)
        {
            FreeBlankGridStock = new UnsafePtrList(maxGridLength, Allocator.Persistent);
            FreeSolidGridStock = new UnsafePtrList(maxGridLength, Allocator.Persistent);
            FreeDefaultGridStock = new UnsafeList(sizeof(CubeGrid32x32x32Unsafe), 4, 2, Allocator.Persistent);

            FreeDefaultGridStock.AsRef<CubeGrid32x32x32Unsafe>(0) = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Blank);
            FreeDefaultGridStock.AsRef<CubeGrid32x32x32Unsafe>(1) = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);
        }

        static public unsafe void Dispose()
        {
            DefaultBlankGrid.Dispose();
            DefaultSolidGrid.Dispose();

            disposeAll_(FreeBlankGridStock);
            disposeAll_(FreeSolidGridStock);

            FreeBlankGridStock.Dispose();
            FreeSolidGridStock.Dispose();
            FreeDefaultGridStock.Dispose();

            return;


            void disposeAll_(UnsafePtrList list)
            {
                for (var i = 0; i < list.Length; i++)
                {
                    CubeGridAllocater.Dispose((UIntPtr)(void*)list[i]);
                }
            }
        }


        //public CubeGrid32x32x32Unsafe RentGrid(GridFillMode fillMode)
        //{
        //    if (this.GridStock.length > this.usedGridCount)
        //    {
        //        var p = this.GridStock[usedGridCount++];
        //        return CubeGridAllocater.Fill(p, fillMode);
        //    }
        //    else
        //    {
        //        var grid = CubeGridAllocater.Alloc(fillMode);

        //        this.GridStock.Add((UIntPtr)grid.pUnits);
        //        this.usedGridCount++;

        //        return grid;
        //    }
        //}
        //public void BackGrid(CubeGrid32x32x32Unsafe grid)
        //{
        //    [--this.usedGridCount]

        //    if (this.GridStock.length > this.usedGridCount)
        //    {

        //    }
        //    else
        //    {

        //    }
        //}
    }

    /// <summary>
    /// グリッドを管理する。
    /// グリッド本体は必要な分のみ確保される。
    /// </summary>
    public unsafe partial struct CubeGridArrayUnsafe
    {

        // 実際に確保するグリッド配列は、外側をデフォルトグリッドでくるむ。
        // 端っこの処理を内側と統一するため。
        readonly public int3 GridLength;
        readonly int3 wholeGridLength;
        readonly int3 gridSpan;

        public UnsafeList<CubeGrid32x32x32Unsafe> grids;
        public UnsafeBitArray solidOrBlankWhenDefaultList;


        /// <summary>
        /// 
        /// </summary>
        unsafe public CubeGridArrayUnsafe(int x, int y, int z)// : this()
        {
            this.GridLength = new int3(x, y, z);
            this.wholeGridLength = new int3(x, y, z) + 2;
            this.gridSpan = new int3(1, this.wholeGridLength.x * this.wholeGridLength.z, this.wholeGridLength.x);

            var totalLength = wholeGridLength.x * wholeGridLength.y * wholeGridLength.z;
            this.grids = new UnsafeList<CubeGrid32x32x32Unsafe>(totalLength, Allocator.Persistent);
            this.solidOrBlankWhenDefaultList = new UnsafeBitArray(totalLength, Allocator.Persistent);

            var startGrid = new int3(-1, -1, -1);
            var endGrid = wholeGridLength;
            this.FillCubes(startGrid, endGrid, isFillAll: false);
        }

        public unsafe void Dispose()
        {
            for (var i = 0; i < this.grids.Length; i++)
            {
                grids[i].Dispose();
            }
            this.grids.Dispose();
            this.solidOrBlankWhenDefaultList.Dispose();
        }


        public unsafe CubeGrid32x32x32UnsafePtr this[int x, int y, int z]
        {
            get
            {
                var i3 = new int3(x, y, z) + 1;
                var i = math.dot(i3, this.gridSpan);

                var gridptr = new CubeGrid32x32x32UnsafePtr { p = this.grids.Ptr + i };

                return gridptr;
            }
        }


        /// <summary>
        /// 指定したグリッド矩形を、デフォルトの all 0 か all 1 で塗りつぶす。
        /// </summary>
        public void FillCubes(int3 topLeft, int3 length3, bool isFillAll = false)
        {
            //var st = math.max( topLeft + 1, int3.zero );
            //var ed = math.min( st + length3 - 1, this.wholeGridLength - 1 );

            //var pGridTemplate = isFillAll ? this.defaultFilledCubePtr : this.defaultBlankCubePtr;

            //var yspan = this.wholeGridLength.x * this.wholeGridLength.z;
            //var zspan = this.wholeGridLength.x;

            //for( var iy = st.y; iy <= ed.y; iy++ )
            //    for( var iz = st.z; iz <= ed.z; iz++ )
            //        for( var ix = st.x; ix <= ed.x; ix++ )
            //        {
            //            this.grids[ iy * yspan + iz * zspan + ix ] = pGridTemplate;
            //        }
        }



        /// <summary>
        /// 
        /// </summary>
        static NearCubeGrids getGridSet_
            ( ref CubeGridArrayUnsafe gridArray, int ix, int iy, int iz, int yspan_, int zspan_ )
        {
            var i = iy * yspan_ + iz * zspan_ + ix;

            return new NearCubeGrids
            {
                //L =
                //{
                //    x = gridArray.grids[ i + 0 ],
                //    y = gridArray.grids[ i + yspan_ + 0 ],
                //    z = gridArray.grids[ i + zspan_ + 0 ],
                //    w = gridArray.grids[ i + yspan_ + zspan_ + 0 ],
                //},
                //R =
                //{
                //    x = gridArray.grids[ i + 1 ],
                //    y = gridArray.grids[ i + yspan_ + 1 ],
                //    z = gridArray.grids[ i + zspan_ + 1 ],
                //    w = gridArray.grids[ i + yspan_ + zspan_ + 1 ],
                //},
            };
        }

        struct GridCounts
        {
            public int4 L, R;
        }
        static GridCounts getEachCount( ref NearCubeGrids g )
        {
            var gridCount = new int4
            (
                g.L.x.p->CubeCount,
                g.L.y.p->CubeCount,
                g.L.z.p->CubeCount,
                g.L.w.p->CubeCount
            );
            var gridCount_right = new int4
            (
                g.R.x.p->CubeCount,
                g.R.y.p->CubeCount,
                g.R.z.p->CubeCount,
                g.R.w.p->CubeCount
            );
            return new GridCounts { L = gridCount, R = gridCount_right };
        }

        static bool isNeedDraw_( int4 gridCount, int4 gridCount_right )
        {
            var addvalue = gridCount + gridCount_right;
            var isZero = !math.any( addvalue );
            var isFull = math.all( addvalue == 0x8000 << 1);
            return !(isZero | isFull);
        }
        



        /// <summary>
        /// 
        /// </summary>
        public void BuildCubeInstanceDataDirect( int3 gridIndex, NativeList<CubeInstance> cubeInstances )
        {

            var yspan = this.wholeGridLength.x * this.wholeGridLength.z;
            var zspan = this.wholeGridLength.x;

            var igrid = gridIndex + 1;
            var gridset = getGridSet_( ref this, igrid.x, igrid.y, igrid.z, yspan, zspan );
            var gridcount = getEachCount( ref gridset );

            if( !isNeedDraw_( gridcount.L, gridcount.R ) ) return;


            new SingleGridJob
            {
                gridArray = this,
                dstCubeInstances = cubeInstances,
                gridset = gridset,
                gridcount = gridcount,
            }
            .Run();
        }




        /// <summary>
        /// 
        /// </summary>
        public JobHandle BuildCubeInstanceData
            ( NativeList<GridInstanceData> gridData, NativeList<CubeInstance> cubeInstances )
        {

            var job = new GridJob
            {
                gridArray = this,
                dstCubeInstances = cubeInstances,
                dstGridData = gridData,
            }
            .Schedule();

            return job;
        }
        

        /// <summary>
        /// 
        /// </summary>
        public JobHandle BuildCubeInstanceDataParaQ
            ( NativeList<float4> gridPositions, NativeList<CubeInstance> cubeInstances )
        {

            var gridsets = new NativeList<NearCubeGrids>( 100, Allocator.TempJob );


            var dispJob = new GridDispatchJob
            {
                gridArray = this,
                dstGridPositions = gridPositions,
                dstNearGrids = gridsets,
            }
            .Schedule();


            //var dstCubeInstances = new InstanceCubeByParaList { list = cubeInstances.AsParallelWriter() };
            var cubeQueue = new NativeQueue<CubeInstance>( Allocator.TempJob );
            var dstCubeInstances = new InstanceCubeByParaQueue { queue = cubeQueue.AsParallelWriter() };
            var instJob = new CubeInstanceJob<InstanceCubeByParaQueue>
            {
                nearGrids = gridsets.AsDeferredJobArray(),
                dstCubeInstances = dstCubeInstances,
            }
            .Schedule( gridsets, -1, dispJob );

            var copyJob = new QueueToListJob<CubeInstance>
            {
                queue = cubeQueue,
                list = cubeInstances,
            }
            .Schedule( instJob );

            gridsets.Dispose( instJob );
            cubeQueue.Dispose( copyJob );

            //return instJob;
            return copyJob;
        }


        /// <summary>
        /// 
        /// </summary>
        public JobHandle BuildCubeInstanceDataPara
            ( NativeList<float4> gridPositions, NativeList<CubeInstance> cubeInstances )
        {

            var gridsets = new NativeList<NearCubeGrids>( 100, Allocator.TempJob );


            var dispJob = new GridDispatchJob
            {
                gridArray = this,
                dstGridPositions = gridPositions,
                dstNearGrids = gridsets,
            }
            .Schedule();


            var dstCubeInstances = new InstanceCubeByParaList { list = cubeInstances.AsParallelWriter() };
            var instJob = new CubeInstanceJob<InstanceCubeByParaList>
            {
                nearGrids = gridsets.AsDeferredJobArray(),
                dstCubeInstances = dstCubeInstances,
            }
            .Schedule( gridsets, -1, dispJob );


            gridsets.Dispose( instJob );

            return instJob;
        }




    }
}

