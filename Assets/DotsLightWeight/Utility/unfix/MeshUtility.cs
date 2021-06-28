using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Unity.Mathematics;

namespace DotsLite.Geometry
{

	using DotsLite.Common.Extension;
    

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



		public static Mesh CreateGridMesh(int segmentX, int segmentY, float unitDistance)
		{
			var mesh = new Mesh();
			var w = segmentX + 1;
			var h = segmentY + 1;

			var qVtxs =
				from ix in Enumerable.Range(0, w)
				from iz in Enumerable.Range(0, h)
				select new Vector3(ix, math.asfloat(ix + iz * w), iz) * unitDistance
				;
			mesh.SetVertices(qVtxs.ToArray());

			var qIdx =
				from ix in Enumerable.Range(0, w - 1)
				from iz in Enumerable.Range(0, h - 1)
				let i0 = ix + (iz + 0) * w
				let i1 = ix + (iz + 1) * w
				from i in new int[]
				{
					i0+0, i0+1, i1+0,//0, 1, 2,
					i1+0, i0+1, i1+1,//2, 1, 3,
                }
				select i
				;
			mesh.SetIndices(qIdx.ToArray(), MeshTopology.Triangles, 0);

			return mesh;
		}
	}

}
