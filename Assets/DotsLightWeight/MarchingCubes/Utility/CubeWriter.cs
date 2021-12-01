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


        public struct FullMeshWriter : ICubeInstanceWriter, IDisposable
        {
            int3 unitOnEdge;
            NativeList<float3> vtxs;
            NativeList<int3> tris;
            //public int count;//
            //public int all;//

            BlobAssetReference<MarchingCubesBlobAsset> mcdata;

            int vtxOffset;


            public FullMeshWriter(int3 unitOnEdge, BlobAssetReference<MarchingCubesBlobAsset> mcdata)
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

                var pos = this.unitOnEdge * new float3(-0.5f, 0.5f, 0.5f) + new float3(x, -y, -z) + new float3(0.5f, -0.5f, -0.5f);
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


        [StructLayout(LayoutKind.Explicit)]
        public struct CubeInstanceByte4
        {
            //public byte x, y, z, id;
            [FieldOffset(0)] public byte x;
            [FieldOffset(1)] public byte y;
            [FieldOffset(2)] public byte z;
            [FieldOffset(3)] public byte id;

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

        public struct FullMeshWriter_ : ICubeInstanceWriter, IDisposable
        {
            int3 unitOnEdge;
            NativeList<CubeInstanceByte4> cubes;

            BlobAssetReference<MarchingCubesBlobAsset> mcdata;
            


            public FullMeshWriter_(int3 unitOnEdge, BlobAssetReference<MarchingCubesBlobAsset> mcdata)
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
                var vtxs = new NativeList<float3>(this.cubes.Length * 12, Allocator.Temp);
                var tris = new NativeList<int3>(this.cubes.Length * 12, Allocator.Temp);
                //var vtxs = new NativeArray<float3>(this.cubes.Length * 12, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                //var tris = new NativeArray<int3>(this.cubes.Length * 12, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                ref var srcIdxLists = ref this.mcdata.Value.CubeIdAndVertexIndicesList;
                ref var srcVtxList = ref this.mcdata.Value.BaseVertexList;
                //UnityEngine.Debug.Log(this.cubes.Length);

                var vtxOffset = 0;

                //var iv = 0;
                //var ii = 0;
                for (var ic = 0; ic < this.cubes.Length; ic++)// cube in this.cubes)
                {
                    var cube = this.cubes[ic];
                    ref var srcidxs = ref srcIdxLists[(int)cube.id - 1].vertexIndices;
                    var pos = origin + new float3(cube.x, -cube.y, -cube.z);

                    for (var i = 0; i < srcidxs.Length; i++)
                    {
                        tris.AddNoResize(vtxOffset + srcidxs[i]);
                        //tris[ii++] = vtxOffset + srcidxs[i];
                    }
                    for (var i = 0; i < srcVtxList.Length; i++)
                    {
                        vtxs.AddNoResize(pos + srcVtxList[i]);
                        //vtxs[iv++] = pos + srcVtxList[i];
                    }
                    vtxOffset += srcVtxList.Length;
                }

                var res = MeshCollider.Create(vtxs, tris, filter);
                vtxs.Dispose();
                tris.Dispose();
                return res;
            }

            public void Dispose() => this.cubes.Dispose();
        }


        public struct CompoundMeshWriter : ICubeInstanceWriter, IDisposable
        {

            float3 origin;
            //NativeList<CompoundCollider.ColliderBlobInstance> blobs;
            NativeArray<CompoundCollider.ColliderBlobInstance> blobs;
            int length;

            NativeArray<UnitCubeColliderAssetData> unitColliders;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CompoundMeshWriter(
                int3 unitOnEdge,
                NativeArray<UnitCubeColliderAssetData> unitColliders)
            {
                var u = unitOnEdge;
                //this.blobs = new NativeList<CompoundCollider.ColliderBlobInstance>(u.x * u.y * u.z, Allocator.Temp);
                this.blobs = new NativeArray<CompoundCollider.ColliderBlobInstance>(u.x * u.y * u.z, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                this.unitColliders = unitColliders;
                this.origin = u * new float3(-0.5f, 0.5f, 0.5f);
                this.length = 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(int x, int y, int z, uint cubeId)
            {
                if (cubeId == 0 || cubeId == 255) return;

                var srccube = this.unitColliders[(int)cubeId - 1];
                var child = new CompoundCollider.ColliderBlobInstance
                {
                    Collider = srccube.Collider,
                    CompoundFromChild = new RigidTransform
                    {
                        pos = this.origin + new float3(x, -y, -z) + new float3(0.5f, -0.5f, -0.5f),
                        rot = srccube.Rotation,
                    }
                };
                //this.blobs.AddNoResize(child);
                this.blobs[length++] = child;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BlobAssetReference<Collider> CreateMesh(CollisionFilter filter) =>
                CompoundCollider.Create(this.blobs.GetSubArray(0, length));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => this.blobs.Dispose();
        }
        public struct CompoundMeshWriter_ : ICubeInstanceWriter, IDisposable
        {

            int3 unitOnEdge;
            NativeList<CubeInstanceByte4> cubes;

            DynamicBuffer<UnitCubeColliderAssetData> unitColliders;


            public CompoundMeshWriter_(
                int3 unitOnEdge,
                DynamicBuffer<UnitCubeColliderAssetData> unitColliders)
            {
                this.unitOnEdge = unitOnEdge;

                var u = unitOnEdge;
                this.cubes = new NativeList<CubeInstanceByte4>(u.x * u.y * u.z, Allocator.Temp);

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
                var origin = u * new float3(-0.5f, 0.5f, 0.5f);

                var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
                    this.cubes.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                for (var ic = 0; ic < this.cubes.Length; ic++)// cube in this.cubes)
                {
                    var cube = this.cubes[ic];
                    var srccube = this.unitColliders[cube.id - 1];
                    var child = new CompoundCollider.ColliderBlobInstance
                    {
                        Collider = srccube.Collider,
                        CompoundFromChild = new RigidTransform
                        {
                            pos = origin + new float3(cube.x, -cube.y, -cube.z) + new float3(0.5f, -0.5f, -0.5f),
                            rot = srccube.Rotation,
                        }
                    };
                    //dst.AddNoResize(child);
                    dst[ic] = child;
                }

                var res = CompoundCollider.Create(dst);
                dst.Dispose();
                return res;
            }

            public void Dispose() => this.cubes.Dispose();
        }
    }
}
