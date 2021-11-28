using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using Unity.Entities;

namespace DotsLite.MarchingCubes
{
    using DotsLite.MarchingCubes.Data;
    using DotsLite.Utilities;


    public static unsafe partial class MakeCube
    {
        public interface ICubeInstanceWriter : IDisposable
        {
            void Add(int x, int y, int z, uint cubeid);
            BlobAssetReference<Collider> CreateMesh(CollisionFilter filter);
        }


        public struct FullMeshWriter_ : ICubeInstanceWriter, IDisposable
        {
            int3 unitOnEdge;
            NativeList<float3> vtxs;
            NativeList<int3> tris;
            //public int count;//
            //public int all;//

            BlobAssetReference<MarchingCubesBlobAsset> mcdata;

            int vtxOffset;


            public FullMeshWriter_(int3 unitOnEdge, BlobAssetReference<MarchingCubesBlobAsset> mcdata)
            {
                this.unitOnEdge = unitOnEdge;

                var u = unitOnEdge;
                this.vtxs = new NativeList<float3>(u.x * u.y * u.z * 12 / 2, Allocator.Temp);
                this.tris = new NativeList<int3>(u.x * u.y * u.z * 12 / 2, Allocator.Temp);

                this.mcdata = mcdata;

                this.vtxOffset = 0;
            }

            public void Add(int x, int y, int z, uint cubeId)
            {
                //this.all++;
                ref var srcIdxLists = ref this.mcdata.Value.CubeIdAndVertexIndicesList;
                ref var srcVtxList = ref this.mcdata.Value.BaseVertexList;

                var pos = this.unitOnEdge * new float3(-0.5f, 0.5f, 0.5f) + new float3(x, -y, -z);
                if (cubeId == 0 || cubeId == 255) return;
                //this.count++;

                ref var srcIdxList = ref srcIdxLists[(int)cubeId - 1].vertexIndices;

                for (var i = 0; i < srcIdxList.Length; i++)
                {
                    this.tris.AddNoResize(this.vtxOffset + srcIdxList[i]);
                }
                for (var i = 0; i < srcVtxList.Length; i++)
                {
                    this.vtxs.AddNoResize(pos + srcVtxList[i]);
                }
                this.vtxOffset += srcVtxList.Length;
            }

            public BlobAssetReference<Collider> CreateMesh(CollisionFilter filter) =>
                MeshCollider.Create(this.vtxs, this.tris, filter);

            public void Dispose()
            {
                this.vtxs.Dispose();
                this.tris.Dispose();
            }
        }



        public struct CubeInstanceByte4
        {
            public byte x, y, z, id;
            public CubeInstanceByte4(int3 pos, uint cubeId)
            {
                this.x = (byte)pos.x;
                this.y = (byte)pos.y;
                this.z = (byte)pos.z;
                this.id = (byte)cubeId;
            }
            public CubeInstanceByte4(int x, int y, int z, uint cubeId)
            {
                this.x = (byte)x;
                this.y = (byte)y;
                this.z = (byte)z;
                this.id = (byte)cubeId;
            }
        }

        public struct FullMeshWriter : ICubeInstanceWriter, IDisposable
        {
            int3 unitOnEdge;
            NativeList<CubeInstanceByte4> cubes;

            BlobAssetReference<MarchingCubesBlobAsset> mcdata;
            


            public FullMeshWriter(int3 unitOnEdge, BlobAssetReference<MarchingCubesBlobAsset> mcdata)
            {
                this.unitOnEdge = unitOnEdge;

                var u = unitOnEdge;
                this.cubes = new NativeList<CubeInstanceByte4>(u.x * u.y * u.z, Allocator.Temp);

                this.mcdata = mcdata;
            }

            public void Add(int x, int y, int z, uint cubeId)
            {
                if (cubeId == 0 || cubeId == 255) return;

                this.cubes.AddNoResize(new CubeInstanceByte4(x, y, z, cubeId));
            }

            public BlobAssetReference<Collider> CreateMesh(CollisionFilter filter)
            {
                var origin = this.unitOnEdge * new float3(-0.5f, 0.5f, 0.5f);
                using var vtxs = new NativeList<float3>(this.cubes.Length * 12, Allocator.Temp);
                using var tris = new NativeList<int3>(this.cubes.Length * 12, Allocator.Temp);

                ref var srcIdxLists = ref this.mcdata.Value.CubeIdAndVertexIndicesList;
                ref var srcVtxList = ref this.mcdata.Value.BaseVertexList;

                var vtxOffset = 0;

                for (var ic = 0; ic < this.cubes.Length; ic++)// cube in this.cubes)
                {
                    var cube = this.cubes[ic];
                    ref var srcIdxList = ref srcIdxLists[(int)cube.id - 1].vertexIndices;
                    var pos = origin + new float3(cube.x, -cube.y, -cube.z);

                    for (var i = 0; i < srcIdxList.Length; i++)
                    {
                        tris.Add(vtxOffset + srcIdxList[i]);
                    }
                    for (var i = 0; i < srcVtxList.Length; i++)
                    {
                        vtxs.Add(pos + srcVtxList[i]);
                    }
                    vtxOffset += srcVtxList.Length;
                }

                return MeshCollider.Create(vtxs, tris, filter);
            }

            public void Dispose() => this.cubes.Dispose();
        }


        public struct CompoundMeshWriter : ICubeInstanceWriter, IDisposable
        {

            int3 unitOnEdge;
            NativeList<CubeInstanceByte4> cubes;

            BlobAssetReference<MarchingCubesBlobAsset> mcdata;
            DynamicBuffer<UnitCubeColliderAssetData> unitColliders;


            public CompoundMeshWriter(
                int3 unitOnEdge,
                BlobAssetReference<MarchingCubesBlobAsset> mcdata,
                DynamicBuffer<UnitCubeColliderAssetData> unitColliders)
            {
                this.unitOnEdge = unitOnEdge;

                var u = unitOnEdge;
                this.cubes = new NativeList<CubeInstanceByte4>(u.x * u.y * u.z, Allocator.Temp);

                this.mcdata = mcdata;
                this.unitColliders = unitColliders;
            }
            public void Add(int x, int y, int z, uint cubeId)
            {
                if (cubeId == 0 || cubeId == 255) return;

                this.cubes.AddNoResize(new CubeInstanceByte4(x, y, z, cubeId));
            }

            public BlobAssetReference<Collider> CreateMesh(CollisionFilter filter)
            {
                var u = this.unitOnEdge;
                var offset = u * new float3(-0.5f, 0.5f, 0.5f);

                using var dst = new NativeList<CompoundCollider.ColliderBlobInstance>(
                    this.cubes.Length, Allocator.Temp);

                foreach (var cube in this.cubes)
                {
                    var srccube = this.unitColliders[cube.id - 1];
                    var child = new CompoundCollider.ColliderBlobInstance
                    {
                        Collider = srccube.Collider,
                        CompoundFromChild = new RigidTransform
                        {
                            pos = new float3(cube.x, cube.y, cube.z) - offset + new float3(-0.5f, 0.5f, 0.5f),
                            rot = srccube.Rotation,
                        }
                    };
                    dst.AddNoResize(child);
                }

                return CompoundCollider.Create(dst);
            }

            public void Dispose() => this.cubes.Dispose();
        }
    }
}
