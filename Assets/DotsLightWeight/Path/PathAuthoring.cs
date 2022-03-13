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
            createColliderSegments_(conv);
            return;

			void removeChildren_()
            {
				var children = this.gameObject.Children().ToArray();
				children.ForEach(go => DestroyImmediate(go));
            }
			PathMeshConvertor createConvertor_()
            {
                var maxZ = this.ModelTopPrefab.GetComponent<MeshCollider>()
                    //var maxZ = this.LevelingColliderPrefab.GetComponent<MeshFilter>()
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
					select (i, tfSegtop, mf, (mtinv, pos, front))
					;
				foreach (var x in q.ToArray())
				{
                    var (i, tfSegtop, mf, pre) = x;
					var tfChild = mf.transform;

                    Debug.Log($"{i} mesh:{mf.name} top:{tfSegtop.name}");

                    switch (useMeshDeforming_())
                    {
                        case true:
                            setPosision_();
                            var mesh = buildMesh_();
                            setDrawMesh_(mesh);
                            setDotsColliderMesh_(mesh);
                            break;

                        case false:
                            setPosision_();
                            setDirection_();
                            break;
                    }

                    continue;


					void setPosision_()
					{
						var pos = conv.CalculateBasePoint(i, pre.pos, float4x4.identity);
						tfChild.position = pos;
					}

					bool useMeshDeforming_() =>
						!mf.GetComponentInParent<StructureAreaPartAuthoring>().DoNotPathDeform;

					void setDirection_()
					{
						var front = conv.CalculateBasePoint(i, pre.front, float4x4.identity);
						tfChild.LookAt(front, Vector3.up);
					}

					Mesh buildMesh_() =>
						conv.BuildMesh(i, mf.sharedMesh, tfSegtop.worldToLocalMatrix);

                    void setDrawMesh_(Mesh newmesh)
					{
						mf.sharedMesh = newmesh;
					}

					void setDotsColliderMesh_(Mesh newmesh)
					{
						var col = mf.GetComponent<PhysicsShapeAuthoring>();
						if (col == null || col.ShapeType != ShapeType.Mesh) return;

						col.SetMesh(newmesh);
					}
                }
				Transform instantiate_()
				{
					var tf = Instantiate(this.ModelTopPrefab).transform;
					tf.SetParent(tfSegment, worldPositionStays: true);
					GameObject.DestroyImmediate(tf.GetComponent<MeshCollider>());
					return tf;
				}
			}

            void createColliderSegments_(PathMeshConvertor conv)
            {
                var tfSegment = this.transform;
                var mtSegInv = (float4x4)tfSegment.worldToLocalMatrix;
                var mtStInv = (float4x4)this.StartAnchor.transform.worldToLocalMatrix;
                var q =
                    from i in Enumerable.Range(0, this.Frequency)
                    let tfSegtop = instantiate_()
                    let srccollider = this.ModelTopPrefab.GetComponent<MeshCollider>()
                    let collider = tfSegtop.gameObject.AddComponent<MeshCollider>()
                    select (i, tfSegtop, collider, srccollider, tfSegtop.position)
                    ;
                foreach (var x in q.ToArray())
                {
                    var (i, tfSegtop, collider, srccollider, prepos) = x;
                    var tfChild = collider.transform;

                    setPosision_();
                    var mesh = buildMesh_();
                    setColliderMesh_(mesh);
                    continue;


                    void setPosision_()
                    {
                        var pos = conv.CalculateBasePoint(i, prepos, float4x4.identity);
                        tfChild.position = pos;
                    }

                    Mesh buildMesh_() =>
                        conv.BuildMesh(i, srccollider.sharedMesh, tfSegtop.worldToLocalMatrix);

                    void setColliderMesh_(Mesh newmesh)
                    {
                        collider.sharedMesh = newmesh;
                    }
                }
                Transform instantiate_()
                {
                    var tf = new GameObject("path collider").transform;
                    tf.SetParent(tfSegment, worldPositionStays: true);
                    return tf;
                }
            }
		}


		public void FitTerrainToPath()
        {

        }

		public void FitPathToTerrain()
        {
			var colliders = this.GetComponentsInChildren<MeshCollider>();
			var terrains = GameObject.FindObjectsOfType<Terrain>();
			var q =
				from terrain in terrains
				from collider in colliders
				select (terrain, collider)
				;
			foreach (var (terrain, collider) in q)
			{
				Debug.Log($"{terrain.name} x {collider.name}");
				PathUtility.AdjustHeights(collider, terrain);
			}
        }

	}



}
