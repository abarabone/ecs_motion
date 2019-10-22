using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

using Abss.Motion;

namespace Abss.Geometry
{

	/// <summary>
	/// SkinMeshRenderer を MeshRenderer に変換する。元のレンダラは破棄される。
	/// </summary>
	public class MeshConvertor : MonoBehaviour
	{

		private void Awake()
		{
			
			var srcmr	= GetComponentInChildren<SkinnedMeshRenderer>();
			var srcmesh	= srcmr.sharedMesh;
			
			var dstmesh = new Mesh();

			dstmesh.vertices	= MeshUtility.ConvertVertices( srcmesh.vertices, srcmesh.boneWeights, srcmesh.bindposes );
			dstmesh.triangles	= srcmesh.triangles;
			dstmesh.normals		= srcmesh.normals;
			dstmesh.uv			= srcmesh.uv;
			dstmesh.bounds		= srcmesh.bounds;

			var dstmr	= srcmr.gameObject.AddComponent<MeshRenderer>();
			var dstmf	= srcmr.gameObject.AddComponent<MeshFilter>();

			dstmr.material	= srcmr.material;
			dstmf.mesh		= dstmesh;
			
			Component.Destroy( srcmr );
		}

	}

	static public class MeshUtility
	{
		/// <summary>
		/// 頂点をトランスフォームし、新しい配列として返す。
		/// </summary>
		static public Vector3[] ConvertVertices( Vector3[] vertices, BoneWeight[] weights, Matrix4x4[] matrices )
		{
			var q =
				from x in Enumerable.Zip( vertices, weights, (v, w) => (v, w) )
				select matrices[ x.w.boneIndex0 ].MultiplyPoint( x.v )
				;

			return q.ToArray();
		}
		
		/// <summary>
		/// メッシュに MotionClip から得たボーン情報を挿入する。
		/// インデックス／バインドポーズは変換後のメッシュ用に再解釈／整列され、Colors と uvChannelForWeight の uv に出力される。
		/// </summary>
		static public Mesh AddBoneInfoFrom( this Mesh mesh, int uvChannelForWeight, MotionClip motionClip )
		{
			var weights		= mesh.boneWeights;
			var bindPoses	= mesh.bindposes;
			var im			= motionClip.IndexMapFbxToMotion;

			var qIndices	= weights
				.Select( x => new Color(im[x.boneIndex0], im[x.boneIndex1], im[x.boneIndex2], im[x.boneIndex3]) );

			var qWeights	= weights
				.Select( x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3) );

			var qBindPoses	=
				from imotion in Enumerable.Range( 0, motionClip.StreamPaths.Length )
				join mapper in im.Select( (imotion, ifbx) => (imotion, ifbx) )
					on imotion equals mapper.imotion
				select bindPoses[ mapper.ifbx ]
				;

			mesh.colors	= qIndices.ToArray();
			mesh.SetUVs( uvChannelForWeight, qWeights.ToList() );
			mesh.bindposes = qBindPoses.ToArray();

			return mesh;
		}

		static public Color32[] CreateBoneIndexColors( BoneWeight[] boneWeights, MotionClip motionClip )
		{
			var im = motionClip.IndexMapFbxToMotion;

			var qIndices = boneWeights
				.Select( x =>
					new Color32()
					{
						r	= (byte)im[x.boneIndex0],
						g	= (byte)im[x.boneIndex1],
						b	= (byte)im[x.boneIndex2],
						a	= (byte)im[x.boneIndex3]
					}
				);

			return qIndices.ToArray();
		}

		static public List<Vector4> CreateBoneWeightUvs( BoneWeight[] boneWeights, MotionClip motionClip )
		{
			var qWeights	= boneWeights
				.Select( x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3) );

			return qWeights.ToList<Vector4>();
		}

		static public Matrix4x4[] CreateReBindPoses( Matrix4x4[] bindPoses, MotionClip motionClip )
		{
			var im = motionClip.IndexMapFbxToMotion;

			var qBindPoses	=
				from imotion in Enumerable.Range( 0, motionClip.StreamPaths.Length )
				join mapper in im.Select( (imotion, ifbx) => (imotion, ifbx) )
					on imotion equals mapper.imotion
				select bindPoses[ mapper.ifbx ]
				;

			return qBindPoses.ToArray();
		}
	}

}
