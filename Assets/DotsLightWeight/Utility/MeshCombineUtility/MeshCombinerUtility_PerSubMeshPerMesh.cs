using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abarabone.Geometry
{

	/// <summary>
	/// 
	/// </summary>
	public static class PerSubMeshPerMesh
	{

		/// <summary>
		/// メッシュ、サブメッシュごとにインデックス配列をクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<int[]>> QueryIndices(IEnumerable<Mesh> meshes)
		{
			return
				from mesh in meshes
				select
					from isubmesh in Enumerable.Repeat(0, mesh.subMeshCount)
					select mesh.GetTriangles(isubmesh)
				;
		}

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<IEnumerable<int>> QueryMaterialHash(IEnumerable<Material[]> materials_PerMesh)
		{
			return
				from mats in materials_PerMesh
				select
					from mat in mats
					select mat.GetHashCode()
				;
		}


		/// <summary>
		/// メッシュ、サブメッシュごとの頂点数をクエリする。
		/// </summary>
		public static IEnumerable<IEnumerable<int>> QueryVertexCount(IEnumerable<Mesh> meshes)
		{
			return
				from mesh in meshes
				select
					from isubmesh in Enumerable.Range(0, mesh.subMeshCount)
					select calculateVertexCount_(isubmesh, mesh)
				;

			int calculateVertexCount_(int isubmesn_, Mesh mesh_)
			{
				bool isLastSubmesh_() => (isubmesn_ == mesh_.subMeshCount - 1);

				var pre =
					(int)mesh_.GetBaseVertex(isubmesn_ + 0);
				var cur = isLastSubmesh_()
					? mesh_.vertexCount
					: (int)mesh_.GetBaseVertex(isubmesn_ + 1);

				return cur - pre;
			}
		}

	}

}
