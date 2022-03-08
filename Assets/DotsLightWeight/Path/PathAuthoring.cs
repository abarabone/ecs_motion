using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using Unity.Linq;
using Unity.Physics.Authoring;

namespace DotsLite.LoadPath.Authoring
{
	using DotsLite.Structure.Authoring;


	// ・
	// ・コライダによる地形フィットではなく、パスと幅によるフィットのほうがいいかも？
	// ・

	// ・地形フィット　コライダ？パスと幅？
	// ・メッシュを地形にフィット　テッセレート
	// ・メッシュはパス変形させないこともできる　パーツ単位？
	// ・


	/// <summary>
	/// 
	/// </summary>
	public class PathAuthoring : MonoBehaviour
	{

		public Transform StartAnchor;
		public bool IsReverseStart;
		public Transform EndAnchor;
		public bool IsReverseEnd;

		public int Frequency;

		public GameObject ModelTopPrefab;
		public GameObject LevelingColliderPrefab;


		public float PathWidthForTerrainFit;
		public bool UseUpInterpolationForTerrainFit;


		void Awake()
		{
			
		}

		public void BuildPathMeshes()
		{
			removeChildren_();
			var conv = createConvertor_();
			createPartSegments_(conv);
			//createColliderSegments_(conv);
			return;

			void removeChildren_()
            {
				var children = this.gameObject.Children().ToArray();
				children.ForEach(go => DestroyImmediate(go));
            }
			PathMeshConvertor createConvertor_()
            {
				//var maxZ = this.LevelingColliderPrefab.GetComponent<MeshCollider>()
				var maxZ = this.LevelingColliderPrefab.GetComponent<MeshFilter>()
					.sharedMesh
					.vertices
					.Max(v => v.z);
				var conv = new PathMeshConvertor(
					this.StartAnchor, this.IsReverseStart, this.EndAnchor, this.IsReverseEnd,
					this.Frequency, maxZ);
				return conv;
            }
			void createPartSegments_(PathMeshConvertor conv)
			{
				var tfSegment = this.transform;
				var mtSegInv = (float4x4)tfSegment.worldToLocalMatrix;
				var mtStInv = (float4x4)this.StartAnchor.transform.worldToLocalMatrix;
				var q =
					from i in Enumerable.Range(0, this.Frequency)
					let tfSegtop = instantiate_()
					from mf in tfSegtop.GetComponentsInChildren<MeshFilter>()
					let tf = mf.transform
					let mtinv = (float4x4)tf.worldToLocalMatrix
					let pos = tf.position//math.transform(mtStInv, tf.position)
					let front = pos + tf.forward//math.transform(mtStInv, tf.forward)
					select (tfSegtop, mf, i, (mtinv, pos, front))
					;
				foreach (var x in q.ToArray())
				{
                    var (tfSegtop, mf, i, pre) = x;

                    var tfChild = mf.transform;
					var pos = conv.CalculateBasePoint(i, pre.pos, float4x4.identity);
                    tfChild.position = pos;

                    Debug.Log($"{i} mesh:{mf.name} top:{tfSegtop.name} {pos} {pre.pos}");

                    if (mf.GetComponent<WithoutShapeTransforming>() != null)
                    {
						//var forward = conv.CalculateBasePoint(i, pre.front, mtSegInv) - pos;
						//tfChild.forward = forward;
						var front = conv.CalculateBasePoint(i, pre.front, float4x4.identity);
						tfChild.LookAt(front, Vector3.up);
                        continue;
                    }

					var newmesh = conv.BuildMesh(i, mf.sharedMesh, tfSegtop.worldToLocalMatrix);
                    mf.sharedMesh = newmesh;

                    // コライダー以外のメッシュはそのまま維持
                    // 暫定で常に外形メッシュと同じものをセット
                    var col = mf.GetComponent<PhysicsShapeAuthoring>();
                    if (col != null && col.ShapeType == ShapeType.Mesh)
                    {
                        col.SetMesh(newmesh);
                    }
                }
				Transform instantiate_()
				{
					var tf = Instantiate(this.ModelTopPrefab).transform;
					tf.SetParent(tfSegment, worldPositionStays: true);
					return tf;
				}
			}
		}


		public void FitTerrainToPath()
        {
			var tf = this.transform;
			var meshes = getMeshesFromColliderOrMeshFilter_();
			foreach (var mesh in meshes)
            {
				//mesh.
            }
			return;


			(Transform tf, Mesh mesh)[] getMeshesFromColliderOrMeshFilter_()
			{
				var mfs = this.GetComponentsInChildren<MeshFilter>();
				var mcs = this.GetComponentsInChildren<MeshCollider>();

				var dict = mfs.ToDictionary(x => x.gameObject, x => (x.transform, x.sharedMesh));
				mcs.ForEach(x => dict[x.gameObject] = (x.transform, x.sharedMesh));

				return dict.Values.ToArray();
			}
        }

		public void FitPathToTerrain()
        {

        }

	}

}
