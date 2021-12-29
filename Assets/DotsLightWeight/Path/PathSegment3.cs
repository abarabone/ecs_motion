using UnityEngine;
using System.Collections;


public class PathSegment3 : MonoBehaviour
{
	
	public GameObject	unitObject;
	
	public Transform	startAnchor;
	public Transform	endAnchor;
	
	public Transform	effectStartSide;
	public Transform	effectEndSide;
	
	public float		segmentUnitSize;
	
	//	public float	offsetHeight;
	
	



	void Awake()
	{

		//if( Application.isEditor )
		//foreach( var rp in GetComponentsInChildren<EditorOnlyObject>( true ) )
		//{
		//	Destroy( rp.gameObject );
		//}


		unitObject = PathMeshTemplateMaker3.build( unitObject );

		unitObject.transform.parent = transform;

	}


	public void build()
	{



		convert( new PathMeshMaker3(), unitObject );
		

		
		unitObject.SetActive( false );//
		
		GameObject.Destroy( unitObject );

		unitObject = null;


		PathMeshTemplateMaker3.cleanup();


		Destroy( this );

	}

	public void buildInEditor()
	{

		var unitObjectTemplate = PathMeshTemplateMaker3InEditor.build( unitObject );


		convert( new PathMeshMaker3InEditor(), unitObjectTemplate );


		PathMeshTemplateMaker3InEditor.cleanup( unitObjectTemplate );

	}


	void convert( _PathMeshMaker3 converter, GameObject unitObjectTemplate )
	{

		var tf = transform;


		converter.setUnitObject( unitObjectTemplate );

		converter.setPathControlParams( startAnchor, effectStartSide, endAnchor, effectEndSide, segmentUnitSize );

		converter.buildAnchorParams( tf );


		var ifreq = converter.frequency;

		for( var i = 0; i < ifreq; i++ )
		{

			var unit = converter.build( i );

			unit.transform.parent = tf;

		}

	}

}







public abstract class _PathMeshMaker3
{
	
	protected Vector3	startPosition;
	protected Vector3	endPosition;
	
	protected Vector3	effectStart;
	protected Vector3	effectEnd;
	
	protected float	tunit;
	protected float	unitRatio;
	
	public int	frequency	{ get; private set; }
	
	
	protected GameObject	unitObject;
	
	protected Transform	tfSegment;
	



	public abstract GameObject build( int i );


	
	public void setPathControlParams( Transform tfStart, Transform tfEffectStart, Transform tfEnd, Transform tfEffectEnd, float unitSize )
	{
		
		startPosition	= tfStart.position;
		
		endPosition		= tfEnd.position;
		
		
		var dist = Vector3.Distance( endPosition, startPosition );
		
		
		effectStart	= ( tfEffectStart == null )? dist * tfStart.forward: tfEffectStart.position - startPosition;
		
		effectEnd	= ( tfEffectEnd == null )? dist * tfEnd.forward: tfEffectEnd.position - endPosition;
		
		
		var freq = Mathf.Floor( dist / unitSize );	// 切捨て
		
		if( freq == 0.0f ) freq = 1.0f;
		
		tunit = 1.0f / freq;	// unitSize を切り捨て分調整したもの
		
		
		unitRatio = tunit * ( 1.0f / unitSize );	// unitSize がどれだけ調整されたかの比
		
		
		frequency	= (int)freq;
		
	}


	
	public void buildAnchorParams( Transform tfBase )
	{
		
		var mtInv = tfBase.worldToLocalMatrix;
		
		
		startPosition = mtInv.MultiplyPoint3x4( startPosition );
		
		endPosition = mtInv.MultiplyPoint3x4( endPosition );
		
		
		effectStart = mtInv.MultiplyVector( effectStart );
		
		effectEnd = mtInv.MultiplyVector( effectEnd );
		
		
		tfSegment = tfBase;

	}


	public void setUnitObject( GameObject unit )
	{

		unitObject = unit;

	}



	
	protected Mesh buildMesh( int i, Mesh srcMesh, Vector3 partPos, float offsetHeight )
	{
		
		var dstvtxs	= new Vector3[ srcMesh.vertexCount ];
		
		
		for( var iv = srcMesh.vertexCount; iv-- > 0; )
		{
			
			dstvtxs[ iv ]	= interpolatePosition3d( i, srcMesh.vertices[iv], offsetHeight ) - partPos;
			
		}
		
		
		var dstMesh = new Mesh();
		
		dstMesh.vertices	= dstvtxs;
		
		dstMesh.uv			= srcMesh.uv;

		dstMesh.triangles	= srcMesh.triangles;


		dstMesh.RecalculateNormals();
		
		
		return dstMesh;
		
	}
	
	protected Vector3 interpolatePosition3d( int i, Vector3 vtx, float offsetHeight )
	{
		
		var t = vtx.z * unitRatio + tunit * (float)i;
		
		var p0 = startPosition;
		var p1 = endPosition;
		var v0 = effectStart;
		var v1 = effectEnd;
		
		var att	= ( 2.0f * p0 - 2.0f * p1 + v0 + v1 ) * t * t;
		var bt	= ( -3.0f * p0 + 3.0f * p1 - 2.0f * v0 - v1 ) * t;
		
		var pos	= att * t + bt * t + v0 * t + p0;
		
		var d	= 3.0f * att + 2.0f * bt + v0;
		
		pos += vtx.x * new Vector3( d.z, 0.0f, -d.x ).normalized;
		
		return new Vector3( pos.x, pos.y + vtx.y + offsetHeight, pos.z );
		
	}
	
}











static public class PathMeshTemplateMaker3Inner
{
	
	static public Mesh buildMesh( Mesh srcmesh, ref Matrix4x4 mt )
	{
		
		var dstmesh = new Mesh();
		
		dstmesh.MarkDynamic();
		
		
		var	dstvtxs = new Vector3[ srcmesh.vertexCount ];
		
		for( var iv = srcmesh.vertexCount; iv-- > 0; )
		{
			
			dstvtxs[iv] = mt.MultiplyPoint3x4( srcmesh.vertices[iv] );
			
		}
		
		
		dstmesh.vertices = dstvtxs;
		
		dstmesh.uv = srcmesh.uv;
		
		//dstmesh.triangles = srcmesh.triangles;
		buildTriangles( dstmesh, srcmesh, ref mt );

		
		return dstmesh;
		
	}

	static void buildTriangles( Mesh dstmesh, Mesh srcmesh, ref Matrix4x4 mt )
	{

		dstmesh.subMeshCount = srcmesh.subMeshCount;


		var isReverse = MeshUtility.isReverse( ref mt );//( mt[0,0] < 0.0f ) ^ ( mt[1,1] < 0.0f ) ^ ( mt[2,2] < 0.0f );
		
		if( isReverse )
		{

			for( var ii = 0; ii < srcmesh.subMeshCount; ii++ )
			{
				var idxs = srcmesh.GetTriangles( ii );
				
				for( var i = 0; i < idxs.Length; i += 3 )
				{
					var i1 = idxs[ i + 1 ];
					idxs[ i + 1 ] = idxs[ i + 2 ];
					idxs[ i + 2 ] = i1;
				}
				
				dstmesh.SetTriangles( idxs, ii );	
			}

		}
		else
		{

			for( var ii = 0; ii < srcmesh.subMeshCount; ii++ )
			{
				dstmesh.SetTriangles( srcmesh.GetTriangles( ii ), ii );	
			}

		}

	}

}














