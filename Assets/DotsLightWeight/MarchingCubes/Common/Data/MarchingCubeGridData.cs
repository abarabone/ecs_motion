using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace DotsLite.MarchingCubes
{




    public static class DrawResourceExtension_
    {

        public static void SetResourcesTo(this GlobalResources res, Material mat, ComputeShader cs)
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

            mat.SetTexture("grid_cubeids", res.GridCubeIdBuffer);
            //mat.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );

            if (cs != null) cs.SetTexture(0, "dst_grid_cubeids", res.GridCubeIdBuffer);
        }


        public static void SetResourcesTo(this DotGridAreaResources res, Material mat, ComputeShader cs)
        {
            mat.SetBuffer("cube_instances", res.CubeInstancesBuffer);

            mat.SetConstantBuffer_("grid_constant", res.GridInstancesBuffer);
            //mat.SetConstantBuffer("grids", res.GridInstancesBuffer);

            if (cs != null) cs.SetBuffer(0, "src_instances", res.CubeInstancesBuffer);
        }

    }


    static class ResourceAllocator
    {

        public static ComputeBuffer createCubeIdInstancingShaderBuffer_(int maxCubeInstances)
        {
            var buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>());

            return buffer;
        }

        public static RenderTexture createGridCubeIdShaderBuffer_(int maxGridInstances)
        {
            var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt, 0);
            //var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt, 0);
            buffer.enableRandomWrite = true;
            buffer.dimension = TextureDimension.Tex2DArray;
            buffer.volumeDepth = maxGridInstances;
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
        public static Dictionary<float3, int> makeVertexNormalsDict_(MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_)
        {
            return cubeIdsAndVtxIndexLists_
                .SelectMany(x => x.normalsForVertex)
                .Select(x => round_normal_(x))
                .Distinct(x => x)
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);
        }



        public static ComputeBuffer createCubeGeometryData_
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

        public static IEnumerable<float4> createNormalList_(Dictionary<float3, int> normalToIdDict)
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

        public static IEnumerable<uint4[]> createCubePatternBuffer_
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

        public static IEnumerable<uint4> createCubeVertexBuffer_(Vector3[] baseVertices)
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

        public static ComputeBuffer createGridShaderBuffer_(int maxGridLength)
        {
            var buffer = new ComputeBuffer(maxGridLength, Marshal.SizeOf<float4>() * 2, ComputeBufferType.Constant);

            return buffer;
        }



        public static Mesh createMesh_()
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

