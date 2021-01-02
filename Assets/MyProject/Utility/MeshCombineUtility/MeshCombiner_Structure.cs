using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abarabone.Geometry//.Editor
{
	using Abarabone.Geometry;
	using Abarabone.Structure.Authoring;

	/// <summary>
	/// メッシュを各要素ごとに結合する。
	/// 
	/// </summary>
	public static partial class MeshCombiner//_Structure
	{

		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。Structure オブジェクト用。
		/// </summary>
		//static public Func<MeshCombinerElements> BuildStructureMeshElements
		//	( IEnumerable<StructurePartAuthoring> parts, Transform tfBase )
		//{
		//	var gameObjects = from part in parts select part.gameObject;
		//	var mmts = FromObject.QueryMeshMatsTransform_IfHaving( gameObjects ).ToArray();

		//	return BuildStructureMeshElements( mmts, tfBase );
		//}


		static public Func<MeshCombinerElements> BuildStructureMeshElements
			(IEnumerable<GameObject> children, Transform tfBase)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(children).ToArray();

			return BuildStructureMeshElements(mmts, tfBase);
		}




		static public Func<MeshCombinerElements> BuildStructureMeshElements
			( (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase )
		{

			var f = MeshCombiner.BuildNormalMeshElements( mmts, tfBase, isCombineSubMeshes:true );


			// 全パーツから、パーツＩＤをすべてクエリする。
			var qPartId_PerMesh =
				from mmt in mmts
				select mmt.tf.gameObject
					.AncestorsAndSelf()
					.Select(x => x.GetComponent<StructurePartAuthoring>())
					.First(x => x != null)
					.PartId
				;
            var partId_PerMesh = qPartId_PerMesh.ToArray();
			//var partId_PerMesh = qPartId_PerMesh.Do(x => Debug.Log(x)).ToArray();


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
