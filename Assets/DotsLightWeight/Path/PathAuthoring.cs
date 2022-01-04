using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

namespace DotsLite.LoadPath.Authoring
{
	using DotsLite.Structure.Authoring;

	public class PathAuthoring : MonoBehaviour
	{

		public Transform StartAnchor;
		public Transform EndAnchor;

		//public Transform EffectStartSide;
		//public Transform EffectEndSide;

		public int Frequency;

		public GameObject ModelTopPrefab;


		void Awake()
		{
			
		}

		public void CreateInstances()
		{
			var tf = this.transform;

			var maxZ = this.ModelTopPrefab.GetComponentsInChildren<MeshFilter>()
				.SelectMany(mf => mf.sharedMesh.vertices)
				.Max(v => v.z);
			Debug.Log(maxZ);

			var q =
				from i in Enumerable.Range(0, this.Frequency)
				let top = GameObject.Instantiate(this.ModelTopPrefab, this.transform)
				from mf in top.GetComponentsInChildren<MeshFilter>()
				select (top, mf, i)
				;
			//foreach (var (top, mf, i) in q)
			foreach (var x in q)
			{
				var (top, mf, i) = x;
				Debug.Log($"{i} mesh:{mf.name} top:{top.name}");

				var conv = new PathMeshConvertor(this.StartAnchor, this.EndAnchor, this.Frequency, maxZ, tf);

				var newmesh = conv.BuildMesh(i, mf.sharedMesh, 0.0f);
				mf.sharedMesh = newmesh;
				mf.transform.localPosition = Vector3.zero;
			}
		}
		//public void CreateInstances()
		//      {
		//	var qMeshModel =
		//		from pt in this.ModelTopPrefab.GetComponentsInChildren<StructurePartAuthoring>()
		//		from meshModel in pt.QueryModel
		//		select meshModel
		//		;
		//	var conv = new PathMeshConvertor(this.StartAnchor, this.EffectStartSide, this.EndAnchor, this.EffectEndSide, this.Frequency);
		//	conv.BuildAnchorParams(this.transform);
		//	foreach (var (ptMeshModel, i) in Enumerable.Range(0, this.Frequency).Select(i => (qMeshModel, i)))
		//	{
		//		Debug.Log(i);
		//		foreach (var x in ptMeshModel)
		//		{
		//			//conv.setUnitObject(x.Obj);
		//			foreach (var mmt in x.QueryMmts)
		//			{
		//				Debug.Log(mmt.mesh.name);
		//				var newmesh = conv.buildMesh(i, mmt.mesh, mmt.tf.position, x.TfRoot.position.y);
		//				var go = new GameObject();
		//				var mf = go.AddComponent<MeshFilter>();
		//				mf.mesh = newmesh;
		//				var mr = go.AddComponent<MeshRenderer>();
		//				mr.material = mmt.mats.First();
		//				go.transform.position = this.transform.position;
		//				go.transform.SetParent(this.transform);
		//			}
		//		}
		//          }
		//      }

		void convertMesh()
        {

		}

	}


	class PathMeshConvertor
	{

		float3 startPosition;
		float3 endPosition;

		float unitRatio;
		float maxZR;

		public PathMeshConvertor(
			Transform tfStart, Transform tfEnd, int frequency, float maxZ, Transform tfTop)
		{
			var startPosition = (float3)tfStart.position;
			var endPosition = (float3)tfEnd.position;


			var dist = math.distance(endPosition, startPosition);
			var effectStart = (float3)tfStart.forward * dist;
			var effectEnd = (float3)tfEnd.forward * dist;


			var mtInv = tfTop.worldToLocalMatrix;

			this.startPosition = mtInv.MultiplyPoint3x4(startPosition);
			this.endPosition = mtInv.MultiplyPoint3x4(endPosition);

			this.effectStart = mtInv.MultiplyVector(effectStart);
			this.effectEnd = mtInv.MultiplyVector(effectEnd);


			this.maxZR = 1.0f / maxZ;
			this.unitRatio = 1.0f / frequency;
		}


		public Mesh BuildMesh(int i, Mesh srcMesh, float offsetHeight)
		{
			var srcvtxs = srcMesh.vertices;

			var dstvtxs = srcvtxs
				.Select(vtx => interpolatePosition3d(i, vtx, offsetHeight))// - partPos)
				.Select(v => new Vector3(v.x, v.y, v.z))
				.ToArray();

			var dstMesh = new Mesh();

			dstMesh.vertices = dstvtxs;
			dstMesh.uv = srcMesh.uv;
			dstMesh.triangles = srcMesh.triangles;
			dstMesh.RecalculateNormals();

			return dstMesh;
		}

		float3 interpolatePosition3d(int i, float3 vtx, float offsetHeight)
		{
			var t = vtx.z * this.maxZR * this.unitRatio + this.unitRatio * (float)i;

			var p0 = startPosition;
			var p1 = endPosition;
			var v0 = effectStart;
			var v1 = effectEnd;

			var att = (2.0f * p0 - 2.0f * p1 + v0 + v1) * t * t;
			var bt = (-3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1) * t;

			var pos = att * t + bt * t + v0 * t + p0;

			var d = 3.0f * att + 2.0f * bt + v0;

			pos += vtx.x * math.normalize(new float3(d.z, 0.0f, -d.x));

			return new float3(pos.x, pos.y + vtx.y + offsetHeight, pos.z);
		}

	}

}
