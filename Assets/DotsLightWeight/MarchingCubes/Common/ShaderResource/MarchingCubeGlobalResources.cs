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

        public struct CommonShaderResources : IDisposable
        {
            public CubeGeometryConstantBuffer CubeGeometryConstants;


            public void Alloc(MarchingCubesAsset asset)
            {
                Debug.Log($"CommonShaderResources alloc");
                this.CubeGeometryConstants = CubeGeometryConstantBuffer.Create(asset);
            }

            public void Dispose()
            {
                Debug.Log($"CommonShaderResources disposed");
                this.CubeGeometryConstants.Dispose();
            }

            public void SetResourcesTo(Material mat)
            {
                // せつめい - - - - - - - - - - - - - - - - -

                //uint4 cube_patterns[ 254 ][2];
                // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
                // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

                //uint4 cube_vtxs[ 12 ];
                // x: near vertex index (x>>0 | y>>8 | z>>16)
                // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
                // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
                // w: pos(x>>0 | y>>8 | z>>16)

                // - - - - - - - - - - - - - - - - - - - - -

                mat.SetConstantBuffer_("static_data", this.CubeGeometryConstants.Buffer);
            }
        }


        public struct WorkingShaderResources : IDisposable
        {
            public GridCubeIdShaderBufferTexture GridCubeIds;


            public void Alloc(int maxGridInstances, int unitOnEdge)
            {
                Debug.Log($"WorkingShaderResources alloc");
                this.GridCubeIds = GridCubeIdShaderBufferTexture.Create(maxGridInstances, unitOnEdge);
            }

            public void Dispose()
            {
                Debug.Log($"WorkingShaderResources disposed");
                this.GridCubeIds.Dispose();
            }

            public void SetResourcesTo(Material mat, ComputeShader cs)
            {
                mat.SetTexture("grid_cubeids", this.GridCubeIds.Texture);

                cs?.SetTexture(0, "dst_grid_cubeids", this.GridCubeIds.Texture);
            }
        }
    }





    public struct GridCubeIdShaderBufferTexture : IDisposable
    {
        public RenderTexture Texture { get; private set; }

        public void Dispose()
        {
            this.Texture?.Release();
            this.Texture = null;
        }

        public static GridCubeIdShaderBufferTexture Create(int maxGridInstances, int unitOnEdge)
        {
            var n = unitOnEdge;
            var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt;
            //var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt;
            var buffer = new RenderTexture(n * n, n, 0, format, 0);
            buffer.enableRandomWrite = true;
            buffer.dimension = TextureDimension.Tex2DArray;
            buffer.volumeDepth = maxGridInstances;
            buffer.Create();

            return new GridCubeIdShaderBufferTexture
            {
                Texture = buffer,
            };
        }
    }

    public struct CubeGeometryConstantBuffer
    {
        public ComputeBuffer Buffer { get; private set; }

        public void Dispose()
        {
            this.Buffer?.Release();
            this.Buffer = null;
        }

        public static CubeGeometryConstantBuffer Create(MarchingCubesAsset asset)
        {
            var vertexNormalDict = makeVertexNormalsDict_(asset.CubeIdAndVertexIndicesList);

            var cubeVertexBuffer = createCubeVertexBuffer_(asset.BaseVertexList);
            var normalBuffer = createNormalList_(vertexNormalDict);
            var cubePatternBuffer = createCubePatternBuffer_(asset.CubeIdAndVertexIndicesList, vertexNormalDict);

            return new CubeGeometryConstantBuffer
            {
                Buffer = createCubeGeometryData_(cubeVertexBuffer, cubePatternBuffer, normalBuffer),
            };


            static IEnumerable<uint4> createCubeVertexBuffer_(Vector3[] baseVertices)
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

            static Dictionary<float3, int> makeVertexNormalsDict_(MarchingCubesAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_)
            {
                return cubeIdsAndVtxIndexLists_
                    .SelectMany(x => x.normalsForVertex)
                    .Select(x => round_normal_(x))
                    .Distinct(x => x)
                    .Select((x, i) => (x, i))
                    .ToDictionary(x => x.x, x => x.i);
            }

            static IEnumerable<float4> createNormalList_(Dictionary<float3, int> normalToIdDict)
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

            static IEnumerable<uint4[]> createCubePatternBuffer_
                (MarchingCubesAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_, Dictionary<float3, int> normalToIdDict)
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
                        //Debug.Log($"{i} @ {round_normal_(normals[i])} => {normalToIdDict_[round_normal_(normals[i])]}");
                        return normalToIdDict_[round_normal_(normals[i])];
                    }
                }
            }

            static ComputeBuffer createCubeGeometryData_
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

            static float3 round_normal_(float3 x)
            {
                var digits = 5;

                return new float3((float)Math.Round(x.x, digits), (float)Math.Round(x.y, digits), (float)Math.Round(x.z, digits));
                //return new float3( new half3( x ) );
            }
        }
    }

}

