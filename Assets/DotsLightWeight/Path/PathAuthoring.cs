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

	public class PathAuthoring : MonoBehaviour
	{

		public Transform StartAnchor;
		public bool IsReverseStart;
		public Transform EndAnchor;
		public bool IsReverseEnd;

		public int Frequency;

		public GameObject ModelTopPrefab;
		public GameObject LevelingColliderPrefab;


		void Awake()
		{
			
		}

		public void CreatePathParts()
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
				var maxZ = this.LevelingColliderPrefab.GetComponent<MeshCollider>()
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
				var q =
					from i in Enumerable.Range(0, this.Frequency)
					let segtop = instantiate_()
					from mf in segtop.GetComponentsInChildren<MeshFilter>()
					let pos = mf.transform.position
					let forward = mf.transform.forward
					select (segtop, mf, i, (pos, forward))
					;
				foreach (var x in q)
				{
					var (segtop, mf, i, pre) = x;
					Debug.Log($"{i} mesh:{mf.name} top:{segtop.name} {pre.pos} {pre.forward}");

                    var tfChild = mf.transform;
                    tfChild.position = conv.CalculateBasePoint(i, pre.pos);

					if (mf.GetComponent<WithoutShapeTransforming>() != null)
					{
						mf.transform.forward = conv.CalculateBaseDirection(i, pre.forward);
						continue;
                    }

					var newmesh = conv.BuildMesh(i, mf.sharedMesh, tfChild.worldToLocalMatrix);
					mf.sharedMesh = newmesh;

					// コライダー以外のメッシュはそのまま維持
					// 暫定で常に外形メッシュと同じものをセット
                    var col = mf.GetComponent<PhysicsShapeAuthoring>();
                    if (col != null && col.ShapeType == ShapeType.Mesh)
                    {
						col.SetMesh(newmesh);
                    }
				}
				GameObject instantiate_()
				{
					var go = GameObject.Instantiate(this.ModelTopPrefab);
					go.transform.SetParent(tfSegment, worldPositionStays: true);
					return go;
				}
			}
			//void createColliderSegments_(PathMeshConvertor conv)
			//{
			//	var tf = this.transform;
			//	var col = this.LevelingColliderPrefab.GetComponent<MeshCollider>();
			//	var q =
			//		from i in Enumerable.Range(0, this.Frequency)
			//		let seg = new GameObject($"collider {i}")
			//		select (seg, col, i)
			//		;
			//	foreach (var x in q)
			//	{
			//		var (seg, srccol, i) = x;
			//		Debug.Log($"{i} collider:{srccol.name} top:{seg.name}");

			//		var newmesh = conv.BuildMesh(i, srccol.sharedMesh, 0.0f);
			//		var dstcol = seg.AddComponent<MeshCollider>();
			//		dstcol.sharedMesh = newmesh;
			//		seg.transform.SetParent(tf, worldPositionStays: true);
			//		seg.transform.localPosition = Vector3.zero;
			//	}
			//}
		}

		void convertMesh()
        {

		}

	}


	class PathMeshConvertor
	{

		float3 startPosition;
		float3 endPosition;

		float3 effectStart;
		float3 effectEnd;

		float unitRatio;
		float maxZR;

		public PathMeshConvertor(
			Transform tfStart, bool isReverseStart,
			Transform tfEnd, bool isReverseEnd,
			int frequency, float maxZ)
		{
			this.startPosition = (float3)tfStart.position;
			this.endPosition = (float3)tfEnd.position;

			var dist = math.distance(endPosition, startPosition);
			this.effectStart = (float3)tfStart.forward * (dist * math.select(1, -1, isReverseStart));
			this.effectEnd = (float3)tfEnd.forward * (dist * math.select(1, -1, isReverseEnd));

			this.maxZR = 1.0f / maxZ;
			this.unitRatio = 1.0f / frequency;
		}


		public Mesh BuildMesh(int i, Mesh srcMesh, float4x4 mtChildInv)
		{
			var srcvtxs = srcMesh.vertices;

			var dstvtxs = srcvtxs
				.Select(vtx => interpolatePosition3d(i, vtx, mtChildInv))
				.Select(v => new Vector3(v.x, v.y, v.z))
				.ToArray();

			var dstMesh = new Mesh();

			dstMesh.vertices = dstvtxs;
			dstMesh.uv = srcMesh.uv;
			dstMesh.triangles = srcMesh.triangles;
			dstMesh.RecalculateNormals();

			return dstMesh;
		}

		public float3 CalculateBasePoint(int i, float3 pos) =>
			this.interpolatePosition3d(i, pos, float4x4.identity);
		public float3 CalculateBaseDirection(int i, float3 forward) =>
			this.interpolatePosition3d(i, forward, float4x4.identity);

		float3 interpolatePosition3d(int i, float3 vtx, float4x4 mtInv)
		{
			var t = vtx.z * this.maxZR * this.unitRatio + this.unitRatio * (float)i;

			var p0 = math.transform(mtInv, startPosition);
			var p1 = math.transform(mtInv, endPosition);
			var v0 = math.rotate(mtInv, effectStart);
			var v1 = math.rotate(mtInv, effectEnd);

			var att = (2.0f * p0 - 2.0f * p1 + v0 + v1) * t * t;
			var bt = (-3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1) * t;

			var pos = att * t + bt * t + v0 * t + p0;

			var d = 3.0f * att + 2.0f * bt + v0;

			pos += vtx.x * math.normalize(new float3(d.z, 0.0f, -d.x));

			return new float3(pos.x, pos.y + vtx.y, pos.z);
		}

	}

}
