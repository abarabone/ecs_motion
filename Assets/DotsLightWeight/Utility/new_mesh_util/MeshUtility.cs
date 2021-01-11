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
            public SubMeshUnit(int i, SubMeshDescriptor descriptor, IEnumerable<T> submeshElements)
            {
                this.SubMeshIndex = i;
                this.Elements = submeshElements;
                this.Descriptor = descriptor;
            }
            public readonly int SubMeshIndex;
            public readonly SubMeshDescriptor Descriptor;
            public readonly IEnumerable<T> Elements;
        }

        static public IEnumerable<SubMeshUnit<float3>> PositionsPerSubMesh(this Mesh.MeshData meshdata)
        {
            var array = new NativeArray<float3>(meshdata.vertexCount, Allocator.Temp);
            meshdata.GetVertices(array.Reinterpret<Vector3>());
            return meshdata.ElementsInSubMesh(array, () => array.Dispose());
        }
        static public IEnumerable<SubMeshUnit<float2>> UvsPerSubMesh(this Mesh.MeshData meshdata)
        {
            var array = new NativeArray<float2>(meshdata.vertexCount, Allocator.Temp);
            meshdata.GetUVs(0, array.Reinterpret<Vector2>());
            return meshdata.ElementsInSubMesh(array, () => array.Dispose());
        }

        static public IEnumerable<SubMeshUnit<T>> VerticesPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetVertexData<T>());

        static public IEnumerable<SubMeshUnit<T>> IndicesPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetIndexData<T>());


        struct DisposableDummy : IDisposable
        {
            Action action;
            public DisposableDummy(Action disposeAction = null) => this.action = disposeAction;
            public void Dispose() => this.action?.Invoke();
        }
        static public IEnumerable<SubMeshUnit<T>> ElementsInSubMesh<T>(this Mesh.MeshData meshdata, NativeArray<T> srcArray, Action disposeAction = null)
            where T : struct
        {
            using var arr = new DisposableDummy(disposeAction);

            var i = 0;
            foreach (var desc in submeshDescripters_())
            {
                yield return new SubMeshUnit<T>(i++, desc, getElementsInSubmesh_());

                IEnumerable<T> getElementsInSubmesh_()
                {
                    for (var i = desc.firstVertex; i < desc.vertexCount; i++)
                    {
                        yield return srcArray[i];
                    }
                }
            }

            IEnumerable<SubMeshDescriptor> submeshDescripters_() =>
                from i in 0.Inc(meshdata.subMeshCount)
                select meshdata.GetSubMesh(i)
                ;
        }


        //[StructLayout(LayoutKind.Sequential)]
        //struct SrcVertexUnit
        //{
        //    public float3 Position;
        //    public float3 Normal;
            
        //}



        static IEnumerable<uint> ConvertIndicesUInt
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<float4x4> mtsPerMesh)
        =>
            from xsub in querySubMeshForIndex<uint>(srcmeshes, mtsPerMesh)
            let mesh = xsub.mesh
            let submesh = xsub.submesh
            let mt = xsub.mt
            from tri in mt.isMuinusScale()
                ? submesh.Elements.AsTriangle().Reverse()
                : submesh.Elements.AsTriangle()
            from idx in tri
            select (uint)mesh.BaseVertex + (uint)submesh.Descriptor.baseVertex + idx
            ;
        static bool isMuinusScale(this float4x4 mt)
        {
            var mtinv = math.transpose(mt);
            var up = math.cross(mtinv.c0.xyz, mtinv.c2.xyz);
            return Vector3.Dot(up, mtinv.c1.xyz) > 0.0f;
        }

        static IEnumerable<float3> ConvertVertices
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<int> texhashPerSubMesh, IEnumerable<float4x4> mtsPerMesh, float4x4 mtBaseInv)
        =>
            from submeshdata in querySubMeshForVertex<float3>(srcmeshes, texhashPerSubMesh, mtsPerMesh, mtBaseInv)
            from vtx in submeshdata.submesh.Elements
            select math.transform(submeshdata.mt, vtx)
            ;



        static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, int texhash, float4x4 mt)>
            querySubMeshForVertex<T>
            (Mesh.MeshDataArray srcmeshes, IEnumerable<int> texhashPerSubMesh, IEnumerable<float4x4> mtsPerMesh, float4x4 mtBaseInv) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = mtBaseInv * x.src1
            from xsub in (mesh.MeshData.VerticesPerSubMesh<T>(), texhashPerSubMesh).Zip()
            let submesh = xsub.src0
            let texhash = xsub.src1
            select (mesh, submesh, texhash, mt)
            ;

        static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, float4x4 mt)>
            querySubMeshForIndex<T>
            (Mesh.MeshDataArray srcmeshes, IEnumerable<float4x4> mtsPerMesh) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            from xsub in mesh.MeshData.IndicesPerSubMesh<T>()
            select (mesh, submesh:xsub, mt)
            ;



        static void aaa<TVtx, TIdx>(IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect,
            Func<TVtx,TVtx> vertexConversion)
        {
            var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

            var meshesPerMesh = mmts.Select(x => x.mesh).ToArray();
            var mtsPerMesh = mmts.Select(x => (float4x4)x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts
                from mat in mmt.mats
                select mat.mainTexture.GetHashCode()
            ).ToArray();

            var mtBaseInv = tfBase.worldToLocalMatrix;


            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshesPerMesh);


            var idxs = srcmeshes.ConvertIndicesUInt(mtsPerMesh);
            var vtxs = srcmeshes.ConvertVertices(texhashesPerSubMesh, mtsPerMesh, mtBaseInv);


            var dstmeshes = Mesh.AllocateWritableMeshData(1);
            dstmeshes[0].GetVertices()
            var dstmesh = new Mesh();
            Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);
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
        static void AsUIntIndex(this Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
        }
        static void AsUShortIndex(this Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }
    }

    /// <summary>
    /// メッシュを各要素ごとに結合する。
    /// </summary>
    public static partial class MeshCombiner2
	{

        //	/// <summary>
        //	/// Mesh 要素を結合するデリゲートを返す。位置のみ。
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
