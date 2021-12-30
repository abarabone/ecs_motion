using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

namespace DotsLite.Path.Authoring
{
	using DotsLite.Structure.Authoring;

	public class PathAuthoring : MonoBehaviour
	{

		public Transform StartAnchor;
		public Transform EndAnchor;

		public Transform EffectStartSide;
		public Transform EffectEndSide;

		public int Freq;

		public GameObject ModelTopPrefab;


		void Awake()
		{
			
		}

		GameObject createInstance()
        {
			var qMeshModel =
				from pt in this.ModelTopPrefab.GetComponentsInChildren<StructurePartAuthoring>()
				from meshModel in pt.QueryModel
				select meshModel
				;
			
			return new GameObject();
        }

		void convertMesh()
        {

		}


		Mesh buildMesh(int i, Mesh srcMesh, Vector3 partPos, float offsetHeight)
		{

			var dstvtxs = new Vector3[srcMesh.vertexCount];


			for (var iv = srcMesh.vertexCount; iv-- > 0;)
			{

				dstvtxs[iv] = interpolatePosition3d(i, srcMesh.vertices[iv], offsetHeight) - partPos;

			}


			var dstMesh = new Mesh();

			dstMesh.vertices = dstvtxs;

			dstMesh.uv = srcMesh.uv;

			dstMesh.triangles = srcMesh.triangles;


			dstMesh.RecalculateNormals();


			return dstMesh;

		}

		Vector3 interpolatePosition3d(int i, Vector3 vtx, float offsetHeight)
		{

			var t = vtx.z * unitRatio + tunit * (float)i;

			var p0 = this.StartAnchor.position;//.StartPosition;
			var p1 = this.EndPosition;
			var v0 = this.EffectStart;
			var v1 = this.EffectEnd;

			var att = (2.0f * p0 - 2.0f * p1 + v0 + v1) * t * t;
			var bt = (-3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1) * t;

			var pos = att * t + bt * t + v0 * t + p0;

			var d = 3.0f * att + 2.0f * bt + v0;

			pos += vtx.x * new Vector3(d.z, 0.0f, -d.x).normalized;

			return new Vector3(pos.x, pos.y + vtx.y + offsetHeight, pos.z);

		}
	}


	public struct PathMeshConvertor
	{

		float3 startPosition;
		float3 endPosition;

		float3 effectStart;
		float3 effectEnd;

		float tunit;
		float unitRatio;

		int frequency;

		GameObject unitObject;

		Transform tfSegment;




		public PathMeshConvertor(
			Transform tfStart, Transform tfEffectStart, Transform tfEnd, Transform tfEffectEnd, int frequency)
		{

			this.startPosition = tfStart.position;

			this.endPosition = tfEnd.position;


			var dist = math.distance(this.endPosition, this.startPosition);


			effectStart = (tfEffectStart == null)
				? dist * (float3)tfStart.forward
				: (float3)tfEffectStart.position - startPosition;

			effectEnd = (tfEffectEnd == null)
				? dist * (float3)tfEnd.forward
				: (float3)tfEffectEnd.position - endPosition;


			this.tunit = 1.0f / frequency;

			this.frequency = frequency;
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




		Mesh buildMesh(int i, Mesh srcMesh, Vector3 partPos, float offsetHeight)
		{

			var dstvtxs = new float3[srcMesh.vertexCount];


			for (var iv = srcMesh.vertexCount; iv-- > 0;)
			{

				dstvtxs[iv] = interpolatePosition3d(i, srcMesh.vertices[iv], offsetHeight) - partPos;

			}


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
