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

	/// <summary>
	/// メッシュを各要素ごとに結合する。
	/// </summary>
	public static partial class MeshCombiner
	{

		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置のみ。
		/// </summary>
		static public Func<MeshCombinerElements> BuildBaseMeshElements
			(IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

			return BuildBaseMeshElements(mmts, tfBase, isCombineSubMeshes);
		}

		static public Func<MeshCombinerElements> BuildBaseMeshElements
			((Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes)
		{

			var vtxss = (from x in mmts select x.mesh.vertices).ToArray();

			var idxsss = (from x in mmts select x.mesh)
				.To(PerSubMeshPerMesh.QueryIndices).ToArrayRecursive2();

			var mtBaseInv = tfBase.worldToLocalMatrix;
			var mtObjects = (from x in mmts select x.tf.localToWorldMatrix).ToArray();
			//Debug.Log( string.Join(";",mmts.SelectMany(x=>x.mats).Select(x=>$"{x.name} {x.GetHashCode()}")) );

			return () =>
			{
				var materialsCombined = (from x in mmts select x.mats)
					.To(MaterialCombined.QueryCombine).ToArray();

				return new MeshCombinerElements
				{
					Vertecies = vtxss.ToVerticesArray(mtObjects, mtBaseInv),

					IndicesPerSubmesh = isCombineSubMeshes
						? ConvertUtility.ToIndicesArray(vtxss, idxsss, mtObjects)
						: ConvertUtility.ToIndicesArray(vtxss, idxsss, mtObjects, (from x in mmts select x.mats), materialsCombined),

					MtBaseInv = mtBaseInv,
					mtObjects = mtObjects,

					materials = materialsCombined,
				};
			};
		}

	}
}
