using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System;

namespace DotsLite.MarchingCubes
{

    public struct MarchingCubesBlobAsset
    {

        public BlobArray<float3> BaseVertexList;
        public BlobArray<CubeWrapper> CubeIdAndVertexIndicesList;


        public struct CubeWrapper
        {
            public byte cubeId;
            public BlobArray<int> vertexIndices;
            public BlobArray<float3> normalsForTriangle;
            public BlobArray<float3> normalsForVertex;
        }

    }

    public static partial class MarchingCubesUtility
    {


        static public BlobAssetReference<MarchingCubesBlobAsset> ConvertToBlobData(this MarchingCubesAsset src)
        {

            using var builder = new BlobBuilder(Allocator.Temp);

            copyAssetToBlob_(ref builder.ConstructRoot<MarchingCubesBlobAsset>());

            return builder.CreateBlobAssetReference<MarchingCubesBlobAsset>(Allocator.Persistent);



            void copyAssetToBlob_(ref MarchingCubesBlobAsset dst)
            {
                var srcvtxs = src.BaseVertexList;
                var dstvtxs = builder.Allocate(ref dst.BaseVertexList, srcvtxs.Length);
                for (var i = 0; i < dstvtxs.Length; i++)
                {
                    dstvtxs[i] = srcvtxs[i];
                }

                var srccubes = src.CubeIdAndVertexIndicesList;
                var dstcubes = builder.Allocate(ref dst.CubeIdAndVertexIndicesList, srccubes.Length);
                for (var i = 0; i < dstcubes.Length; i++)
                {
                    dstcubes[i].cubeId = srccubes[i].cubeId;
                    copyIndicesToblob_(ref dstcubes[i]);
                    copyNormalsForVtxIndicesToblob_(ref dstcubes[i]);
                    copyNormalsForTrianbleToblob_(ref dstcubes[i]);

                    void copyIndicesToblob_(ref MarchingCubesBlobAsset.CubeWrapper dstcube)
                    {
                        var srcvis = srccubes[i].vertexIndices;
                        var dstvis = builder.Allocate(ref dstcube.vertexIndices, srcvis.Length);
                        for (var i = 0; i < dstvis.Length; i++)
                        {
                            dstvis[i] = srcvis[i];
                        }
                    }
                    void copyNormalsForVtxIndicesToblob_(ref MarchingCubesBlobAsset.CubeWrapper dstcube)
                    {
                        var srcnvs = srccubes[i].normalsForVertex;
                        var dstnvs = builder.Allocate(ref dstcube.normalsForVertex, srcnvs.Length);
                        for (var i = 0; i < dstnvs.Length; i++)
                        {
                            dstnvs[i] = srcnvs[i];
                        }
                    }
                    void copyNormalsForTrianbleToblob_(ref MarchingCubesBlobAsset.CubeWrapper dstcube)
                    {
                        var srcnts = srccubes[i].normalsForTriangle;
                        var dstnts = builder.Allocate(ref dstcube.normalsForTriangle, srcnts.Length);
                        for (var i = 0; i < dstnts.Length; i++)
                        {
                            dstnts[i] = srcnts[i];
                        }
                    }
                }
            }
        }



    }
}
