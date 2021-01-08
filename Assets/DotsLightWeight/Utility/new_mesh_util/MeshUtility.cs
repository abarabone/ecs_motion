using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;

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
            public SubMesh(int i, IEnumerable<T> submeshElements)
            {
                this.SubMeshIndex = i;
                this.Elements = submeshElements;
            }

            public int SubMeshIndex { get; private set; }
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


        static public IEnumerable<SubMesh<T>> VertexInSubMeshes<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetVertexData<T>(), (submesh, vtx) => vtx);

        static public IEnumerable<SubMesh<T>> IndexInSubMeshes<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.ElementsInSubMesh(meshdata.GetIndexData<T>(), (submesh, idx) => idx + submesh.baseVertex);


        //static public IEnumerable<SubMesh<int>> IndicesInSubMesh2(this Mesh.MeshData meshdata, NativeArray<int> srcArray)
        //{
        //    var i = 0;
        //    foreach (var desc in meshdata.submeshDescripters_())
        //    {
        //        yield return new SubMesh<int>(i++, getElementsInSubmesh_());

        //        IEnumerable<int> getElementsInSubmesh_()
        //        {
        //            for (var i = desc.indexStart; i < desc.indexCount; i++)
        //            {
        //                yield return srcArray[i] + desc.baseVertex;
        //            }
        //        }
        //    }
        //}

        static public IEnumerable<SubMesh<T>> ElementsInSubMesh<T>
            (this Mesh.MeshData meshdata, NativeArray<T> srcArray, Func<SubMeshDescriptor,T,T> conversion) where T : struct
        {
            var i = 0;
            foreach (var desc in meshdata.submeshDescripters_())
            {
                yield return new SubMesh<T>(i++, getElementsInSubmesh_());

                IEnumerable<T> getElementsInSubmesh_()
                {
                    for (var i = desc.firstVertex; i < desc.vertexCount; i++)
                    {
                        yield return conversion(desc, srcArray[i]);
                    }
                }
            }
        }

        static IEnumerable<SubMeshDescriptor> submeshDescripters_(this Mesh.MeshData meshdata) =>
            from i in 0.UpTo(meshdata.subMeshCount)
            select meshdata.GetSubMesh(i)
            ;

        //static IEnumerable<(int start, int length)> getSubmeshVertexRanges_(this Mesh.MeshData meshdata) =>
        //    from i in 0.UpTo(meshdata.subMeshCount)
        //    let submeshDescriptor = meshdata.GetSubMesh(i)
        //    select (start: submeshDescriptor.firstVertex, length: submeshDescriptor.vertexCount)
        //    ;

        //static IEnumerable<(int start, int length)> getSubmeshIndexRanges_(this Mesh.MeshData meshdata) =>
        //    from i in 0.UpTo(meshdata.subMeshCount)
        //    let submeshDescriptor = meshdata.GetSubMesh(i)
        //    select (start: submeshDescriptor.indexStart, length: submeshDescriptor.indexCount)
        //    ;


        static void aaa(IEnumerable<GameObject> gameObjects, Transform tfBase)
        {
            var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

            var meshes = mmts.Select(x => x.mesh).ToArray();

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshes);
            var dstmesh = Mesh.AllocateWritableMeshData(1)[0];

            var qTri =
                from mesh in srcmeshes.AsEnumerable()
                from submesh in mesh.IndexInSubMeshes<int>()
                from tri in submesh.Elements.AsTriangle()
                select tri
                ;
            var qVtx =
                from mesh in srcmeshes.AsEnumerable()
                from submesh in mesh.VertexInSubMeshes<Vector3>()
                from vtx in submesh.Elements
                select vtx
                ;
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
