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

			var qVtx =
				from iz in Enumerable.Range(0, h)
				from ix in Enumerable.Range(0, w)
				let heightIndex = ix + iz * w
				let hi = math.asfloat(heightIndex)
				select new Vector3(ix, hi, iz) * unitDistance
				;
			mesh.SetVertices(qVtx.ToArray());

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
		public static Mesh CreateSlantHalfGridMesh(int segmentX, int segmentY, float unitDistance)
		{
			var mesh = new Mesh();
			var w = segmentX + 1;
			var h = segmentY + 1;
			var unit = unitDistance;
			var half = unit * 0.5f;

			int toindex_(int ix, int iz) => ix + iz * w;
			static int pack2_(int u, int d) => (u << 0) | (d << 16);
			static int pack1to2_(int index) => (index << 0) | (index << 16);

			var qVtxLine =
				from iz in Enumerable.Range(0, h)
				select
					from ix in Enumerable.Range(0, w)
					let heightIndex = toindex_(ix, iz)
					let hi = pack1to2_(heightIndex)
					let fhi = math.asfloat(hi)
					select new Vector3(ix, fhi, iz) * unit
				;
			var qVtxHalf =
				from iz in Enumerable.Range(0, segmentY)
				select
					from ix in Enumerable.Range(0, segmentX)
					let heightIndex0 = toindex_(ix + 0, iz + 0)
					let heightIndex2 = toindex_(ix + 0, iz + 1)
					let hi = pack2_(heightIndex0, heightIndex2)
					let fhi = math.asfloat(hi)
					select new Vector3(half + ix, fhi, half + iz) * unit
				;
			var qVtx = qVtxLine.First().Concat(
					(qVtxHalf, qVtxLine.Skip(1)).Zip().SelectMany(x => x.src0.Concat(x.src1))
				);

			//qVtx.Select(x => new float3(x))
			//	.Select(x => (x.xz, i: math.asint(x.y)))
			//	.ForEach(x => Debug.Log($"{x.xz} {x.i & 0xffff} {(x.i >> 16) & 0xffff}"));
			mesh.SetVertices(qVtx.ToArray());

			var qIdx =
				from ix in Enumerable.Range(0, w - 1)
				from iz in Enumerable.Range(0, h - 1)
				let i0 = ix + (iz + 0) * (w + segmentX)
				let ihf = i0 + w
				let i1 = ix + (iz + 1) * (w + segmentX)
				from i in new int[]
				{
					i0+0, i0+1, ihf,
					i0+0, ihf, i1+0,
					ihf, i0+1, i1+1,
					i1+0, ihf, i1+1,
				}
				select i
				;
			mesh.SetIndices(qIdx.ToArray(), MeshTopology.Triangles, 0);

			return mesh;
		}
		//public static Mesh CreateSlantHalfGridMesh(int segmentX, int segmentY, float unitDistance)
		//{
		//	var mesh = new Mesh();
		//	var w = segmentX + 1;
		//	var h = segmentY + 1;
		//	var unit = unitDistance;
		//	var half = unit * 0.5f;

		//	int toindex_(int ix, int iz) => ix + iz * w;
		//	static int pack4_(int ul, int ur, int dl, int dr) => (ul << 0) | (ur << 8) | (dl << 16) | (dr << 24);
		//	static int pack1to4_(int index) => (index << 0) | (index << 8) | (index << 16) | (index << 24);

		//	var qVtxLine =
		//		from iz in Enumerable.Range(0, h)
		//		select
		//			from ix in Enumerable.Range(0, w)
		//			let heightIndex = toindex_(ix, iz)
		//			let hi4 = pack1to4_(heightIndex)
		//			let fhi = math.asfloat(hi4)
		//			select new Vector3(ix, fhi, iz) * unit
		//		;
		//	var qVtxHalf =
		//		from iz in Enumerable.Range(0, segmentY)
		//		select
		//			from ix in Enumerable.Range(0, segmentX)
		//			let heightIndex0 = toindex_(ix + 0, iz + 0)
		//			let heightIndex1 = toindex_(ix + 1, iz + 0)
		//			let heightIndex2 = toindex_(ix + 0, iz + 1)
		//			let heightIndex3 = toindex_(ix + 1, iz + 1)
		//			let hi4 = pack4_(heightIndex0, heightIndex1, heightIndex2, heightIndex3)
		//			let fhi = math.asfloat(hi4)
		//			select new Vector3(half + ix, fhi, half + iz) * unit
		//		;
		//	var qVtx = qVtxLine.First().Concat(
		//			(qVtxHalf, qVtxLine.Skip(1)).Zip().SelectMany(x => x.src0.Concat(x.src1))
		//		);
		//	mesh.SetVertices(qVtx.ToArray());

		//	var qIdx =
		//		from ix in Enumerable.Range(0, w - 1)
		//		from iz in Enumerable.Range(0, h - 1)
		//		let i0 = ix + (iz + 0) * (w + segmentX)
		//		let ihf = i0 + w
		//		let i1 = ix + (iz + 1) * (w + segmentX)
		//		from i in new int[]
		//		{
		//			i0+0, i0+1, ihf,
		//			i0+0, ihf, i1+0,
		//			ihf, i0+1, i1+1,
		//			i1+0, ihf, i1+1,
		//		}
		//		select i
		//		;
		//	mesh.SetIndices(qIdx.ToArray(), MeshTopology.Triangles, 0);

		//	return mesh;
		//}
	}

}
