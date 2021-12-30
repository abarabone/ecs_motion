using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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
}
