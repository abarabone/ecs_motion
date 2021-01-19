using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.Geometry.inner
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;



    public struct AdditionalParameters
    {
        public Matrix4x4 mtBaseInv;
        public IEnumerable<Matrix4x4> mtsPerMesh;
        public IEnumerable<int> texhashPerSubMesh;
        public Dictionary<int, Rect> texhashToUvRect;
    }


    public struct MeshUnit
    {
        public MeshUnit(int i, Mesh.MeshData meshdata, int baseVertex)
        {
            this.MeshIndex = i;
            this.MeshData = meshdata;
            this.BaseVertex = baseVertex;
        }
        public readonly int MeshIndex;
        public readonly Mesh.MeshData MeshData;
        public readonly int BaseVertex;
    }


    public struct SubMeshUnit<T> where T : struct
    {
        public SubMeshUnit(int i, SubMeshDescriptor descriptor, NativeArray<T> srcArray)
        {
            this.SubMeshIndex = i;
            this.Descriptor = descriptor;
            this.srcArray = srcArray;
        }
        public readonly int SubMeshIndex;
        public readonly SubMeshDescriptor Descriptor;
        readonly NativeArray<T> srcArray;

        public IEnumerable<T> Indices() => this.srcArray.Range(this.Descriptor.indexStart, this.Descriptor.indexCount);
        public IEnumerable<T> Vertices() => this.srcArray.Range(this.Descriptor.firstVertex, this.Descriptor.vertexCount);
        public IEnumerable<T> IndicesWithUsing(){ using (this.srcArray) return Indices(); }
        public IEnumerable<T> VerticesWithUsing(){ using (this.srcArray) return Vertices(); }
    }


    static partial class ConvertIndicesUtility
    {

        public static IEnumerable<Ti> QueryConvertIndices<TIdx, Ti>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where TIdx : IIndexType<Ti>, new()
            where Ti : struct
        {
            return
                from xsub in srcmeshes.QuerySubMeshForIndexData<Ti>(mtsPerMesh)
                let mesh = xsub.mesh
                let submesh = xsub.submesh
                let mt = xsub.mt
                from tri in mt.IsMuinusScale()
                    ? submesh.Indices().AsTriangle().Reverse()
                    : submesh.Indices().AsTriangle()
                from idx in tri.Do(x => Debug.Log(x))
                select new TIdx().Add(mesh.BaseVertex + submesh.Descriptor.baseVertex, idx)
            ;
        }

    }

    static class VertexUtility
    {

        static public IEnumerable<Vector3> QueryConvertPositions
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from x in srcmeshes.QuerySubMeshForElements<Vector3>(p, (md, arr) => md.GetVertices(arr))
            from xsub in x.submeshes
            from vtx in xsub.submesh.VerticesWithUsing()
            select (Vector3)math.transform(x.mt, vtx)
            ;


        static public IEnumerable<Vector2> QueryConvertUvs
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from x in srcmeshes.QuerySubMeshForElements<Vector2>(p, (md, arr) => md.GetUVs(0, arr))
            from xsub in x.submeshes
            from uv in xsub.submesh.VerticesWithUsing()
            select p.texhashToUvRect != null
                ? uv.ScaleUv(p.texhashToUvRect[xsub.texhash])
                : uv
            ;

    }

    static class MeshElementsSourceUtility
    {

        public static IEnumerable<SubMeshUnit<T>> ElementsPerSubMeshWithAlloc<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.Temp);
            getElementSrc(meshdata, array);
            return meshdata.elementsInSubMesh(array);
        }

        public static IEnumerable<SubMeshUnit<T>> VertexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh(meshdata.GetVertexData<T>());

        public static IEnumerable<SubMeshUnit<T>> IndexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh(meshdata.GetIndexData<T>());



        static IEnumerable<SubMeshUnit<T>> elementsInSubMesh<T>
            (this Mesh.MeshData meshdata, NativeArray<T> srcArray) where T : struct
        =>
            from i in 0.Inc(meshdata.subMeshCount)
            let desc = meshdata.GetSubMesh(i)
            select new SubMeshUnit<T>(i, desc, srcArray)
            ;
        
    }


    static class SubmeshQyeryConvertUtility
    {


        public static IEnumerable<(Matrix4x4 mt, IEnumerable<(SubMeshUnit<T> submesh, int texhash)> submeshes)>
            QuerySubMeshForElements<T>
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters adpars, Action<Mesh.MeshData, NativeArray<T>> getElementSrc)
            where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), adpars.mtsPerMesh).Zip()
            let submeshes = x.src0.MeshData.ElementsPerSubMeshWithAlloc<T>(getElementSrc)
            let mt = adpars.mtBaseInv * x.src1
            select (
                mt,
                from xsub in (submeshes, adpars.texhashPerSubMesh).Zip()
                let submesh = xsub.src0
                let texhash = xsub.src1
                select (submesh, texhash)
            );


        public static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, Matrix4x4 mt)>
            QuerySubMeshForIndexData<T>(this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            from xsub in mesh.MeshData.IndexDataPerSubMesh<T>()
            select (mesh, submesh: xsub, mt)
            ;
    }

    static class MeshQyeryConvertUtility
    {
        static public IEnumerable<MeshUnit> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            var baseVertex = 0;
            for (var i = 0; i < meshDataArray.Length; i++)
            {
                yield return new MeshUnit(i, meshDataArray[i], baseVertex);

                baseVertex += meshDataArray[i].vertexCount;
            }
        }


    }
}
namespace Abarabone.Geometry
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner;



    public struct MeshElements<TIdx> where TIdx : struct
    {
        public TIdx[] idxs;
        public Vector3[] poss;
        public Vector2[] uvs;
        public Vector3[] nms;
    }

    public interface IIndexType<T>
    {
        public T Add(int l, T r);
    }
    public struct UI16 : IIndexType<ushort>
    {
        public ushort Add(int l, ushort r) => (ushort)(l + r);
    }
    public struct UI32 : IIndexType<uint>
    {
        public uint Add(int l, uint r) => (uint)(l + r);
    }


    public interface IVertexUnit<TVtx>
        where TVtx : struct
    {
        public IEnumerable<TVtx> Replace<TIdx>(MeshElements<TIdx> src) where TIdx : struct;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionVertex : IVertexUnit<PositionVertex>
    {
        public Vector3 Position;

        public IEnumerable<PositionVertex> Replace<TIdx>(MeshElements<TIdx> src) where TIdx : struct =>
            from x in src.poss
            select new PositionVertex
            {
                Position = x
            };
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionUvVertex : IVertexUnit<PositionUvVertex>
    {
        public Vector3 Position;
        public Vector2 Uv;

        public IEnumerable<PositionUvVertex> Replace<TIdx>(MeshElements<TIdx> src) where TIdx : struct =>
            from x in (src.poss.Do(x => Debug.Log(x)), src.uvs).Zip()
            select new PositionUvVertex
            {
                Position = x.src0,
                Uv = x.src1,
            };
    }


    public static class MeshCombineUtility
    {
        //static IIndexType<T> getAdder<T>() where T : struct =>
        //    default(T) switch
        //    {
        //        ushort us => new UI16(),
        //        uint ui => new UI32(),
        //    };
        

        static public Func<MeshElements<Ti>> CombinePositionMesh<TIdx, Ti>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase)
            where TIdx : IIndexType<Ti>, new()
            where Ti : struct
        {
            var (srcmeshes, p) = calculateParametors(gameObjects, tfBase);

            return () => new MeshElements<Ti>
            {
                idxs = srcmeshes.QueryConvertIndices<TIdx, Ti>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
            };
        }

        static public Func<MeshElements<Ti>> CombinePositionUvMesh<TIdx, Ti>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect = null)
            where TIdx : IIndexType<Ti>, new() 
            where Ti : struct
        {
            var (srcmeshes, p) = calculateParametors(gameObjects, tfBase, texhashToUvRect);

            return () => new MeshElements<Ti>
            {
                idxs = srcmeshes.QueryConvertIndices<TIdx, Ti>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                uvs = srcmeshes.QueryConvertUvs(p).ToArray(),
            };
        }


        static (Mesh.MeshDataArray, AdditionalParameters) calculateParametors
            (IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect = null)
        {
            var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

            var meshesPerMesh = mmts.Select(x => x.mesh).ToArray();
            var mtsPerMesh = mmts.Select(x => x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts
                from mat in mmt.mats
                select mat.mainTexture?.GetHashCode() ?? 0
            ).ToArray();

            var mtBaseInv = tfBase.worldToLocalMatrix;

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshesPerMesh);

            return (srcmeshes, new AdditionalParameters
            {
                mtsPerMesh = mtsPerMesh,
                texhashPerSubMesh = texhashesPerSubMesh,
                mtBaseInv = mtBaseInv,
                texhashToUvRect = texhashToUvRect,
            });
        }
    }



    static public partial class MeshCreatorUtility
    {

        public static Mesh CreateMesh<TIdx, TVtx>(this MeshElements<TIdx> meshElements)
            where TIdx : struct
            where TVtx : struct, IVertexUnit<TVtx>
        {
            var dstmeshes = Mesh.AllocateWritableMeshData(1);
            var dstmesh = new Mesh();

            var src = meshElements;
            var dst = dstmeshes[0];

            var idxs = src.idxs.ToArray();
            dst.AsIndex<TIdx>(idxs.Length);
            dst.GetIndexData<TIdx>().CopyFrom(idxs);

            var vtxs =  new TVtx().Replace(meshElements).ToArray();
            dst.AsPositionUv(vtxs.Length);
            dst.GetVertexData<TVtx>().CopyFrom(vtxs);

            dst.subMeshCount = 1;
            dst.SetSubMesh(0, new SubMeshDescriptor(0, idxs.Length));
            
            Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);
            dstmesh.RecalculateBounds();

            return dstmesh;
        }


        static void AsIndex<T>(this Mesh.MeshData meshdata, int indexLength) where T : struct
        {
            var format = default(T) switch
            {
                uint ui => IndexFormat.UInt32,
                ushort us => IndexFormat.UInt16,
            };
            meshdata.SetIndexBufferParams(indexLength, format);
        }

        static void AsPosition(this Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
        static void AsPositionUv(this Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
        static void AsPositionNormalUv(this Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }

    }

}
