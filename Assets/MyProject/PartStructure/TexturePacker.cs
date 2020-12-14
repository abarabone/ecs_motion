//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;

//namespace Abarabone.Geometry
//{

//	using Abarabone.Common.Extension;


//	public static class TexturePacker
//	{

//		/// <summary>
//		/// 全テクスチャを結合して、ソースとなったオブジェクトのＵＶと材質を差し替える。
//		/// 材質は、元同じだったもの同士は、新しい材質でも同じものを共有する。
//		/// </summary>
//		public static void PackTextureAndTranslateUv( IEnumerable<GameObject> targetObjects )
//		{

//			var mmts = FromObject.QueryMeshMatsTransform_IfHaving( targetObjects ).ToArray();


//			// 重複のない材質配列を生成。
//			var dstMats = (from x in mmts select x.mats)
//				.To(MaterialCombined.QueryCombine)
//				.ToArray()
//				;
			
//			// テクスチャを結合する。
//			var (dstTexture, uvRects) = packTexture_( dstMats );


//			// ＵＶ位置→頂点数情報割り当て→ＵＶ配列生成→メッシュＵＶに割り当て
//			uvRects
//				.To( x => queryJoinUvRectToVtxInfo_PerSubPerMesh_(x, mmts, dstMats) )
//				.To( x => queryCreateNewUvs_PerMesh_(x, mmts) )
//				.To( x => translateUvsInMeshes_(x, mmts) )
//				;
			
//			// 全材質を新テクスチャー材質に差し替える。
//			setNewMats_( dstMats, dstTexture, mmts );

//			return;
			


//			(Texture, Rect[]) packTexture_( IEnumerable<Material> qDstMats_ )
//			{
//				var texs = (from mat in dstMats select mat.mainTexture).Cast<Texture2D>().ToArray();
				
//				var dstTexture_ = new Texture2D( width:0, height:0, textureFormat:TextureFormat.ARGB32, mipChain:true );
//				//var dstTexture_ = new Texture2D( 0, 0 );
//				var uvRects_ = dstTexture_.PackTextures
//					( texs, padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true );

//				return (dstTexture_, uvRects_);
//			}


//			IEnumerable<IEnumerable<(Rect rect, int vtxCount, int baseVtx)>>
//				queryJoinUvRectToVtxInfo_PerSubPerMesh_
//				(
//					Rect[] uvRects_,
//					IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts_,
//					IEnumerable<Material> qDstMats_
//				)
//			{

//				// 結合のため、uvrect に材質ハッシュを割り振る。
//				var qRectAndHash_PerDstMat =
//					from xy in (uvRects_, qDstMats_).Zip()
//					select (rect: xy.x, hash: xy.y.GetHashCode())
//					;


//				// サブメッシュごとのベース頂点・頂点数を取得する。また、結合用にハッシュも取得する。

//				var qVtxCountss = (from x in mmts_ select x.mesh)
//					.To(PerSubMeshPerMesh.QueryVertexCount);

//				var qBaseVtxss =
//					from x in mmts_
//					select
//						from isub in Enumerable.Range(0, x.mesh.subMeshCount)
//						select (int)x.mesh.GetBaseVertex(isub)
//					;

//				var qHashess = (from x in mmts_ select x.mats)
//					.To(PerSubMeshPerMesh.QueryMaterialHash);


//				// 結合する。
//				var qUvInfo_PerSubPerMesh =
//					from perMesh in (qVtxCountss, qBaseVtxss, qHashess).Zip()
//					select
//						from perSub in perMesh.Zip()
//						select (vtxCount: perSub.x, baseVtx: perSub.y, hash: perSub.z) into perSub
//						join mat in qRectAndHash_PerDstMat on perSub.hash equals mat.hash
//						select (mat.rect, perSub.vtxCount, perSub.baseVtx)
//					;

//				return qUvInfo_PerSubPerMesh;
//			}


//			IEnumerable<IEnumerable<Vector2>> queryCreateNewUvs_PerMesh_
//				(
//					IEnumerable<IEnumerable<(Rect rect, int vtxCount, int baseVtx)>> qUvInfo_PerSubPerMesh_,
//					IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts_
//				)
//			{
//				var qUvs_PerMesh = from x in mmts_ select x.mesh.uv;

//				var qUvsTranslated_PerMesh =
//					from perMesh in (qUvs_PerMesh, qUvInfo_PerSubPerMesh_).Zip()
//					select (uvs: perMesh.x, info: perMesh.y) into perMesh
//					select
//						from infoPerSub in perMesh.info
//						from iuv in Enumerable.Range( infoPerSub.baseVtx, infoPerSub.vtxCount )

//						let rect = infoPerSub.rect
//						let uv = perMesh.uvs[iuv]
						
//						select new Vector2
//						{
//							x = rect.x + uv.x * rect.width,
//							y = rect.y + uv.y * rect.height,
//						}
//					;
				
//				return qUvsTranslated_PerMesh;
//			}


//			void translateUvsInMeshes_
//				(
//					IEnumerable<IEnumerable<Vector2>> qUvsTranslated_PerMesh_,
//					IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts_
//				)
//			{
//				var qMeshTraverse =
//					from xy in (from x in mmts_ select x.tf, qUvsTranslated_PerMesh_).Zip()
//					select (dstTf: xy.x, srcUvs: xy.y)
//					;

//				foreach( var perMesh in qMeshTraverse )
//				{
//					var smr = perMesh.dstTf.GetComponent<SkinnedMeshRenderer>().As();
//					var mf = perMesh.dstTf.GetComponent<MeshFilter>().As();
					
//					var mesh = smr?.sharedMesh ?? mf?.mesh;
//					//mesh.SetUVs( 0, perMesh.srcUvs.ToList() );
//					mesh.uv = perMesh.srcUvs.ToArray();

//					if( smr != null ) smr.sharedMesh = mesh;
//					if( mf != null ) mf.mesh = mesh;
//				}
//			}

//			void setNewMats_
//				(
//					Material[] materials_, Texture texturePacked_,
//					IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts_
//				)
//			{
//				var newMatDict = materials_
//					.Select( mat => (oldmat:mat, newmat:new Material(mat)) )
//					.Do( x => x.newmat.mainTexture = texturePacked_ )
//					.ToDictionary( x => x.oldmat, x => x.newmat )
//					;
				
//				foreach( var tf in mmts.Select( mmt => mmt.tf ) )
//				{
//					var smr = tf.GetComponent<SkinnedMeshRenderer>().As();
//					var mr = tf.GetComponent<MeshRenderer>().As();
					
//					if( smr != null ) smr.sharedMaterials =
//							( from mat in smr.sharedMaterials select newMatDict[mat] ).ToArray();
//					if( mr != null ) mr.materials = 
//							( from mat in mr.sharedMaterials select newMatDict[mat] ).ToArray();
//				}
//			}
			
//		}

//	}
//}

