using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abarabone.Geometry.Editor
{
	using Abarabone.Geometry;
	using Abarabone.Structure.Authoring;

	/// <summary>
	/// メッシュを各要素ごとに結合する。
	/// 
	/// </summary>
	public static class MeshCombiner_Structure
	{

		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。Structure オブジェクト用。
		/// </summary>
		static public Func<MeshElements> BuildStructureWithPalletMeshElements
			( IEnumerable<StructurePartAuthoring> parts, Transform tfBase )
		{
			var gameObjects = from part in parts select part.gameObject;
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToArray();

			return BuildStructureWithPalletMeshElements( mmts, tfBase );
		}

		static public Func<MeshElements> BuildStructureWithPalletMeshElements
			( (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase )
		{

			var f = MeshCombiner.BuildNormalMeshElements( mmts, tfBase, isCombineSubMeshes:true );
			
			
			// 全パーツから、パーツＩＤをすべてクエリする。
			//var partId_PerMesh = ( from x in mmts select x.tf.GetComponent<StructurePartAuthoring>().PartId ).ToArray();
			var partId_PerMesh = Enumerable.Range(0, mmts.Length).ToArray();

			// サブメッシュ単位で、頂点数を取得。
			var vertexCount_PerSubmeshPerMesh =
				( from x in mmts select x.mesh ).To(PerSubMeshPerMesh.QueryVertexCount).ToArrayRecursive2();
			

			return () =>
			{
				var me = f();
				
				var materials_PerMesh = ( from x in mmts select x.mats );
				var materialsCombined = me.materials;
				me.Color32s = ConvertUtility.ToColor32Array
					( vertexCount_PerSubmeshPerMesh, partId_PerMesh, materials_PerMesh, materialsCombined );

				return me;
			};
			
		}

	}

}
