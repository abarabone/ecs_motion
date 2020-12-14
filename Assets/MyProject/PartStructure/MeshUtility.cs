using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;

namespace Abarabone.Geometry
{

	using Abarabone.Common.Extension;
    

	public static class MeshUtility
	{
		
		static public GameObject AddToNewGameObject( this Mesh mesh, Material mat )
		{
			var go = new GameObject();
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;

			return go;
		}

		static public Mesh ToWriteOnly( this Mesh mesh )
		{
			mesh.UploadMeshData( markNoLongerReadable: true );
			return mesh;
		}
		static public Mesh ToDynamic( this Mesh mesh )
		{
			mesh.MarkDynamic();
			return mesh;
		}


		static public IEnumerable<(int start, int length)> GetSubmeshIndices(this Mesh mesh) =>
			from i in Enumerable.Range(0, mesh.subMeshCount)
			let submesh = mesh.GetSubMesh(i)
			select (fst: submesh.firstVertex, length: submesh.vertexCount)
			;

		static public IEnumerable<IEnumerable<Vector3>> SubmeshVertices(this Mesh mesh)
        {
			var vtxs = mesh.vertices;
			foreach(var isub in mesh.GetSubmeshIndices())
			{
				yield return getVerticesInSubmesh_();

				IEnumerable<Vector3> getVerticesInSubmesh_()
				{
					for (var i = isub.start; i < isub.length; i++)
					{
						yield return vtxs[i];
					}
				}
			}
		}
		static public IEnumerable<IEnumerable<T>> SubmeshVertices<T>(this Mesh mesh, T[] srcs)
		{
			foreach (var isub in mesh.GetSubmeshIndices())
			{
				yield return getVerticesInSubmesh_();

				IEnumerable<T> getVerticesInSubmesh_()
				{
					for (var i = isub.start; i < isub.length; i++)
					{
						yield return srcs[i];
					}
				}
			}
		}

		static public Vector2 ScaleUv(this Vector2 uv, Rect rect) =>
			new Vector2
			{
				x = rect.x + uv.x * rect.width,
				y = rect.y + uv.y * rect.height,
			};

		//static public IEnumerable<(int i0, int i1, int i2)> AsTriangleTupple( this int[] indexes )
		//{
		//	for( var i=0; i<indexes.Length; i+=3 )
		//	{
		//		yield return ( indexes[i+0], indexes[i+1], indexes[i+2] );
		//	}
		//}

	}

}
