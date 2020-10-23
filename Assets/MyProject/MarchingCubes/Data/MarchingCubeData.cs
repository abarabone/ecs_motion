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

namespace Abarabone.MarchingCubes
{

    using Abarabone.Draw;
    using Abarabone.Utilities;



    public class MarchingCubeGlobalData : IComponentData//, IDisposable
    {
        public NativeList<CubeInstance> CubeInstances;
        public NativeList<GridInstanceData> GridInstances;

        public NativeArray<DotGrid32x32x32Unsafe> DefaultGrids;
        public FreeStockList FreeStocks;

        //public mc.DrawResources DrawResources;
        public DrawResources DrawResources;
        public Material CubeMaterial;
        public ComputeShader GridCubeIdSetShader;


        public MarchingCubeGlobalData Init
            (int maxCubeInstances, int maxGridInstances, int maxFreeGrids, MarchingCubeAsset asset, Material mat, ComputeShader cs)
        {
            this.CubeInstances = new NativeList<CubeInstance>(maxCubeInstances, Allocator.Persistent);
            this.GridInstances = new NativeList<GridInstanceData>(maxGridInstances, Allocator.Persistent);
            this.DefaultGrids = new NativeArray<DotGrid32x32x32Unsafe>(2, Allocator.Persistent);
            this.FreeStocks = new FreeStockList(maxFreeGrids);

            this.DefaultGrids[(int)GridFillMode.Blank] = DotGridAllocater.Alloc(GridFillMode.Blank);
            this.DefaultGrids[(int)GridFillMode.Solid] = DotGridAllocater.Alloc(GridFillMode.Solid);

            //this.DrawResources = new mc.DrawResources(asset, maxGridInstances);
            this.DrawResources = new DrawResources(asset, maxGridInstances);
            this.DrawResources.SetResourcesTo(mat, cs);
            this.GridCubeIdSetShader = cs;
            this.CubeMaterial = mat;

            return this;
        }

        public void Dispose()
        {
            this.DrawResources.Dispose();

            this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
            this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

            this.FreeStocks.Dispose();
            this.DefaultGrids.Dispose();
            this.GridInstances.Dispose();
            this.CubeInstances.Dispose();
        }
    }


    static public partial class DotGridArea
    {

        public struct InitializeData : IComponentData
        {
            public GridFillMode FillMode;
        }


        public unsafe struct BufferData : IComponentData
        {
            public UnsafeList<DotGrid32x32x32Unsafe> Grids;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            public int3 GridWholeLength;
        }
        public struct InfoWorkData : IComponentData
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

    }



    static public partial class GridArea
    {
        /// <summary>
        /// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe DotGrid32x32x32UnsafePtr GetGridFromArea
            (
                //ref this (DotGridArea.BufferData, DotGridArea.InfoWorkData) x,
                ref DotGridArea.BufferData areaGrids,
                ref DotGridArea.InfoWorkData areaInfo,
                int ix, int iy, int iz
            )
        {
            //ref var areaGrids = ref x.Item1;
            //ref var areaInfo = ref x.Item2;

            var i3 = new int3(ix, iy, iz) + 1;
            var i = math.dot(i3, areaInfo.GridSpan);

            return new DotGrid32x32x32UnsafePtr { p = areaGrids.Grids.Ptr + i };
        }
    }








    public struct DrawResources : System.IDisposable
    {
        public ComputeBuffer ArgsBufferForInstancing;
        public ComputeBuffer ArgsBufferForDispatch;

        //public ComputeBuffer NormalBuffer;
        //public ComputeBuffer CubePatternBuffer;
        //public ComputeBuffer CubeVertexBuffer;
        public ComputeBuffer StaticDataBuffer;
        public ComputeBuffer GridBuffer;

        public ComputeBuffer CubeInstancesBuffer;
        public RenderTexture GridCubeIdBuffer;
        //public ComputeBuffer GridCubeIdBuffer;

        public Mesh mesh;


        public DrawResources(MarchingCubeAsset asset, int maxGridLength) : this()
        {
            this.ArgsBufferForInstancing = ComputeShaderUtility.CreateIndirectArgumentsBufferForInstancing();
            this.ArgsBufferForDispatch = ComputeShaderUtility.CreateIndirectArgumentsBufferForDispatch();

            this.CubeInstancesBuffer = createCubeIdInstancingShaderBuffer_(32 * 32 * 32 * maxGridLength);
            this.GridCubeIdBuffer = createGridCubeIdShaderBuffer_(maxGridLength);

            var cubeVertexBuffer = createCubeVertexBuffer_(asset.BaseVertexList);
            var vertexNormalDict = makeVertexNormalsDict_(asset.CubeIdAndVertexIndicesList);
            var cubePatternBuffer = createCubePatternBuffer_(asset.CubeIdAndVertexIndicesList, vertexNormalDict);
            var normalBuffer = createNormalList_(vertexNormalDict);
            this.StaticDataBuffer = createCubeGeometryData_(cubeVertexBuffer, cubePatternBuffer, normalBuffer);

            this.GridBuffer = createGridShaderBuffer_(maxGridLength);// 512);

            this.mesh = createMesh_();
        }

        public void Dispose()
        {
            if (this.ArgsBufferForInstancing != null) this.ArgsBufferForInstancing.Dispose();
            if (this.ArgsBufferForDispatch != null) this.ArgsBufferForDispatch.Dispose();

            if (this.CubeInstancesBuffer != null) this.CubeInstancesBuffer.Dispose();
            if (this.GridCubeIdBuffer != null) this.GridCubeIdBuffer.Release();

            if (this.StaticDataBuffer != null) this.StaticDataBuffer.Dispose();
            if (this.GridBuffer != null) this.GridBuffer.Dispose();
        }

        ComputeBuffer createCubeIdInstancingShaderBuffer_(int maxUnitLength)
        {
            var buffer = new ComputeBuffer(maxUnitLength, Marshal.SizeOf<uint>());

            return buffer;
        }

        RenderTexture createGridCubeIdShaderBuffer_(int maxGridLength)
        {
            var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt, 0);
            //var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt, 0);
            buffer.enableRandomWrite = true;
            buffer.dimension = TextureDimension.Tex2DArray;
            buffer.volumeDepth = maxGridLength;
            buffer.Create();

            return buffer;
        }
        //ComputeBuffer createGridCubeIdShaderBuffer_( int maxGridLength )
        //{
        //    var buffer = new ComputeBuffer( 32 * 32 * 32 * maxGridLength, Marshal.SizeOf<uint>() );

        //    return buffer;
        //}


        static float3 round_normal_(float3 x)
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



        ComputeBuffer createCubeGeometryData_
            (IEnumerable<uint4> cubeVertices, IEnumerable<uint4[]> cubePatterns, IEnumerable<float4> normalList)
        {

            var data = new[]
                {
                    from x in cubeVertices select math.asfloat(x),
                    from x in cubePatterns from y in x select math.asfloat(y),
                    normalList,
                }
                .SelectMany(x => x)
                .ToArray();

            var buffer = new ComputeBuffer(data.Length, Marshal.SizeOf<float4>(), ComputeBufferType.Constant);

            buffer.SetData(data);

            return buffer;
        }

        IEnumerable<float4> createNormalList_(Dictionary<float3, int> normalToIdDict)
        {
            var q =
                from n in normalToIdDict
                    //.OrderBy( x => x.Value )
                    //.Do( x => Debug.Log( $"{x.Value} {x.Key}" ) )
                    .Select(x => x.Key)
                select new float4
                {
                    x = n.x,
                    y = n.y,
                    z = n.z,
                    w = 0.0f,
                };
            return q;
        }

        IEnumerable<uint4[]> createCubePatternBuffer_
            (MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_, Dictionary<float3, int> normalToIdDict)
        {
            var q =
                from cube in cubeIdsAndVtxIndexLists_
                orderby cube.cubeId
                select new[]
                {
                    toTriPositionIndex_( cube.vertexIndices ),
                    toVtxNormalIndex_( cube.normalsForVertex, normalToIdDict )
                };
            //q.SelectMany(x=>x).ForEach( x => Debug.Log(x) );
            
            return q;


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

        IEnumerable<uint4> createCubeVertexBuffer_(Vector3[] baseVertices)
        {
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

            return q;
        }

        ComputeBuffer createGridShaderBuffer_(int maxGridLength)
        {
            //var buffer = new ComputeBuffer(maxGridLength, Marshal.SizeOf<float4>()*2, ComputeBufferType.Constant);
            var buffer = new ComputeBuffer(maxGridLength * 2, Marshal.SizeOf<float4>(), ComputeBufferType.Constant);

            return buffer;
        }



        Mesh createMesh_()
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


    public static class DrawResourceExtension_
    {

        public static void SetResourcesTo(this DrawResources res, Material mat, ComputeShader cs)
        {
            //uint4 cube_patterns[ 254 ][2];
            // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
            // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

            //uint4 cube_vtxs[ 12 ];
            // x: near vertex index (x>>0 | y>>8 | z>>16)
            // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
            // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
            // w: pos(x>>0 | y>>8 | z>>16)

            //uint3 grids[ 512 ][2];
            // [0] : position as float3
            // [1] : near grid id
            // { x: prev(left>>0 | up>>9 | front>>18)  y: next(right>>0 | down>>9 | back>>18)  z: current }

            mat.SetConstantBuffer_("static_data", res.StaticDataBuffer);
            mat.SetConstantBuffer_("grid_constant", res.GridBuffer);
            //res.GridBuffer.SetData(new float4[] {new float4(), new float4()});
            //mat.SetConstantBuffer("grids", res.GridBuffer);

            mat.SetBuffer("cube_instances", res.CubeInstancesBuffer);
            mat.SetTexture("grid_cubeids", res.GridCubeIdBuffer);
            //mat.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );

            if (cs != null) cs.SetBuffer(0, "src_instances", res.CubeInstancesBuffer);
            if (cs != null) cs.SetTexture(0, "dst_grid_cubeids", res.GridCubeIdBuffer);
        }
        //public static void SetResourcesTo_(this DrawResources res, Material mat, ComputeShader cs)
        //{
        //    //uint4 cube_patterns[ 254 ][2];
        //    // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
        //    // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

        //    //uint4 cube_vtxs[ 12 ];
        //    // x: near vertex index (x>>0 | y>>8 | z>>16)
        //    // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
        //    // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
        //    // w: pos(x>>0 | y>>8 | z>>16)

        //    //uint3 grids[ 512 ][2];
        //    // [0] : position as float3
        //    // [1] : near grid id
        //    // { x: prev(left>>0 | up>>9 | front>>18)  y: next(right>>0 | down>>9 | back>>18)  z: current }

        //    mat.SetConstantBuffer("normals", res.NormalBuffer);
        //    mat.SetConstantBuffer("cube_patterns", res.CubePatternBuffer);
        //    mat.SetConstantBuffer("cube_vtxs", res.CubeVertexBuffer);
        //    //mat.SetConstantBuffer_("grid_constant", res.GridBuffer);
        //    //mat.SetConstantBuffer("grids", res.GridBuffer);

        //    mat.SetBuffer("cube_instances", res.CubeInstancesBuffer);
        //    mat.SetTexture("grid_cubeids", res.GridCubeIdBuffer);
        //    //mat.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );


        //    cs.SetBuffer(0, "src_instances", res.CubeInstancesBuffer);
        //    cs.SetTexture(0, "dst_grid_cubeids", res.GridCubeIdBuffer);
        //    //cs.SetBuffer( 0, "dst_grid_cubeids", res.GridCubeIdBuffer );
        //}

    }


}

