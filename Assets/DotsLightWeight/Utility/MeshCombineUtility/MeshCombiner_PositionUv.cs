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
	/// </summary>
	public static partial class MeshCombiner
	{


		/// <summary>
		/// Mesh 要素を結合するデリゲートを返す。位置とＵＶのみ。
		/// </summary>
		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			(this IEnumerable<GameObject> gameObjects,
			Transform tfBase, bool isCombineSubMeshes = true, Dictionary<int, Rect> texToUvRect = null)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

			return texToUvRect == null
				? BuildUnlitMeshElements(mmts, tfBase, isCombineSubMeshes)
				: BuildUnlitMeshElements(mmts, tfBase, isCombineSubMeshes, texToUvRect)
				;
		}


		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			((Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes)
		{
			var f = BuildBaseMeshElements(mmts, tfBase, isCombineSubMeshes);

			var uvss = (from x in mmts select x.mesh.uv).ToArray();

			return () =>
			{
				var me = f();

				me.Uvs = uvss.SelectMany(uvs => uvs).ToArray();

				return me;
			};
		}

		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			((Mesh mesh, Material[] mats, Transform tf)[] mmts,
			Transform tfBase, bool isCombineSubMeshes, Dictionary<int, Rect> texToUvRect)
		{
			var f = BuildBaseMeshElements(mmts, tfBase, isCombineSubMeshes);

			var uvsrc = (mmts.Select(x => x.mesh), mmts.Select(x => x.mats)).ToUvTranslateSource();

			return () =>
			{
				var me = f();

				me.Uvs = uvsrc.QueryTranslatedUv(texToUvRect).SelectMany(x => x).ToArray();

				return me;
			};
		}


	}
}
