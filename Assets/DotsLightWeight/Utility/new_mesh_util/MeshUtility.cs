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

namespace Abarabone.Geometry
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;

    static public class MeshInWorkerThreadUtility
    {

        static public IEnumerable<Mesh.MeshData> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            for(var i = 0; i < meshDataArray.Length; i++)
            {
                yield return meshDataArray[i];
            }
        }


        public struct SubMesh<T> where T : struct
        {
            public SubMesh(int i, SubMeshDescriptor descriptor, IEnumerable<T> submeshElements)
            {
                this.SubMeshIndex = i;
                this.Elements = submeshElements;
                this.Descriptor = descriptor;
            }

            public int SubMeshIndex { get; private set; }
            public SubMeshDescriptor Descriptor { get; private set; }
            public IEnumerable<T> Elements { get; private set; }
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


        static public IEnumerable<SubMesh<T>> VertexPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetVertexData<T>());

        static public IEnumerable<SubMesh<T>> IndexPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetIndexData<T>());


        static public IEnumerable<SubMesh<T>> ElementsInSubMesh<T>(this Mesh.MeshData meshdata, NativeArray<T> srcArray)
            where T : struct
        {
            var i = 0;
            foreach (var desc in meshdata.submeshDescripters_())
            {
                yield return new SubMesh<T>(i++, desc, getElementsInSubmesh_());

                IEnumerable<T> getElementsInSubmesh_()
                {
                    for (var i = desc.firstVertex; i < desc.vertexCount; i++)
                    {
                        yield return srcArray[i];
                    }
                }
            }
        }

        static IEnumerable<SubMeshDescriptor> submeshDescripters_(this Mesh.MeshData meshdata) =>
            from i in 0.Inc(meshdata.subMeshCount)
            select meshdata.GetSubMesh(i)
            ;

        [StructLayout(LayoutKind.Sequential)]
        struct SrcVertexUnit
        {
            public float3 Position;
            public float3 Normal;
            
        }


        
        static IEnumerable<float3> ConvertIndices
            (Mesh.MeshDataArray srcmeshes)
        =>
            from desc in querySubMeshForIndex<int>(srcmeshes)
            from idx in desc.Elements
            select math.transform(submeshdata.mt, id)
            ;

        static IEnumerable<float3> ConvertVertices
            (Mesh.MeshDataArray srcmeshes, IEnumerable<int> texhashPerSubMesh, IEnumerable<Matrix4x4> mtsPerMesh, Matrix4x4 mtBaseInv)
        =>
            from submeshdata in querySubMeshForVertex<float3>(srcmeshes, texhashPerSubMesh, mtsPerMesh, mtBaseInv)
            from vtx in submeshdata.desc.Elements
            select math.transform(submeshdata.mt, vtx)
            ;



        static IEnumerable<(SubMesh<T> desc, int texhash, Matrix4x4 mt)> querySubMeshForVertex<T>
            (Mesh.MeshDataArray srcmeshes, IEnumerable<int> texhashPerSubMesh, IEnumerable<Matrix4x4> mtsPerMesh, Matrix4x4 mtBaseInv) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = mtBaseInv * x.src1
            from xsub in (mesh.VertexPerSubMesh<T>(), texhashPerSubMesh).Zip()
            let desc = xsub.src0
            let texhash = xsub.src1
            select (desc, texhash, mt)
            ;

        static IEnumerable<SubMesh<T>> querySubMeshForIndex<T>
            (Mesh.MeshDataArray srcmeshes) where T : struct
        =>
            from mesh in srcmeshes.AsEnumerable()
            from xsub in mesh.IndexPerSubMesh<T>()
            select xsub
            ;



        static void aaa<TVtx, TIdx>(IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect,
            Func<TVtx,TVtx> vertexConversion)
        {
            var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

            var meshesPerMesh = mmts.Select(x => x.mesh).ToArray();
            var mtxsPerMesh = mmts.Select(x => x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts
                from mat in mmt.mats
                select mat.mainTexture.GetHashCode()
            ).ToArray();

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshesPerMesh);
            var dstmeshes = Mesh.AllocateWritableMeshData(1);

            var mtBaseInv = tfBase.worldToLocalMatrix;





            var qTriSubmesh =

            var dstmesh = new Mesh();
            Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);
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
