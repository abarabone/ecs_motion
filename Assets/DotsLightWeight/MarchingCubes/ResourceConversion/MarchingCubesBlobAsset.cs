using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System;
using Unity.Physics;
using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Geometry;
    using DotsLite.Misc;

    public struct MarchingCubesBlobAsset
    {

        public BlobArray<float3> BaseVertexList;
        public BlobArray<CubeWrapper> CubeIdAndVertexIndicesList;

        public struct CubeWrapper
        {
            public byte cubeId;
            public BlobArray<int3> vertexIndices;
            public BlobArray<float3> normalsForTriangle;
            public BlobArray<float3> normalsForVertex;
        }
    }

    [InternalBufferCapacity(0)]
    public struct UnitCubeColliderAssetData : IBufferElementData
    {
        public bool IsPrimaryCube;
        public BlobAssetReference<Unity.Physics.Collider> Collider;
        public quaternion Rotation;
    }
    //public struct UnitCubeColliderAssetData : IComponentData
    //{
    //    public UnsafeList<Asset> ColliderInstances;
    //    public struct Asset
    //    {
    //        public bool IsPrimaryCube;
    //        public BlobAssetReference<Unity.Physics.Collider> Collider;
    //        public quaternion Rotation;
    //    }
    //}
    //public struct UnitCubeColliderAsset
    //{
    //    public BlobArray<ColliderAndRotation> cubeColliders;

    //    public struct ColliderAndRotation
    //    {
    //        public BlobAssetReference<Unity.Physics.Collider> Collider;
    //        public quaternion Rotation;
    //    }
    //}



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
                        var dstvis = builder.Allocate(ref dstcube.vertexIndices, srcvis.Length / 3);
                        var idst = 0;
                        for (var i = 0; i < srcvis.Length; i += 3)
                        {
                            //dstvis[idst++] = new int3(srcvis[i + 2], srcvis[i + 1], srcvis[i + 0]);
                            dstvis[idst++] = new int3(srcvis[i + 0], srcvis[i + 1], srcvis[i + 2]);
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


        public static NativeArray<UnitCubeColliderAssetData> CreateCubeColliders(this MarchingCubesAsset src, CollisionFilter filter)
        //public static UnsafeList<UnitCubeColliderAssetData.Asset> CreateCubeColliders(this MarchingCubesAsset src, CollisionFilter filter)
        {

            var primaryColliders = createPrimaryColliders_();
            var qSrccubes =
                from cube in src.CubeIdAndVertexIndicesList
                select new UnitCubeColliderAssetData
                {
                    Collider = primaryColliders[cube.primaryCubeId],
                    Rotation = cube.rotation,
                    IsPrimaryCube = primaryColliders.ContainsKey(cube.cubeId),
                };
                //select new UnitCubeColliderAssetData.Asset
                //{
                //    Collider = primaryColliders[cube.primaryCubeId],
                //    Rotation = cube.rotation,
                //    IsPrimaryCube = primaryColliders.ContainsKey(cube.cubeId),
                //};

            return qSrccubes.ToNativeArray(src.CubeIdAndVertexIndicesList.Length, Allocator.Temp);
            //var list = new UnsafeList<UnitCubeColliderAssetData.Asset>(src.CubeIdAndVertexIndicesList.Length, Allocator.Temp);
            //qSrccubes.ForEach(x => list.AddNoResize(x));
            //return list;


            Dictionary<byte, BlobAssetReference<Collider>> createPrimaryColliders_()
            {
                var primaryIds = src.CubeIdAndVertexIndicesList
                    .Select(x => x.primaryCubeId)
                    .Distinct()
                    .ToArray();
                var q =
                    from cube in src.CubeIdAndVertexIndicesList
                    join primaryId in primaryIds on cube.cubeId equals primaryId
                    let tris = cube.vertexIndices.AsTriangle()
                        .Select(x =>
                        {
                            var idxs = x.ToArray();
                            return new int3(idxs[0], idxs[1], idxs[2]);
                        })
                        .ToNativeArray(Allocator.Temp)
                    let vtxs = src.BaseVertexList
                        .Select(x => (float3)x)
                        .ToNativeArray(Allocator.Temp)
                    select (primaryId, tris, vtxs)
                    ;
                return q
                    .Select(x =>
                    {
                        using (x.vtxs)
                        using (x.tris)
                            return (x.primaryId, collider: MeshCollider.Create(x.vtxs, x.tris, filter));
                    })
                    .ToDictionary(x => x.primaryId, x => x.collider);
            }
        }
        //public static BlobAssetReference<UnitCubeColliderAsset> CreateUnitCubeAsset(
        //    this MarchingCubesAsset src, CollisionFilter filter)
        //{

        //    using var builder = new BlobBuilder(Allocator.Temp);

        //    build_(ref builder.ConstructRoot<UnitCubeColliderAsset>());

        //    return builder.CreateBlobAssetReference<UnitCubeColliderAsset>(Allocator.Persistent);



        //    void build_(ref UnitCubeColliderAsset dst)
        //    {
        //        var primaryColliders = createPrimaryColliders_();
        //        var qSrccubes =
        //            from cube in src.CubeIdAndVertexIndicesList
        //            select (cube.primaryCubeId, cube.rotation)
        //            ;
        //        var dstcubes = builder.Allocate(ref dst.cubeColliders, src.CubeIdAndVertexIndicesList.Length);
        //        var q = qSrccubes.Select((x, i) => (cube: x, i));
        //        foreach (var x in q)
        //        {
        //            dstcubes[x.i] = new UnitCubeColliderAsset.ColliderAndRotation
        //            {
        //                Collider = primaryColliders[x.i],
        //                Rotation = x.cube.rotation,
        //            };
        //        }
        //    }

        //    BlobAssetReference<Collider>[] createPrimaryColliders_()
        //    {
        //        var primaryIds = src.CubeIdAndVertexIndicesList
        //            .Select(x => x.primaryCubeId)
        //            .Distinct()
        //            .ToArray();
        //        var q =
        //            from cube in src.CubeIdAndVertexIndicesList
        //            join id in primaryIds on cube.cubeId equals id
        //            let tris = cube.vertexIndices.AsTriangle()
        //                .Select(x => new int3(x.ElementAt(0), x.ElementAt(1), x.ElementAt(2)))
        //                .ToNativeArray(Allocator.Temp)
        //            let vtxs = src.BaseVertexList
        //                .Select(x => (float3)x)
        //                .ToNativeArray(Allocator.Temp)
        //            select (tris, vtxs)
        //            ;
        //        return q
        //            .Select(x =>
        //            {
        //                using (x.vtxs)
        //                using (x.tris)
        //                    return MeshCollider.Create(x.vtxs, x.tris, filter);
        //            })
        //            .ToArray();
        //    }
        //}
    }


    public static partial class MarchingCubesUtility
    {
        static public BlobArrayEnumrable<T> ToEnumerable<T>(ref this BlobArray<T> src)
            where T : unmanaged
        {
            return new BlobArrayEnumrable<T>(ref src);
        }
    }
    
    public unsafe struct BlobItem<T>
        where T : unmanaged
    {
        public T* p;
        public ref T Value => ref *p;
    }

    public unsafe struct BlobArrayEnumrable<T> : IEnumerable<BlobItem<T>>
        where T : unmanaged//, struct
    {
        T* p;
        int length;
        public BlobArrayEnumrable(ref BlobArray<T> src)
        {
            this.p = (T*)src.GetUnsafePtr();
            this.length = src.Length;
        }

        public IEnumerator<BlobItem<T>> GetEnumerator() => new BlobArrayEnumerator(p, length);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct BlobArrayEnumerator : IEnumerator<BlobItem<T>>
        {
            T* p;
            int index;
            int length;

            public BlobArrayEnumerator(T* p, int length)
            {
                this.p = p;
                this.index = -1;
                this.length = length;
            }

            public BlobItem<T> Current => new BlobItem<T> { p = &p[index] };

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() => ++this.index < this.length;

            public void Reset() => this.index = -1;
        }
    }


}
