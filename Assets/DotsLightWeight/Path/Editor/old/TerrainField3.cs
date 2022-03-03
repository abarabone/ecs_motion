using UnityEngine;
using System.Collections;
using System; 

public class TerrainField3 : _Hit3
{

	public Rigidbody		rb { get; protected set; }

	public Terrain			terrain			{ get; protected set; }
	public TerrainCollider	terrainCollider	{ get; protected set; }


	ITerrainOperator	terrainOperator;

	

	public void init( Terrain tr, TerrainCollider tc, Rigidbody irb )
	{

		terrain			= tr;
		terrainCollider	= tc;

		rb = irb;

		terrainOperator = new FullTerrainOperator( tr );
		
	}
	
	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider c, _Action3 attacker )
	//public override void blasted( float damage, float pressure, float boringFactor, Vector3 center, float radius, Collider c, int teamFlag )
	{

		StartCoroutine( terrainOperator.adjustSphere( center, radius, boringFactor, terrain, terrainCollider, rb ) );
	//	terrainOperator.adjustSphere( center, radius, boringFactor, terrain, terrainCollider );


		GM.groundDustCloud.emit( center, Vector3.up, 1.0f, radius * 2.0f, Mathf.Max( (int)radius, 8 ) );

	}

}




// 整数２次元位置 ----------------

public struct Int2
{
	public int	x;
	public int	y;

	public Int2( int a, int b )
	{
		x = a;
		y = b;
	}

	static public Int2 ceil( Vector2 vc )
	{
		Int2 i;

		i.x = Mathf.CeilToInt( vc.x );
		i.y = Mathf.CeilToInt( vc.y );

		return i;
	}

	static public Int2 floor( Vector2 vc )
	{
		Int2 i;

		i.x = Mathf.FloorToInt( vc.x );
		i.y = Mathf.FloorToInt( vc.y );

		return i;
	}

	static public Int2 min( Int2 a, Int2 b )
	{
		var res = new Int2();

		res.x = ( a.x > b.x )? b.x: a.x;
		res.y = ( a.y > b.y )? b.y: a.y;

		return res;
	}

	static public Int2 max( Int2 a, Int2 b )
	{
		var res = new Int2();

		res.x = ( a.x < b.x )? b.x: a.x;
		res.y = ( a.y < b.y )? b.y: a.y;

		return res;
	}
	
}





// 地形を変形させる ***********************************

// SimpleTerrainOperator	… 高さ・テクスチャ・詳細がすべて同じ解像度の場合に使う（半端実装）
// TerrainOperator			… 高さ・テクスチャを別個に処理する
// FullTerrainOperator		… 高さ・テクスチャ・詳細を別個に処理する


public interface ITerrainOperator
{
 
	void adjustMesh( MeshCollider mc );
	// メッシュにフィットさせる

	IEnumerator adjustSphere( Vector3 center, float radius, float impact, Terrain tr, TerrainCollider tc, Rigidbody rb );
	// 爆発円で変形させる

}




public class SimpleTerrainOperator : ITerrainOperator
// ハイトとスプラットが同じ解像度の時に使える　でもそんなことないか、ハイトは奇数・スプラットは偶数だし
{
	
	public TerrainData	td	{ get; private set; }
	
	public Vector2	fieldUnit			{ get; private set; }
	public float	fieldUnitHeight		{ get; private set; }
	public Vector2	fieldUnitR			{ get; private set; }
	public float	fieldUnitHeightR	{ get; private set; }
	public Int2		fieldLength			{ get; private set; }

	public Vector2	terrainPosition			{ get; private set; }
	public float	terrainPositionHeight	{ get; private set; }
	
	
	public SimpleTerrainOperator( Terrain terrain )
	{
		
		td	= terrain.terrainData;
		
		fieldUnit		= getFieldScale( terrain );
		fieldUnitHeight	= terrain.terrainData.heightmapScale.y;
		
		fieldUnitR		 = new Vector2( 1.0f / fieldUnit.x, 1.0f / fieldUnit.y );
		fieldUnitHeightR = 1.0f / fieldUnitHeight;

		terrainPosition			= getPosition( terrain );
		terrainPositionHeight	= terrain.GetPosition().y;
		
		fieldLength	= getFieldLength( terrain );
		
	}
	
	Int2 getFieldLength( Terrain terrain )
	{
		//	var unitLength	= terrain.terrainData.heightmapResolution;
		return new Int2( terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution );//unitLength, unitLength );
	}
	Vector2 getFieldScale( Terrain terrain )
	{
		var unitSize	= terrain.terrainData.heightmapScale;
		return new Vector2( unitSize.x, unitSize.z );
	}
	Vector2 getPosition( Terrain terrain )
	{
		var tpos	= terrain.GetPosition();
		return new Vector2( tpos.x, tpos.z );
	}
	
	
	public void adjustMesh( MeshCollider mc )
	{
/*
		var min = new Vector2( mc.bounds.min.x, mc.bounds.min.z ) - terrainPosition;
		var max = new Vector2( mc.bounds.max.x, mc.bounds.max.z ) - terrainPosition;
		
		
		var m = new FieldManipulator( min, max, fieldUnitR, fieldLength );
		
		var hs = m.getHeights( td );
		var ms = m.getAlphamaps( td );
		
		var tofs = new Vector3( terrainPosition.x, terrainPositionHeight, terrainPosition.y );
		
		for( var iy = 0; iy < m.len.y; iy++ )
			for( var ix = 0; ix < m.len.x; ix++ )
		{
			
			var pos = m.getIterationPosition3d( ix, iy, fieldUnit ) + tofs;
			
			var start	= pos + Vector3.up * 512.0f;
			var end		= pos + Vector3.down * 512.0f;
			
			var ray = new Ray( start, end - start );
			var res = new RaycastHit();
			if( mc.Raycast( ray, out res, 1024.0f ) )
			{
				
				hs[ iy, ix ] = ( res.point.y - terrainPositionHeight ) * fieldUnitHeightR;
				
				ms[ iy, ix, 0 ] = 0.0f;
				ms[ iy, ix, 1 ] = 1.0f;
				ms[ iy, ix, 2 ] = 0.0f;
				ms[ iy, ix, 3 ] = 0.0f;

			}
			
		}
		
		m.setHeights( td, hs );
		m.setAlphamaps( td, ms );
*/
	}

	public IEnumerator adjustSphere( Vector3 center, float radius, float impact, Terrain tr, TerrainCollider tc, Rigidbody rb )
	{
		yield return 0;
/*			
		var sqrradius = radius * radius;
		var sradius	= new Vector2( radius, radius );
		
		var scenter = new Vector2( center.x, center.z ) - terrainPosition;
		var centerHeight = center.y - terrainPositionHeight;
		
		center = new Vector3( scenter.x, centerHeight, scenter.y );
		
		var min = scenter - sradius;
		var max = scenter + sradius;
		
		
		var sqrradiusR	= 1.0f / sqrradius;
		
		var m = new FieldManipulator( min, max, fieldUnitR, fieldLength );
		
		var hs = m.getHeights( td );
		var ms = m.getAlphamaps( td );

		for( var iy = 0; iy < m.len.y; iy++ )
		for( var ix = 0; ix < m.len.x; ix++ )
		// 地形ローカル空間で処理
		{
			
			var height = hs[ iy, ix ] * fieldUnitHeight;
			
			var pos = m.getIterationPosition3d( ix, iy, fieldUnit, height );
			
			var spos = new Vector2( pos.x, pos.z );
			
			if( height > centerHeight )
			{
				var sqrsdist = ( spos - scenter ).sqrMagnitude;
				
				if( sqrsdist <= sqrradius )
				{
					var hith = centerHeight - ( sqrradius - sqrsdist ) * impact;
					
					hs[ iy, ix ] = Mathf.Lerp( hith, height, ( sqrsdist * sqrradiusR ) ) * fieldUnitHeightR;
					
					ms[ iy, ix, 0 ] = 0.3f;
					ms[ iy, ix, 1 ] = 0.0f;
					ms[ iy, ix, 2 ] = 0.0f;
					ms[ iy, ix, 3 ] = 0.0f;
				}
			}
			else
			{
				if( ( pos - center ).sqrMagnitude <= sqrradius )
				{
					var hith = centerHeight - ( sqrradius - ( scenter - spos ).sqrMagnitude ) * impact;
					
					hs[ iy, ix ] = ( hith < height? hith: height ) * fieldUnitHeightR;
					
					ms[ iy, ix, 0 ] = 0.3f;
					ms[ iy, ix, 1 ] = 0.0f;
					ms[ iy, ix, 2 ] = 0.0f;
					ms[ iy, ix, 3 ] = 0.0f;
				}
			}
		}
		
		m.setHeights( td, hs );
		m.setAlphamaps( td, ms );
*/
	}

}


public class TerrainOperator : SimpleTerrainOperator, ITerrainOperator
{

	public Vector2	mapUnit		{ get; private set; }
	public Vector2	mapUnitR	{ get; private set; }
	public Int2		mapLength	{ get; private set; }


	public TerrainOperator( Terrain terrain ) : base( terrain )
	{

		mapUnit		= getMapScale( terrain );
		mapUnitR	= new Vector2( 1.0f / mapUnit.x, 1.0f / mapUnit.y );
		mapLength	= getMapLength( terrain );

	}

	Int2 getMapLength( Terrain terrain )
	{
		var unitLength	= terrain.terrainData.alphamapHeight;
		return new Int2( unitLength, unitLength );
	}
	Vector2 getMapScale( Terrain terrain )
	{
		var unitSize	= terrain.terrainData.size / (float)terrain.terrainData.alphamapResolution;
		return new Vector2( unitSize.x, unitSize.z );
	}

	
	public void adjustMesh( MeshCollider mc )
	{

		var adjuster = new FieldAdjusterForRoadMesh( mc, this );

		adjuster.adjustHeights( this );

		adjuster.adjustAlphamaps( this );

	}

	public IEnumerator adjustSphere( Vector3 center, float radius, float impact, Terrain tr, TerrainCollider tc/*, Rigidbody rb*/ )
	{
		
		var adjuster = new FieldAdjusterForExplosionShpere( center, radius, impact, this );
		
		
		tc.enabled = false;
		
		adjuster.adjustHeights( this );
		
		tc.enabled = true;
		
		yield return 0;
		
		adjuster.adjustAlphamaps( this );

		
	}

}


public class FullTerrainOperator : TerrainOperator, ITerrainOperator
{
	
	public Vector2	detailUnit		{ get; private set; }
	public Vector2	detailUnitR		{ get; private set; }
	public Int2		detailLength	{ get; private set; }


	int	hitCount;

	
	public FullTerrainOperator( Terrain terrain ) : base( terrain )
	{
		
		detailUnit		= getDetailScale( terrain );
		detailUnitR		= new Vector2( 1.0f / detailUnit.x, 1.0f / detailUnit.y );
		detailLength	= getDetailLength( terrain );
		
	}
	
	Int2 getDetailLength( Terrain terrain )
	{
		var unitLength	= terrain.terrainData.detailHeight;
		return new Int2( unitLength, unitLength );
	}
	Vector2 getDetailScale( Terrain terrain )
	{
		var unitSize	= terrain.terrainData.size / (float)terrain.terrainData.detailResolution;
		return new Vector2( unitSize.x, unitSize.z );
	}


	public IEnumerator adjustSphere( Vector3 center, float radius, float impact, Terrain tr, TerrainCollider tc, Rigidbody rb )
	{
		//if( hitCount == 0 ) Debug.Log( "tr hit start " + Time.frameCount );
		
		hitCount++;

		//Debug.Log( hitCount );

		yield return new WaitForFixedUpdate();



		var adjuster = new FieldAdjusterForExplosionShpere( center, radius, impact, this );


		// 位置フレーム内の地形変形は、一度に修正する。
		// 地形変形の場合、detect off はあったほうがいいぽい。衝突物の多さの関係かも？

		if( !tc.enabled ) { rb.detectCollisions = false; tc.enabled = false; }

		adjuster.adjustHeights( this );

		if( --hitCount <= 0 ) { tc.enabled = true; rb.detectCollisions = true; }

		//if( hitCount <= 0 ) Debug.Log( "tr hit end " + Time.frameCount );


		yield return 0;// new WaitForFixedUpdate();



		adjuster.adjustAlphamaps( this );



		yield return 0;// new WaitForFixedUpdate();



		adjuster.adjustDetails( this );

		tr.Flush();

	}
	
}





// 地形変形操作ユーティリティー **********************************


// 地形の値を読み書きする ------------------------

public struct FieldManipulator
{

	public Int2	st		{ get; private set; }	// テレイン上の開始インデックス
	public Int2	ed		{ get; private set; }	// テレイン上の終了インデックス
	
	public Int2	len		{ get; private set; }	// テレイン上の繰り返し回数


	public FieldManipulator( Vector2 min, Vector2 max, Vector2 unitR, Int2 length )
	{
		
		st = Int2.floor( Vector2.Scale( min, unitR ) );
		ed = Int2.ceil( Vector2.Scale( max, unitR ) );
		
		st = Int2.max( new Int2(), st );
		ed = Int2.min( ed, new Int2( length.x - 1, length.y - 1 ) );
		
		len = new Int2( ed.x - st.x + 1, ed.y - st.y + 1 );

	}


	public float[,] getHeights( TerrainData td )
	{
		return td.GetHeights( st.x, st.y, len.x, len.y );
	}

	public float[,] getBrankHeights()
	{
		return new float[ len.y, len.x ];
	}

	public void setHeights( TerrainData td, float[,] hs )
	{
		td.SetHeights( st.x, st.y, hs );
	}


	public float[,,] getAlphamaps( TerrainData td )
	{
		return td.GetAlphamaps( st.x, st.y, len.x, len.y );
	}

	public void setAlphamaps( TerrainData td, float[,,] ms )
	{
		td.SetAlphamaps( st.x, st.y, ms );
	}

	public int[] getDetailSupportedLayers( TerrainData td )
	{
		return td.GetSupportedLayers( st.x, st.y, len.x, len.y );
	}
	public int[,] getDetails( TerrainData td, int layer )
	{
		return td.GetDetailLayer( st.x, st.y, len.x, len.y, layer );
	}
	public void setDetails( TerrainData td, int[,] ds, int layer )
	{
		td.SetDetailLayer( st.x, st.y, layer, ds );
	}


	public Vector2 getIterationPosition( int ix, int iy, Vector2 unit )
	{
		return new Vector2( (float)( st.x + ix ) * unit.x, (float)( st.y + iy ) * unit.y );
	}

	public Vector3 getIterationPosition3d( int ix, int iy, Vector2 unit, float h = 0.0f )
	{
		return new Vector3( (float)( st.x + ix ) * unit.x, h, (float)( st.y + iy ) * unit.y );
	}

}



// 地形変形を実際に行う ================================

interface _FieldAdjuster
{

	void adjustHeights( SimpleTerrainOperator op );

	void adjustAlphamaps( TerrainOperator op );

	void adjustDetails( FullTerrainOperator op );

}


struct FieldAdjusterForRoadMesh : _FieldAdjuster
{

	MeshCollider mc;

	Vector2	min;
	Vector2	max;

	Vector3 tofs;


	public FieldAdjusterForRoadMesh( MeshCollider mc, SimpleTerrainOperator op )
	{
		min = new Vector2( mc.bounds.min.x, mc.bounds.min.z ) - op.terrainPosition;
		max = new Vector2( mc.bounds.max.x, mc.bounds.max.z ) - op.terrainPosition;

		this.mc = mc;

		tofs = new Vector3( op.terrainPosition.x, op.terrainPositionHeight, op.terrainPosition.y );
	}

	public void adjustHeights( SimpleTerrainOperator op )
	{
		
		var m = new FieldManipulator( min, max, op.fieldUnitR, op.fieldLength );
		
		var hs = m.getHeights( op.td );
		
		var tofs = new Vector3( op.terrainPosition.x, op.terrainPositionHeight, op.terrainPosition.y );
		
		for( var iy = 0; iy < m.len.y; iy++ )
		for( var ix = 0; ix < m.len.x; ix++ )
		{
			
			var pos = m.getIterationPosition3d( ix, iy, op.fieldUnit ) + tofs;
			
			var start	= pos + Vector3.up * 512.0f;
			var end		= pos + Vector3.down * 512.0f;
			
			var ray = new Ray( start, end - start );
			var res = new RaycastHit();
			if( mc.Raycast( ray, out res, 1024.0f ) )
			{
				
				hs[ iy, ix ] = ( res.point.y - op.terrainPositionHeight ) * op.fieldUnitHeightR;
				
			}
			
		}
		
		m.setHeights( op.td, hs );

	}
	
	public void adjustAlphamaps( TerrainOperator op )
	{

		var m = new FieldManipulator( min, max, op.mapUnitR, op.mapLength );
		
		var ms = m.getAlphamaps( op.td );
		
		for( var iy = 0; iy < m.len.y; iy++ )
		for( var ix = 0; ix < m.len.x; ix++ )
		{
			
			var pos = m.getIterationPosition3d( ix, iy, op.mapUnit ) + tofs;
			
			var start	= pos + Vector3.up * 512.0f;
			var end		= pos + Vector3.down * 512.0f;
			
			var ray = new Ray( start, end - start );
			var res = new RaycastHit();
			if( mc.Raycast( ray, out res, 1024.0f ) )
			{
				
				ms[ iy, ix, 0 ] = 0.0f;
				ms[ iy, ix, 1 ] = 1.0f;
				ms[ iy, ix, 2 ] = 0.0f;
				ms[ iy, ix, 3 ] = 0.0f;
				
			}
			
		}
		
		m.setAlphamaps( op.td, ms );

	}
	
	public void adjustDetails( FullTerrainOperator op )
	{
		
	}

}


struct FieldAdjusterForExplosionShpere : _FieldAdjuster
{

	float	sqrradius;
	Vector2	sradius;
	
	Vector2	scenter;
	float	centerHeight;
	
	Vector3	center;
	
	Vector2	min;
	Vector2	max;

	float impact;

	
	FieldManipulator	hm;

	float[,]	hs;


	
	public FieldAdjusterForExplosionShpere( Vector3 center, float radius, float impact, SimpleTerrainOperator op )
	{
		
		sqrradius = radius * radius;
		sradius	= new Vector2( radius, radius );
		
		scenter = new Vector2( center.x, center.z ) - op.terrainPosition;
		centerHeight = center.y - op.terrainPositionHeight;

		this.center = new Vector3( scenter.x, centerHeight, scenter.y );
		
		min = scenter - sradius;
		max = scenter + sradius;

		this.impact = impact;


		hm = new FieldManipulator( min, max, op.fieldUnitR, op.fieldLength );
		
		hs = hm.getHeights( op.td );

	}



	public void adjustHeights( SimpleTerrainOperator op )
	{

		var sqrradiusR = 1.0f / sqrradius;

		var impactSqrradiusR = impact * sqrradiusR;

		var edgeFactor = sradius.x * ( 1.0f / 20.0f ) * sqrradiusR;
		

		for( var iy = 0; iy < hm.len.y; iy++ )
		for( var ix = 0; ix < hm.len.x; ix++ )
		// 地形ローカル空間で処理
		{
			
			var height = hs[ iy, ix ] * op.fieldUnitHeight;
			
			var pos = hm.getIterationPosition3d( ix, iy, op.fieldUnit, height );
			
			var spos = new Vector2( pos.x, pos.z );
			
			if( false &&	height > centerHeight )
			{
				var sqrsdist = ( spos - scenter ).sqrMagnitude;
				
				if( sqrsdist <= sqrradius )
				{

					var hith = centerHeight - ( sqrradius - sqrsdist ) * impactSqrradiusR;
					
					hs[ iy, ix ] = Mathf.Lerp( hith, height, sqrsdist * sqrradiusR ) * op.fieldUnitHeightR;

				}
			}
			else
			{
				if( ( pos - center ).sqrMagnitude <= sqrradius )
				{

					var sqrsdist = ( spos - scenter ).sqrMagnitude;

					var hith = centerHeight - ( sqrradius - sqrsdist ) * impactSqrradiusR;

					hs[ iy, ix ] = ( hith < height? hith + sqrsdist * edgeFactor : height ) * op.fieldUnitHeightR;

				}
			}
		}
		
		hm.setHeights( op.td, hs );

	}
	
	public void adjustAlphamaps( TerrainOperator op )
	{

		var mratio = new Vector2( op.fieldUnit.x / op.mapUnit.x, op.fieldUnit.y / op.mapUnit.y );
		
		var m = new FieldManipulator( min, max, op.mapUnitR, op.mapLength );
		
		var ms = m.getAlphamaps( op.td );
		
		for( var iy = 0; iy < m.len.y; iy++ )
		for( var ix = 0; ix < m.len.x; ix++ )
		{
			
			var spos = m.getIterationPosition( ix, iy, op.mapUnit );
			
			var height = hs[ (int)(iy * mratio.y), (int)(ix * mratio.x) ] * op.fieldUnitHeight;
			
			var pos = new Vector3( spos.x, height, spos.y );
			
			if( ( pos - center ).sqrMagnitude <= sqrradius )
			{
				ms[ iy, ix, 0 ] = 0.3f;
				ms[ iy, ix, 1 ] = 0.0f;
				ms[ iy, ix, 2 ] = 0.0f;
				ms[ iy, ix, 3 ] = 0.0f;
			}
			
		}
		
		m.setAlphamaps( op.td, ms );

	}
	
	public void adjustDetails( FullTerrainOperator op )
	{

		var dratio = new Vector2( op.fieldUnit.x / op.detailUnit.x, op.fieldUnit.y / op.detailUnit.y );
		
		var m = new FieldManipulator( min, max, op.detailUnitR, op.detailLength );
		
		var layers = m.getDetailSupportedLayers( op.td );
		
		foreach( var layer in layers )
		{
			var ds = m.getDetails( op.td, layer );
			
			for( var iy = 0; iy < m.len.y; iy++ )
			for( var ix = 0; ix < m.len.x; ix++ )
			{
				
				var spos = m.getIterationPosition( ix, iy, op.detailUnit );
				
				var height = hs[ (int)(iy * dratio.y), (int)(ix * dratio.x) ] * op.fieldUnitHeight;
				
				var pos = new Vector3( spos.x, height, spos.y );
				
				if( ( pos - center ).sqrMagnitude <= sqrradius )
				{
					ds[ iy, ix ] = 0;
				}
				
			}
			
			m.setDetails( op.td, ds, layer );
		}

	}
	
}


struct FieldAdjusterFor : _FieldAdjuster
{
	
	public void adjustHeights( SimpleTerrainOperator op )
	{
		
	}
	
	public void adjustAlphamaps( TerrainOperator op )
	{
		
	}
	
	public void adjustDetails( FullTerrainOperator op )
	{
		
	}
	
}







