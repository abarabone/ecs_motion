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
	/// Mesh 要素を格納する。並列処理させた結果を格納し、最後に Mesh を作成するために使用する。
	/// </summary>
	public struct MeshCombinerElements
	{
		// Mesh 要素
		public Vector3[] Vertecies;
		public Vector3[] Normals;
		public Vector2[] Uvs;
		public int[][] IndicesPerSubmesh;
		public Vector3[] Tangents;
		public Color32[] Color32s;

		// 間接的に必要な要素
		public Matrix4x4[] mtObjects;
		public Matrix4x4 MtBaseInv;

		// 結合後の材質（
		public Material[] materials;


		public Mesh CreateMesh()
		{
			var mesh = new Mesh();

			if (this.Vertecies != null) mesh.vertices = this.Vertecies;
			if (this.Normals != null) mesh.normals = this.Normals;
			if (this.Uvs != null) mesh.uv = this.Uvs;
			if (this.Color32s != null) mesh.colors32 = this.Color32s;
			if (this.IndicesPerSubmesh != null)
			{
				mesh.subMeshCount = this.IndicesPerSubmesh.Length;
				foreach (var x in this.IndicesPerSubmesh.Select((idxs, i) => (idxs, i)))
				{
					mesh.SetTriangles(x.idxs, submesh: x.i, calculateBounds: true);
				}
			}
			//if(this.Color32s != null) this.Color32s.ForEach(x=>Debug.Log($"cm {x}"));
			//if( this.Normals != null ) mesh.RecalculateNormals();//
			mesh.RecalculateBounds();
			return mesh;
		}
	}

}
