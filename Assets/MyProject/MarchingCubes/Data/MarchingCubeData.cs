﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{

    using Abarabone.Draw;
    using Abarabone.Utilities;



    //static public partial class CubeGrid
    //{
    //    public struct BufferData// : IComponentData
    //    {
    //        public UIntPtr pCubes;
    //        public int CubeCount;
    //    }
    //}

    static public partial class CubeGridGlobal
    {

        public struct BufferData : IComponentData
        {
            public UnsafeList<UIntPtr> FreeGridStocks;
        }


        [InternalBufferCapacity(2)]
        public struct FreeGridStockData : IBufferElementData
        {
            public UnsafeList<UIntPtr> FreeGridStocks;
        }


        [InternalBufferCapacity(2)]
        public struct DefualtGridData : IBufferElementData
        {
            public CubeGrid32x32x32Unsafe DefaultGrid;
        }

        //public struct DefualtGridSolidData : IComponentData
        //{
        //    public CubeGrid32x32x32Unsafe DefaultGrid;
        //}
        //public struct DefualtGridBlankData : IComponentData
        //{
        //    public CubeGrid32x32x32Unsafe DefaultGrid;
        //}

        public struct InfoData : IComponentData
        {
            public int MaxDrawGridLength;
            public int MaxCubeInstanceLength;
        }



        static CubeGrid32x32x32Unsafe rentGrid
            (ref this DynamicBuffer<FreeGridStockData> buf, ref CubeGrid32x32x32Unsafe grid, GridFillMode fillMode)
        {
            const int dotnum = CubeGrid32x32x32Unsafe.dotNum;

            ref var stocks = ref buf.ElementAt((int)fillMode).FreeGridStocks;
            var p = stocks.ElementAt(stocks.length - 1);
            return new CubeGrid32x32x32Unsafe(p, dotnum * (int)fillMode);
        }
        static public CubeGrid32x32x32Unsafe RentBlankGrid(ref this DynamicBuffer<FreeGridStockData> buf, ref CubeGrid32x32x32Unsafe grid) =>
            buf.rentGrid(ref grid, GridFillMode.Blank);
        static public CubeGrid32x32x32Unsafe RentSolidGrid(ref this DynamicBuffer<FreeGridStockData> buf, ref CubeGrid32x32x32Unsafe grid) =>
            buf.rentGrid(ref grid, GridFillMode.Solid);

        public static CubeGrid32x32x32Unsafe Blank(ref this DynamicBuffer<DefualtGridData> buf) => buf[0].DefaultGrid;
        public static CubeGrid32x32x32Unsafe Solid(ref this DynamicBuffer<DefualtGridData> buf) => buf[1].DefaultGrid;

    }

    static public partial class CubeGridArea
    {

        public struct InitializeData : IComponentData
        {
            public GridFillMode FillMode;
        }


        public unsafe struct BufferData : IComponentData
        {
            public UnsafeList<CubeGrid32x32x32Unsafe> Grids;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            public int3 GridWholeLength;
        }
        public struct InfoTempData : IComponentData
        {
            public int3 GridSpan;
        }
    }




    static public partial class Resource
    {

        public class Initialize : IComponentData
        {
            public MarchingCubeAsset Asset;
            public int MaxGridLengthInShader;
        }




        public class DrawResourceData : IComponentData
        {
            public Mesh InstatnceMesh;
            public Material CubeMaterial;

            public ComputeShader GridCubeIdSetShader;
        }


        public class DrawBufferData : IComponentData, IDisposable
        {

            public ComputeBuffer ArgsBufferForInstancing;
            public ComputeBuffer ArgsBufferForDispatch;

            public ComputeBuffer NormalBuffer;
            public ComputeBuffer CubePatternBuffer;
            public ComputeBuffer CubeVertexBuffer;
            public ComputeBuffer GridBuffer;

            public ComputeBuffer CubeInstancesBuffer;
            public RenderTexture GridCubeIdBuffer;
            //public ComputeBuffer GridCubeIdBuffer;


            public void Dispose()
            {
                if (this.ArgsBufferForInstancing != null) this.ArgsBufferForInstancing.Dispose();
                if (this.ArgsBufferForDispatch != null) this.ArgsBufferForDispatch.Dispose();

                if (this.CubeInstancesBuffer != null) this.CubeInstancesBuffer.Dispose();
                if (this.GridCubeIdBuffer != null) this.GridCubeIdBuffer.Release();

                if (this.NormalBuffer != null) this.NormalBuffer.Dispose();
                if (this.CubePatternBuffer != null) this.CubePatternBuffer.Dispose();
                if (this.CubeVertexBuffer != null) this.CubeVertexBuffer.Dispose();
                if (this.GridBuffer != null) this.GridBuffer.Dispose();
            }
        }

    }




    static public partial class GridArea
    {


    }

    static public partial class Resource
    {


        static public DrawBufferData CreateDrawBufferData(MarchingCubeAsset asset, int maxGridLength)
        {
            var buf = new DrawBufferData();

            buf.ArgsBufferForInstancing = ComputeShaderUtility.CreateIndirectArgumentsBufferForInstancing();
            buf.ArgsBufferForDispatch = ComputeShaderUtility.CreateIndirectArgumentsBufferForDispatch();

            buf.CubeInstancesBuffer = createCubeIdInstancingShaderBuffer_(32 * 32 * 32 * maxGridLength);
            buf.GridCubeIdBuffer = createGridCubeIdShaderBuffer_(maxGridLength);

            var vertexNormalDict = makeVertexNormalsDict_(asset.CubeIdAndVertexIndicesList); Debug.Log(vertexNormalDict.Count);
            buf.NormalBuffer = createNormalList_(vertexNormalDict);
            buf.CubePatternBuffer = createCubePatternBuffer_(asset.CubeIdAndVertexIndicesList, vertexNormalDict);
            buf.CubeVertexBuffer = createCubeVertexBuffer_(asset.BaseVertexList);
            buf.GridBuffer = createGridShaderBuffer_(512);

            return buf;


            ComputeBuffer createCubeIdInstancingShaderBuffer_(int maxUnitLength)
            {
                var buffer = new ComputeBuffer(maxUnitLength, Marshal.SizeOf<uint>());

                return buffer;
            }

            RenderTexture createGridCubeIdShaderBuffer_(int maxGridLength_)
            {
                var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt, 0);
                //var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt, 0);
                buffer.enableRandomWrite = true;
                buffer.dimension = TextureDimension.Tex2DArray;
                buffer.volumeDepth = maxGridLength_;
                buffer.Create();

                return buffer;
            }
            //static public ComputeBuffer createGridCubeIdShaderBuffer_( int maxGridLength )
            //{
            //    var buffer = new ComputeBuffer( 32 * 32 * 32 * maxGridLength, Marshal.SizeOf<uint>() );

            //    return buffer;
            //}


            float3 round_normal_(float3 x)
            {
                var digits = 5;

                return new float3((float)Math.Round(x.x, digits), (float)Math.Round(x.y, digits), (float)Math.Round(x.z, digits));
                //return new float3( new half3( x ) );
            }
            Dictionary<float3, int> makeVertexNormalsDict_(MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_)
            {
                return cubeIdsAndVtxIndexLists_
                    .SelectMany(x => x.normalsForVertex)
                    .Select(x => round_normal_(x))
                    .Distinct(x => x)
                    .Select((x, i) => (x, i))
                    .ToDictionary(x => x.x, x => x.i);
            }

             ComputeBuffer createNormalList_(Dictionary<float3, int> normalToIdDict)
            {
                var buffer = new ComputeBuffer(normalToIdDict.Count, Marshal.SizeOf<Vector4>(), ComputeBufferType.Constant);

                var q =
                    from n in normalToIdDict
                        //.OrderBy( x => x.Value )
                        //.Do( x => Debug.Log( $"{x.Value} {x.Key}" ) )
                        .Select(x => x.Key)
                    select new Vector4
                    {
                        x = n.x,
                        y = n.y,
                        z = n.z,
                        w = 0.0f,
                    };

                buffer.SetData(q.ToArray());

                return buffer;
            }

            ComputeBuffer createCubePatternBuffer_(MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_, Dictionary<float3, int> normalToIdDict)
            {
                //var buffer = new ComputeBuffer( 254, Marshal.SizeOf<uint4>() * 2, ComputeBufferType.Constant );
                var buffer = new ComputeBuffer(254 * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

                var q =
                    from cube in cubeIdsAndVtxIndexLists_
                    orderby cube.cubeId
                    select new[]
                    {
                        toTriPositionIndex_( cube.vertexIndices ),
                        toVtxNormalIndex_( cube.normalsForVertex, normalToIdDict )
                    };
                //q.SelectMany(x=>x).ForEach( x => Debug.Log(x) );
                buffer.SetData(q.SelectMany(x => x).Select(x => math.asfloat(x)).ToArray());

                return buffer;


                uint4 toTriPositionIndex_(int[] indices)
                {
                    var idxs = indices
                        .Concat(Enumerable.Repeat(0, 12 - indices.Length))
                        .ToArray();

                    return new uint4
                    {
                        x = (idxs[0], idxs[1], idxs[2], 0).PackToByte4Uint(),
                        y = (idxs[3], idxs[4], idxs[5], 0).PackToByte4Uint(),
                        z = (idxs[6], idxs[7], idxs[8], 0).PackToByte4Uint(),
                        w = (idxs[9], idxs[10], idxs[11], 0).PackToByte4Uint(),
                        //x = (uint)( idxs[ 0]<<0 & 0xff | idxs[ 1]<<8 & 0xff00 | idxs[ 2]<<16 & 0xff0000 ),
                        //y = (uint)( idxs[ 3]<<0 & 0xff | idxs[ 4]<<8 & 0xff00 | idxs[ 5]<<16 & 0xff0000 ),
                        //z = (uint)( idxs[ 6]<<0 & 0xff | idxs[ 7]<<8 & 0xff00 | idxs[ 8]<<16 & 0xff0000 ),
                        //w = (uint)( idxs[ 9]<<0 & 0xff | idxs[10]<<8 & 0xff00 | idxs[11]<<16 & 0xff0000 ),
                    };
                }
                uint4 toVtxNormalIndex_(Vector3[] normals, Dictionary<float3, int> normalToIdDict_)
                {
                    return new uint4
                    {
                        x = (ntoi(0), ntoi(1), ntoi(2), ntoi(3)).PackToByte4Uint(),
                        y = (ntoi(4), ntoi(5), ntoi(6), ntoi(7)).PackToByte4Uint(),
                        z = (ntoi(8), ntoi(9), ntoi(10), ntoi(11)).PackToByte4Uint(),
                        //x = (uint)( ntoi(0,0) | ntoi(1,8) | ntoi( 2,16) | ntoi( 3,24) ),
                        //y = (uint)( ntoi(4,0) | ntoi(5,8) | ntoi( 6,16) | ntoi( 7,24) ),
                        //z = (uint)( ntoi(8,0) | ntoi(9,8) | ntoi(10,16) | ntoi(11,24) ),
                        w = 0,
                    };
                    //int ntoi( int i, int shift ) => (normalToIdDict_[ round_normal_(normals[ i ]) ] & 0xff) << shift;
                    int ntoi(int i)
                    {
                        Debug.Log($"{i} @ {round_normal_(normals[i])} => {normalToIdDict_[round_normal_(normals[i])]}");
                        return normalToIdDict_[round_normal_(normals[i])];
                    }
                }
            }

            ComputeBuffer createCubeVertexBuffer_(Vector3[] baseVertices)
            {
                var buffer = new ComputeBuffer(12, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

                ((int x, int y, int z) ortho1, (int x, int y, int z) ortho2)[] near_cube_offsets =
                {
                    (( 0, 0, -1), ( 0, -1, 0)),
                    (( -1, 0, 0), ( 0, -1, 0)),
                    (( +1, 0, 0), ( 0, -1, 0)),
                    (( 0, 0, +1), ( 0, -1, 0)),

                    (( -1, 0, 0), ( 0, 0, -1)),
                    (( +1, 0, 0), ( 0, 0, -1)),
                    (( -1, 0, 0), ( 0, 0, +1)),
                    (( +1, 0, 0), ( 0, 0, +1)),

                    (( 0, 0, -1), ( 0, +1, 0)),
                    (( -1, 0, 0), ( 0, +1, 0)),
                    (( +1, 0, 0), ( 0, +1, 0)),
                    (( 0, 0, +1), ( 0, +1, 0)),
                };
                (int ortho1, int ortho2, int slant)[] near_cube_ivtxs =
                {
                    (3,8,11),
                    (2,9,10),
                    (1,10,9),
                    (0,11,8),

                    (5,6,7),
                    (4,7,6),
                    (7,4,5),
                    (6,5,4),

                    (11,0,3),
                    (10,1,2),
                    (9,2,1),
                    (8,3,0),
                };

                var q =
                    from v in Enumerable
                        .Zip(near_cube_offsets, near_cube_ivtxs, (x, y) => (ofs: x, ivtx: y))
                        .Zip(baseVertices, (x, y) => (x.ofs, x.ivtx, pos: y))
                    let x = (v.ivtx.ortho1, v.ivtx.ortho2, v.ivtx.slant, 0).PackToByte4Uint()
                    let y = (v.ofs.ortho1.x + 1, v.ofs.ortho1.y + 1, v.ofs.ortho1.z + 1, 0).PackToByte4Uint()
                    let z = (v.ofs.ortho2.x + 1, v.ofs.ortho2.y + 1, v.ofs.ortho2.z + 1, 0).PackToByte4Uint()
                    let w = ((int)(v.pos.x * 2) + 1, (int)(v.pos.y * 2) + 1, (int)(v.pos.z * 2) + 1, 0).PackToByte4Uint()
                //let x = v.ivtx.x<<0 & 0xff | v.ivtx.y<<8 & 0xff00 | v.ivtx.z<<16 & 0xff0000
                //let y = v.ofs.ortho1.x+1<<0 & 0xff | v.ofs.ortho1.y+1<<8 & 0xff00 | v.ofs.ortho1.z+1<<16 & 0xff0000
                //let z = v.ofs.ortho2.x+1<<0 & 0xff | v.ofs.ortho2.y+1<<8 & 0xff00 | v.ofs.ortho2.z+1<<16 & 0xff0000
                //let w = (int)(v.pos.x*2+1)<<0 & 0xff | (int)(v.pos.y*2+1)<<8 & 0xff00 | (int)(v.pos.z*2+1)<<16 & 0xff0000
                select new uint4(x, y, z, w)
                    ;

                buffer.SetData(q.Select(x => math.asfloat(x)).ToArray());

                return buffer;
            }

            ComputeBuffer createGridShaderBuffer_(int maxGridLength_)
            {
                var buffer = new ComputeBuffer(maxGridLength_ * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

                return buffer;
            }

        }




        static public Mesh CreateMesh()
        {
            var mesh_ = new Mesh();
            mesh_.name = "marching cube unit";

            var qVtx =
                from i in Enumerable.Range(0, 12)
                select new Vector3(i % 3, i / 3, 0)
                ;
            var qIdx =
                from i in Enumerable.Range(0, 3 * 4)
                select i
                ;
            mesh_.vertices = qVtx.ToArray();
            mesh_.triangles = qIdx.ToArray();

            return mesh_;
        }

    }
}
