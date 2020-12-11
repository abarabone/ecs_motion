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
	/// メッシュを各要素ごとに結合する。
	/// 
	/// </summary>
	public static partial class MeshCombiner
	{

		
		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶのみ。
		/// </summary>
		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			( IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true )
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToArray();

			return BuildUnlitMeshElements( mmts, tfBase, isCombineSubMeshes );
		}

		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			( (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes )
		{
			var f = BuildBaseMeshElements( mmts, tfBase, isCombineSubMeshes );
			
			var uvss = ( from x in mmts select x.mesh.uv ).ToArray();
			
			return () =>
			{
				var me = f();

				me.Uvs = uvss.SelectMany( uvs => uvs ).ToArray();

				return me;
			};
		}
		

		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶと法線。
		/// </summary>
		static public Func<MeshCombinerElements> BuildNormalMeshElements
			( IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true )
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToArray();

			return BuildNormalMeshElements( mmts, tfBase, isCombineSubMeshes );
		}

		static public Func<MeshCombinerElements> BuildNormalMeshElements
			( (Mesh mesh,Material[] mats,Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes )
		{
			var f = BuildUnlitMeshElements( mmts, tfBase, isCombineSubMeshes );
			
			var nmss = ( from x in mmts select x.mesh ).To(PerMesh.QueryNormals).ToArray();
			
			return () =>
			{
				var me = f();

				me.Normals = ConvertUtility.ToNormalsArray( nmss, me.mtObjects, me.MtBaseInv );

				return me;
			};
		}

	}
	
}
