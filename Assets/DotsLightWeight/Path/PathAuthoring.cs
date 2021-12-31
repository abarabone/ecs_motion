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

		public Transform EffectStartSide;
		public Transform EffectEndSide;

		public int Frequency;

		public GameObject ModelTopPrefab;


		void Awake()
		{
			
		}

		public void CreateInstances()
        {
			var qMeshModel =
				from pt in this.ModelTopPrefab.GetComponentsInChildren<StructurePartAuthoring>()
				from meshModel in pt.QueryModel
				select meshModel
				;
			var conv = new PathMeshConvertor(this.StartAnchor, this.EffectStartSide, this.EndAnchor, this.EffectEndSide, this.Frequency);
			conv.BuildAnchorParams(this.transform);
			foreach (var (ptMeshModel, i) in Enumerable.Range(0, this.Frequency).Select(i => (qMeshModel, i)))
			{
				Debug.Log(i);
				foreach (var x in ptMeshModel)
				{
					//conv.setUnitObject(x.Obj);
					foreach (var mmt in x.QueryMmts)
					{
						Debug.Log(mmt.mesh.name);
						var newmesh = conv.buildMesh(i, mmt.mesh, mmt.tf.position, x.TfRoot.position.y);
						var go = new GameObject();
						var mf = go.AddComponent<MeshFilter>();
						mf.mesh = newmesh;
						var mr = go.AddComponent<MeshRenderer>();
						mr.material = mmt.mats.First();
						go.transform.position = this.transform.position;
						go.transform.SetParent(this.transform);
					}
				}
            }
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

		float tunit;
		//float unitRatio;

		//int frequency;

		GameObject unitObject;

		Transform tfSegment;


		public PathMeshConvertor(
			Transform tfStart, Transform tfEffectStart, Transform tfEnd, Transform tfEffectEnd, int frequency)
		{
			this.startPosition = tfStart.position;
			this.endPosition = tfEnd.position;


			var dist = math.distance(this.endPosition, this.startPosition);

			this.effectStart = (tfEffectStart == null)
				? (float3)tfStart.forward * dist
				: (float3)tfEffectStart.position - this.startPosition;

			this.effectEnd = (tfEffectEnd == null)
				? (float3)tfEnd.forward * dist
				: (float3)tfEffectEnd.position - this.endPosition;


			this.tunit = 1.0f / frequency;
		}



		public void BuildAnchorParams(Transform tfBase)
		{
			var mtInv = tfBase.worldToLocalMatrix;

			this.startPosition = mtInv.MultiplyPoint3x4(startPosition);
			this.endPosition = mtInv.MultiplyPoint3x4(endPosition);

			this.effectStart = mtInv.MultiplyVector(effectStart);
			this.effectEnd = mtInv.MultiplyVector(effectEnd);

			this.tfSegment = tfBase;
		}


		public void setUnitObject(GameObject unit)
		{

			unitObject = unit;

		}




		public Mesh buildMesh(int i, Mesh srcMesh, float3 partPos, float offsetHeight)
		{

			var dstvtxs =
				srcMesh.vertices
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
			var t = vtx.z * this.tunit + this.tunit * (float)i;

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
