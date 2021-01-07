using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;

namespace Abarabone.Geometry
{
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
            public SubMesh(int i, IEnumerable<T> submeshVertexData)
            {
                this.VertexData = submeshVertexData;
                this.SubMeshIndex = i;
            }

            public int SubMeshIndex { get; private set; }
            public IEnumerable<T> VertexData { get; private set; }
        }

        static public IEnumerable<SubMesh<T>> VertexSubMeshes<T>(this Mesh.MeshData meshdata)
            where T:struct
        {
            var vtxs = meshdata.GetVertexData<T>();

            var i = 0;
            foreach (var range in meshdata.getSubmeshVertexRanges_())
            {
                yield return new SubMesh<T>(i++, getVerticesInSubmesh_());

                IEnumerable<T> getVerticesInSubmesh_()
                {
                    for (var i = range.start; i < range.length; i++)
                    {
                        yield return vtxs[i];
                    }
                }
            }
		}

		static IEnumerable<(int start, int length)> getSubmeshVertexRanges_(this Mesh.MeshData meshdata) =>
            from i in 0.UpTo(meshdata.subMeshCount)
            let submeshDescriptor = meshdata.GetSubMesh(i)
            select (fst: submeshDescriptor.firstVertex, length: submeshDescriptor.vertexCount)
            ;


        static public IEnumerable<SubMesh<T>> IndexSubMeshes<T>(this Mesh.MeshData meshdata)
            where T : struct
        {
            var idxs = meshdata.GetIndexData<T>();

            var i = 0;
            foreach (var range in meshdata.getSubmeshIndexRanges_())
            {
                yield return new SubMesh<T>(i++, getIndicesInSubmesh_());

                IEnumerable<T> getIndicesInSubmesh_()
                {
                    for (var i = range.start; i < range.length; i++)
                    {
                        yield return idxs[i];
                    }
                }
            }
        }

        static IEnumerable<(int start, int length)> getSubmeshIndexRanges_(this Mesh.MeshData meshdata) =>
            from i in 0.UpTo(meshdata.subMeshCount)
            let submeshDescriptor = meshdata.GetSubMesh(i)
            select (fst: submeshDescriptor.indexStart, length: submeshDescriptor.indexCount)
            ;

        static IEnumerable<IEnumerable<int>> triangle_(IEnumerable<int> indecies_)
        {
            using (var e = indecies_.GetEnumerator())
            {
                for (; ; )
                {
                    yield return tri_();

                    IEnumerable<int> tri_()
                    {
                        yield return e.Current;
                        if (!e.MoveNext()) yield break;
                        yield return e.Current;
                        if (!e.MoveNext()) yield break;
                        yield return e.Current;
                        if (!e.MoveNext()) yield break;
                    }
                }
            }
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
