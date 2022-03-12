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





	// 整数２次元位置 ----------------

	public struct Int2
	{
		public int x;
		public int y;

		public Int2(int a, int b)
		{
			x = a;
			y = b;
		}

		static public Int2 ceil(Vector2 vc)
		{
			Int2 i;

			i.x = Mathf.CeilToInt(vc.x);
			i.y = Mathf.CeilToInt(vc.y);

			return i;
		}

		static public Int2 floor(Vector2 vc)
		{
			Int2 i;

			i.x = Mathf.FloorToInt(vc.x);
			i.y = Mathf.FloorToInt(vc.y);

			return i;
		}

		static public Int2 min(Int2 a, Int2 b)
		{
			var res = new Int2();

			res.x = (a.x > b.x) ? b.x : a.x;
			res.y = (a.y > b.y) ? b.y : a.y;

			return res;
		}

		static public Int2 max(Int2 a, Int2 b)
		{
			var res = new Int2();

			res.x = (a.x < b.x) ? b.x : a.x;
			res.y = (a.y < b.y) ? b.y : a.y;

			return res;
		}

	}







	// 地形変形操作ユーティリティー **********************************


	// 地形の値を読み書きする ------------------------

	public struct FieldManipulator
	{

		public Int2 st { get; private set; }    // テレイン上の開始インデックス
		public Int2 ed { get; private set; }    // テレイン上の終了インデックス

		public Int2 len { get; private set; }   // テレイン上の繰り返し回数


		public FieldManipulator(Vector2 min, Vector2 max, Vector2 unitR, Int2 length)
		{

			st = Int2.floor(Vector2.Scale(min, unitR));
			ed = Int2.ceil(Vector2.Scale(max, unitR));

			st = Int2.max(new Int2(), st);
			ed = Int2.min(ed, new Int2(length.x - 1, length.y - 1));

			len = new Int2(ed.x - st.x + 1, ed.y - st.y + 1);

		}


		public float[,] getHeights(TerrainData td)
		{
			return td.GetHeights(st.x, st.y, len.x, len.y);
		}

		public float[,] getBrankHeights()
		{
			return new float[len.y, len.x];
		}

		public void setHeights(TerrainData td, float[,] hs)
		{
			td.SetHeights(st.x, st.y, hs);
		}


		public float[,,] getAlphamaps(TerrainData td)
		{
			return td.GetAlphamaps(st.x, st.y, len.x, len.y);
		}

		public void setAlphamaps(TerrainData td, float[,,] ms)
		{
			td.SetAlphamaps(st.x, st.y, ms);
		}

		public int[] getDetailSupportedLayers(TerrainData td)
		{
			return td.GetSupportedLayers(st.x, st.y, len.x, len.y);
		}
		public int[,] getDetails(TerrainData td, int layer)
		{
			return td.GetDetailLayer(st.x, st.y, len.x, len.y, layer);
		}
		public void setDetails(TerrainData td, int[,] ds, int layer)
		{
			td.SetDetailLayer(st.x, st.y, layer, ds);
		}


		public Vector2 getIterationPosition(int ix, int iy, Vector2 unit)
		{
			return new Vector2((float)(st.x + ix) * unit.x, (float)(st.y + iy) * unit.y);
		}

		public Vector3 getIterationPosition3d(int ix, int iy, Vector2 unit, float h = 0.0f)
		{
			return new Vector3((float)(st.x + ix) * unit.x, h, (float)(st.y + iy) * unit.y);
		}

	}



	// 地形を変形させる ***********************************

	// SimpleTerrainOperator	… 高さ・テクスチャ・詳細がすべて同じ解像度の場合に使う（半端実装）
	// TerrainOperator			… 高さ・テクスチャを別個に処理する
	// FullTerrainOperator		… 高さ・テクスチャ・詳細を別個に処理する


	public interface ITerrainOperator
	{

		void adjustMesh(MeshCollider mc);
		// メッシュにフィットさせる

		//IEnumerator adjustSphere(Vector3 center, float radius, float impact, Terrain tr, TerrainCollider tc, Rigidbody rb);
		//// 爆発円で変形させる

	}

	public class SimpleTerrainOperator : ITerrainOperator
	// ハイトとスプラットが同じ解像度の時に使える　でもそんなことないか、ハイトは奇数・スプラットは偶数だし
	{

		public TerrainData td { get; private set; }

		public Vector2 fieldUnit { get; private set; }
		public float fieldUnitHeight { get; private set; }
		public Vector2 fieldUnitR { get; private set; }
		public float fieldUnitHeightR { get; private set; }
		public Int2 fieldLength { get; private set; }

		public Vector2 terrainPosition { get; private set; }
		public float terrainPositionHeight { get; private set; }


		public SimpleTerrainOperator(Terrain terrain)
		{

			td = terrain.terrainData;

			fieldUnit = getFieldScale(terrain);
			fieldUnitHeight = terrain.terrainData.heightmapScale.y;

			fieldUnitR = new Vector2(1.0f / fieldUnit.x, 1.0f / fieldUnit.y);
			fieldUnitHeightR = 1.0f / fieldUnitHeight;

			terrainPosition = getPosition(terrain);
			terrainPositionHeight = terrain.GetPosition().y;

			fieldLength = getFieldLength(terrain);

		}

		Int2 getFieldLength(Terrain terrain)
		{
			//	var unitLength	= terrain.terrainData.heightmapResolution;
			return new Int2(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);//unitLength, unitLength );
		}
		Vector2 getFieldScale(Terrain terrain)
		{
			var unitSize = terrain.terrainData.heightmapScale;
			return new Vector2(unitSize.x, unitSize.z);
		}
		Vector2 getPosition(Terrain terrain)
		{
			var tpos = terrain.GetPosition();
			return new Vector2(tpos.x, tpos.z);
		}

	}

	// 地形変形を実際に行う ================================

	struct FieldAdjusterForRoadMesh
	{

		MeshCollider mc;

		Vector2 min;
		Vector2 max;

		Vector3 tofs;


		public FieldAdjusterForRoadMesh(MeshCollider mc, SimpleTerrainOperator op)
		{
			min = new Vector2(mc.bounds.min.x, mc.bounds.min.z) - op.terrainPosition;
			max = new Vector2(mc.bounds.max.x, mc.bounds.max.z) - op.terrainPosition;

			this.mc = mc;

			tofs = new Vector3(op.terrainPosition.x, op.terrainPositionHeight, op.terrainPosition.y);
		}

		public void adjustHeights(SimpleTerrainOperator op)
		{

			var m = new FieldManipulator(min, max, op.fieldUnitR, op.fieldLength);

			var hs = m.getHeights(op.td);

			var tofs = new Vector3(op.terrainPosition.x, op.terrainPositionHeight, op.terrainPosition.y);

			for (var iy = 0; iy < m.len.y; iy++)
				for (var ix = 0; ix < m.len.x; ix++)
				{

					var pos = m.getIterationPosition3d(ix, iy, op.fieldUnit) + tofs;

					var start = pos + Vector3.up * 512.0f;
					var end = pos + Vector3.down * 512.0f;

					var ray = new Ray(start, end - start);
					var res = new RaycastHit();
					if (mc.Raycast(ray, out res, 1024.0f))
					{

						hs[iy, ix] = (res.point.y - op.terrainPositionHeight) * op.fieldUnitHeightR;

					}

				}

			m.setHeights(op.td, hs);

		}

		//public void adjustAlphamaps(TerrainOperator op)
		//{

		//	var m = new FieldManipulator(min, max, op.mapUnitR, op.mapLength);

		//	var ms = m.getAlphamaps(op.td);

		//	for (var iy = 0; iy < m.len.y; iy++)
		//		for (var ix = 0; ix < m.len.x; ix++)
		//		{

		//			var pos = m.getIterationPosition3d(ix, iy, op.mapUnit) + tofs;

		//			var start = pos + Vector3.up * 512.0f;
		//			var end = pos + Vector3.down * 512.0f;

		//			var ray = new Ray(start, end - start);
		//			var res = new RaycastHit();
		//			if (mc.Raycast(ray, out res, 1024.0f))
		//			{

		//				ms[iy, ix, 0] = 0.0f;
		//				ms[iy, ix, 1] = 1.0f;
		//				ms[iy, ix, 2] = 0.0f;
		//				ms[iy, ix, 3] = 0.0f;

		//			}

		//		}

		//	m.setAlphamaps(op.td, ms);

		//}

		//public void adjustDetails(FullTerrainOperator op)
		//{

		//}

	}

}
