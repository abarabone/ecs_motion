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


	public static class PathUtility
	{


		//public static void CreatePathParts(this PathAuthoring authoring)
		//{
		//	removeChildren_();
		//	var conv = createConvertor_();
		//	createPartSegments_(conv);
		//	//createColliderSegments_(conv);
		//	return;

		//	void removeChildren_()
		//	{
		//		var children = authoring.gameObject.Children().ToArray();
		//		children.ForEach(go => GameObject.DestroyImmediate(go));
		//	}
		//	PathMeshConvertor createConvertor_()
		//	{
		//		//var maxZ = this.LevelingColliderPrefab.GetComponent<MeshCollider>()
		//		var maxZ = authoring.LevelingColliderPrefab.GetComponent<MeshFilter>()
		//			.sharedMesh
		//			.vertices
		//			.Max(v => v.z);
		//		var conv = new PathMeshConvertor(
		//			authoring.StartAnchor, authoring.IsReverseStart, authoring.EndAnchor, authoring.IsReverseEnd,
		//			authoring.Frequency, maxZ);
		//		return conv;
		//	}
		//	void createPartSegments_(PathMeshConvertor conv)
		//	{
		//		var tfSegment = authoring.transform;
		//		var mtSegInv = (float4x4)tfSegment.worldToLocalMatrix;
		//		var mtStInv = (float4x4)authoring.StartAnchor.transform.worldToLocalMatrix;
		//		var q =
		//			from i in Enumerable.Range(0, authoring.Frequency)
		//			let tfSegtop = instantiate_()
		//			from mf in tfSegtop.GetComponentsInChildren<MeshFilter>()
		//			let tf = mf.transform
		//			let mtinv = (float4x4)tf.worldToLocalMatrix
		//			let pos = tf.position//math.transform(mtStInv, tf.position)
		//			let front = pos + tf.forward//math.transform(mtStInv, tf.forward)
		//			select (tfSegtop, mf, i, (mtinv, pos, front))
		//			;
		//		foreach (var x in q.ToArray())
		//		{
		//			var (tfSegtop, mf, i, pre) = x;

		//			var tfChild = mf.transform;
		//			var pos = conv.CalculateBasePoint(i, pre.pos, float4x4.identity);
		//			tfChild.position = pos;

		//			Debug.Log($"{i} mesh:{mf.name} top:{tfSegtop.name} {pos} {pre.pos}");

		//			if (mf.GetComponent<WithoutShapeTransforming>() != null)
		//			{
		//				//var forward = conv.CalculateBasePoint(i, pre.front, mtSegInv) - pos;
		//				//tfChild.forward = forward;
		//				var front = conv.CalculateBasePoint(i, pre.front, float4x4.identity);
		//				tfChild.LookAt(front, Vector3.up);
		//				continue;
		//			}

		//			var newmesh = conv.BuildMesh(i, mf.sharedMesh, tfSegtop.worldToLocalMatrix);
		//			mf.sharedMesh = newmesh;

		//			// コライダー以外のメッシュはそのまま維持
		//			// 暫定で常に外形メッシュと同じものをセット
		//			var col = mf.GetComponent<PhysicsShapeAuthoring>();
		//			if (col != null && col.ShapeType == ShapeType.Mesh)
		//			{
		//				col.SetMesh(newmesh);
		//			}
		//		}
		//		Transform instantiate_()
		//		{
		//			var tf = GameObject.Instantiate(authoring.ModelTopPrefab).transform;
		//			tf.SetParent(tfSegment, worldPositionStays: true);
		//			return tf;
		//		}
		//	}
		//}

	}

	public class PathMeshConvertor
	{

		float3 startPosition;
		float3 endPosition;

		float3 forwardStart;
		float3 forwardEnd;

		float3 rightStart;
		float3 rightEnd;
		float3 upStart;
		float3 upEnd;

		float unitRatio;
		float maxZR;

		public PathMeshConvertor(
			Transform tfStart, bool isReverseStart,
			Transform tfEnd, bool isReverseEnd,
			int frequency, float maxZ)
		{
			this.startPosition = (float3)tfStart.position;
			this.endPosition = (float3)tfEnd.position;

			var ss = math.select(1, -1, isReverseStart);
			var se = math.select(1, -1, isReverseEnd);

			// 向きを
			var dist = math.distance(endPosition, startPosition);
			this.forwardStart = (float3)tfStart.forward * (dist * ss);
			this.forwardEnd = (float3)tfEnd.forward * (dist * se);

			this.rightStart = tfStart.right * ss;
			this.rightEnd = tfEnd.right * se;
			this.upStart = tfStart.up;
			this.upEnd = tfEnd.up;// tfStart.up;

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

		public float3 CalculateBasePoint(int i, float3 pos, float4x4 mtInv) =>
			this.interpolatePosition3d(i, pos, mtInv);

		float3 interpolatePosition3d(int i, float3 vtx, float4x4 mtInv)
		{
            var t = vtx.z * this.maxZR * this.unitRatio + this.unitRatio * (float)i;

            var p0 = math.transform(mtInv, startPosition);
			var p1 = math.transform(mtInv, endPosition);
			var v0 = math.rotate(mtInv, forwardStart);
			var v1 = math.rotate(mtInv, forwardEnd);

			var att = (2.0f * p0 - 2.0f * p1 + v0 + v1) * t * t;
			var bt = (-3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1) * t;

			var pos = att * t + bt * t + v0 * t + p0;

			var d = 3.0f * att + 2.0f * bt + v0;// pos の微分（割合？）

            pos += vtx.x * math.normalize(new float3(d.z, 0.0f, -d.x));
            //pos += vtx.x * math.normalize(new float3(d.z, d.y, -d.x));
            //pos += vtx.y * math.normalize(new float3(d.x, d.z, d.y));
            return new float3(pos.x, pos.y + vtx.y, pos.z);
		}

		//float3 interpolatePosition3d(int i, float3 vtx, float4x4 mtInv)
		//{
		//	var t = vtx.z * this.maxZR * this.unitRatio + this.unitRatio * (float)i;

		//	var p0 = math.transform(mtInv, startPosition);
		//	var p1 = math.transform(mtInv, endPosition);
		//	var v0 = math.rotate(mtInv, forwardStart);
		//	var v1 = math.rotate(mtInv, forwardEnd);

		//	var att = (2.0f * p0 - 2.0f * p1 + v0 + v1) * t * t;
		//	var bt = (-3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1) * t;

		//	var pos = att * t + bt * t + v0 * t + p0;

		//	var d = 3.0f * att + 2.0f * bt + v0;// pos の微分（割合？）
		//	var xofs = vtx.x * math.normalize(new float3(d.z, 0.0f, -d.x));


  //          var p0u = math.transform(mtInv, this.upStart);
  //          var p1u = math.transform(mtInv, this.upEnd);
  //          var v0u = math.rotate(mtInv, forwardStart);
  //          var v1u = math.rotate(mtInv, forwardEnd);

  //          var attu = (2.0f * p0u - 2.0f * p1u + v0u + v1u) * t * t;
  //          var btu = (-3.0f * p0u + 3.0f * p1u - 2.0f * v0u - v1u) * t;

  //          var posu = attu * t + btu * t + v0u * t + p0u;



  //          pos += xofs;
		//	return new float3(pos.x, pos.y + vtx.y, pos.z);
		//}

		//float3 interpolatePosition3d(int i, float3 vtx, float4x4 mtInv)
		//{
		//	var t = vtx.z * this.maxZR * this.unitRatio + this.unitRatio * (float)i;

		//	var p0f = math.transform(mtInv, startPosition);
		//	var p1f = math.transform(mtInv, endPosition);
		//	var v0f = math.rotate(mtInv, forwardStart);
		//	var v1f = math.rotate(mtInv, forwardEnd);

		//	var attf = (2.0f * p0f - 2.0f * p1f + v0f + v1f) * t * t;
		//	var btf = (-3.0f * p0f + 3.0f * p1f - 2.0f * v0f - v1f) * t;

		//	var posf = attf * t + btf * t + v0f * t + p0f;


		//	var p0r = math.transform(mtInv, startPosition + this.rightStart);
		//	var p1r = math.transform(mtInv, endPosition + this.rightEnd);
		//	var v0r = math.rotate(mtInv, forwardStart);
		//	var v1r = math.rotate(mtInv, forwardEnd);

		//	var attr = (2.0f * p0r - 2.0f * p1r + v0r + v1r) * t * t;
		//	var btr = (-3.0f * p0r + 3.0f * p1r - 2.0f * v0r - v1r) * t;

		//	var posr = attr * t + btr * t + v0r * t + p0r;


		//	var p0u = math.transform(mtInv, startPosition + this.upStart);
		//	var p1u = math.transform(mtInv, endPosition + this.upEnd);
		//	var v0u = math.rotate(mtInv, forwardStart);
		//	var v1u = math.rotate(mtInv, forwardEnd);

		//	var attu = (2.0f * p0u - 2.0f * p1u + v0u + v1u) * t * t;
		//	var btu = (-3.0f * p0u + 3.0f * p1u - 2.0f * v0u - v1u) * t;

		//	var posu = attu * t + btu * t + v0u * t + p0u;


		//	return posf + (posu - posf) * vtx.y + (posr - posf) * vtx.x;
		//}
	}



}
