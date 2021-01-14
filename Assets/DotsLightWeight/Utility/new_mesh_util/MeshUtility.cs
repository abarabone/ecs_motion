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

namespace Abarabone.Geometry
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;

    static public class MeshUtilityThreadSafe
    {

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

        static public IEnumerable<MeshUnit> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            var baseVertex = 0;
            for(var i = 0; i < meshDataArray.Length; i++)
            {
                yield return new MeshUnit(i, meshDataArray[i], baseVertex);

                baseVertex += meshDataArray[i].vertexCount;
            }
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

            public IEnumerable<T> Indices() =>
                this.srcArray.Range(this.Descriptor.indexStart, this.Descriptor.indexCount);

            public IEnumerable<T> Vertices() =>
                this.srcArray.Range(this.Descriptor.firstVertex, this.Descriptor.vertexCount);

            public IEnumerable<T> IndicesWithUsing()
            {
                using (this.srcArray) return Indices();
            }

            public IEnumerable<T> VerticesWithUsing()
            {
                using (this.srcArray) return Vertices();
            }

        }

        static IEnumerable<SubMeshUnit<T>> verticesPerSubMesh<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getVertices) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.Temp);
            getVertices(meshdata, array);
            return meshdata.elementsInSubMesh2(array);
        }

        static IEnumerable<SubMeshUnit<T>> vertexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh2(meshdata.GetVertexData<T>());

        static IEnumerable<SubMeshUnit<T>> indexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh2(meshdata.GetIndexData<T>());


        static IEnumerable<SubMeshUnit<T>> elementsInSubMesh2<T>
            (this Mesh.MeshData meshdata, NativeArray<T> srcArray) where T : struct
        =>
            from i in 0.Inc(meshdata.subMeshCount)
            let desc = meshdata.GetSubMesh(i)
            select new SubMeshUnit<T>(i, desc, srcArray)
            ;
    



        static IEnumerable<(int baseVertex, T idx)> queryIndices<T>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh) where T : struct
        =>
            from xsub in srcmeshes.querySubMeshForIndexData<T>(mtsPerMesh)
            let mesh = xsub.mesh
            let submesh = xsub.submesh
            let mt = xsub.mt
            from tri in mt.isMuinusScale()
                ? submesh.Indices().AsTriangle().Reverse()
                : submesh.Indices().AsTriangle()
            from idx in tri
            select (mesh.BaseVertex + submesh.Descriptor.baseVertex, idx)
            ;
        static bool isMuinusScale(this Matrix4x4 mt)
        {
            var mtinv = math.transpose(mt);
            var up = math.cross(mtinv.c0.xyz, mtinv.c2.xyz);
            return Vector3.Dot(up, mtinv.c1.xyz) > 0.0f;
        }

        static IEnumerable<uint> queryConvertIndicesUInt
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
        =>
            from x in queryIndices<uint>(srcmeshes, mtsPerMesh)
            select (uint)x.baseVertex + x.idx
            ;
        static IEnumerable<ushort> queryConvertIndicesUShort
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
        =>
            from x in queryIndices<ushort>(srcmeshes, mtsPerMesh)
            select (ushort)(x.baseVertex + x.idx)
            ;

        static IEnumerable<T> queryConvertIndices<T>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
        =>
            default(T) switch
            {
                uint ui => (IEnumerable<T>)srcmeshes.queryConvertIndicesUInt(mtsPerMesh),
                ushort us => (IEnumerable<T>)srcmeshes.queryConvertIndicesUShort(mtsPerMesh),
            };


        static IEnumerable<Vector3> queryConvertPositions
            (
                this Mesh.MeshDataArray srcmeshes,
                IEnumerable<int> texhashPerSubMesh, IEnumerable<Matrix4x4> mtsPerMesh, float4x4 mtBaseInv
            )
        =>
            from submeshdata in srcmeshes.querySubMeshForVertices<Vector3>
                ((md, arr) => md.GetVertices(arr), texhashPerSubMesh, mtsPerMesh, mtBaseInv)
            from vtx in submeshdata.submesh.VerticesWithUsing()
            select (Vector3)math.transform(submeshdata.mt, vtx)
            ;

        static IEnumerable<Vector2> queryConvertUvs
            (
                this Mesh.MeshDataArray srcmeshes,
                IEnumerable<int> texhashPerSubMesh, IEnumerable<Matrix4x4> mtsPerMesh, Matrix4x4 mtBaseInv,
                Dictionary<int, Rect> texhashToUvRect
            )
        =>
            from submeshdata in srcmeshes.querySubMeshForVertices<Vector2>
                ((md, arr) => md.GetUVs(0, arr), texhashPerSubMesh, mtsPerMesh, mtBaseInv)
            from uv in submeshdata.submesh.VerticesWithUsing()
            select texhashToUvRect != null
                ? uv.ScaleUv(texhashToUvRect[submeshdata.texhash])
                : uv
            ;


        static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, int texhash, Matrix4x4 mt)>
            querySubMeshForVertices<T>(
                this Mesh.MeshDataArray srcmeshes,
                Action<Mesh.MeshData, NativeArray<T>> getVertices,
                IEnumerable<int> texhashPerSubMesh, IEnumerable<Matrix4x4> mtsPerMesh, Matrix4x4 mtBaseInv
            ) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = mtBaseInv * x.src1
            from xsub in (mesh.MeshData.verticesPerSubMesh(getVertices), texhashPerSubMesh).Zip()
            let submesh = xsub.src0
            let texhash = xsub.src1
            select (mesh, submesh, texhash, mt)
            ;

        static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, Matrix4x4 mt)>
            querySubMeshForIndexData<T>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            from xsub in mesh.MeshData.indexDataPerSubMesh<T>()
            select (mesh, submesh:xsub, mt)
            ;



        static (Mesh.MeshDataArray srcmeshes, Matrix4x4[] mtsPerMesh, int[] texhashesPerSubMesh, Matrix4x4 mtBaseInv)
            calculateParametorsUnmanaged(IEnumerable<GameObject> gameObjects, Transform tfBase)
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

            return (srcmeshes, mtsPerMesh, texhashesPerSubMesh, mtBaseInv);
        }

        static public Func<MeshElements<TIdx>> CombinePositionMesh<TIdx>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase)
        {
            var (srcmeshes, mtsPerMesh, texhashesPerSubMesh, mtBaseInv) =
                calculateParametorsUnmanaged(gameObjects, tfBase);

            return () => new MeshElements<TIdx>
            {
                idxs = srcmeshes.queryConvertIndices<TIdx>(mtsPerMesh).ToArray(),
                poss = srcmeshes.queryConvertPositions(texhashesPerSubMesh, mtsPerMesh, mtBaseInv).ToArray(),
            };
        }

        static public Func<MeshElements<TIdx>> CombinePositionUvMesh<TIdx>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect = null)
        {
            var (srcmeshes, mtsPerMesh, texhashesPerSubMesh, mtBaseInv) =
                calculateParametorsUnmanaged(gameObjects, tfBase);

            return () => new MeshElements<TIdx>
            {
                idxs = srcmeshes.queryConvertIndices<TIdx>(mtsPerMesh).ToArray(),
                poss = srcmeshes.queryConvertPositions(texhashesPerSubMesh, mtsPerMesh, mtBaseInv).ToArray(),
                uvs = srcmeshes.queryConvertUvs(texhashesPerSubMesh, mtsPerMesh, mtBaseInv, texhashToUvRect).ToArray(),
            };
        }

        public struct MeshElements<TIdx>
        {
            public TIdx[] idxs;
            public Vector3[] poss;
            public Vector2[] uvs;
            public Vector3[] nms;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PosisionUvVertex
        {
            public Vector3 Position;
            public Vector2 Uv;
        }


        static public Mesh CreateMesh<T>(this MeshElements<T> meshElements) where T : struct
        {
            var dstmeshes = Mesh.AllocateWritableMeshData(1);
            var dstmesh = new Mesh();

            var m = meshElements;

            var idxs = m.idxs.ToArray();
            dstmeshes[0].AsIndex<T>(idxs.Length);
            dstmeshes[0].GetIndexData<T>().CopyFrom(idxs);

            var qVtx =
                from x in (m.poss, m.uvs).Zip()
                select new PosisionUvVertex
                {
                    Position = x.src0,
                    Uv = x.src1,
                };
            var vtxs = qVtx.ToArray();
            dstmeshes[0].AsPositionUv(vtxs.Length);
            dstmeshes[0].GetVertexData<PosisionUvVertex>().CopyFrom(vtxs);

            Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);

            return dstmesh;
        }

        static public IEnumerable<IEnumerable<T>> AsTriangle<T>(this IEnumerable<T> indecies_)
            where T : struct
        {
            using var e = indecies_.GetEnumerator();

            while (e.MoveNext())
            {
                yield return tri_();

                IEnumerable<T> tri_()
                {
                    yield return e.Current; e.MoveNext();
                    //if (!e.MoveNext()) yield break;
                    yield return e.Current; e.MoveNext();
                    //if (!e.MoveNext()) yield break;
                    yield return e.Current;
                }
            }
        }

        static float3 TransformPosition(float3 position, float4x4 mt) =>
            math.transform(mt, position);


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
        static void AsIndex<T>(this Mesh.MeshData meshdata, int indexLength) where T : struct
        {
            var format = default(T) switch
            {
                uint ui => IndexFormat.UInt32,
                ushort us => IndexFormat.UInt16,
            };
            meshdata.SetIndexBufferParams(indexLength, format);
        }
    }



    /// <summary>
    /// ���b�V�����e�v�f���ƂɌ�������B
    /// </summary>
    public static partial class MeshCombiner2
	{

        //	/// <summary>
        //	/// Mesh �v�f����������f���Q�[�g��Ԃ��B�ʒu�̂݁B
        //	/// </summary>
        //	static public Func<MeshCombinerElements> BuildPositionMeshCombiner
        //		(IEnumerable<GameObject> gameObjects, Transform tfBase)
        //	{
        //		var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

        //		var meshes = mmts.Select(x => x.mesh).ToArray();
        //		var srcmesh = Mesh.AcquireReadOnlyMeshData(meshes);


        //		var dstmesh = Mesh.AllocateWritableMeshData(1);

        //		return BuildPositionMeshCombiner(mmts, tfBase);
        //	}

        //	static public Func<MeshCombinerElements> BuildPositionMeshCombiner
        //		(Mesh.MeshDataArray dstmeshes, Mesh.MeshDataArray srcmeshes,
        //		(Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase)
        //	{

        //		var vtxss = (from x in mmts select x.mesh.vertices).ToArray();

        //		var idxsss = (from x in mmts select x.mesh)
        //			.To(PerSubMeshPerMesh.QueryIndices).ToArrayRecursive2();

        //		var mtBaseInv = tfBase.worldToLocalMatrix;
        //		var mtObjects = (from x in mmts select x.tf.localToWorldMatrix).ToArray();
        //		//Debug.Log( string.Join(";",mmts.SelectMany(x=>x.mats).Select(x=>$"{x.name} {x.GetHashCode()}")) );

        //		return () =>
        //		{
        //			var materialsCombined = (from x in mmts select x.mats)
        //				.To(MaterialCombined.QueryCombine).ToArray();

        //			return new MeshCombinerElements
        //			{
        //				Vertecies = vtxss.ToVerticesArray(mtObjects, mtBaseInv),

        //				IndicesPerSubmesh = isCombineSubMeshes
        //					? ConvertUtility.ToIndicesArray(vtxss, idxsss, mtObjects)
        //					: ConvertUtility.ToIndicesArray(vtxss, idxsss, mtObjects, (from x in mmts select x.mats), materialsCombined),

        //				MtBaseInv = mtBaseInv,
        //				mtObjects = mtObjects,

        //				materials = materialsCombined,
        //			};
        //		};
        //	}

    }
}