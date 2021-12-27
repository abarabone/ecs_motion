using UnityEngine;
using System.Collections;




public class PathMeshMaker3 : _PathMeshMaker3
{
	
	
	public override GameObject build( int i )
	{
		
		var partObject = (GameObject)GameObject.Instantiate( unitObject );
		
		
		var tfSrcs = unitObject.GetComponentsInChildren<Transform>();
		
		var tfDsts = partObject.GetComponentsInChildren<Transform>();
		
		for( var ii = 0; ii < tfSrcs.Length; ii++ )
		{
			
			var mrSrc = tfSrcs[ii].GetComponent<MeshRenderer>();
			
			var partPos = interpolatePosition3d( i, mrSrc != null? mrSrc.bounds.center: tfSrcs[ii].position, 0.0f );
			
			
			var mf = tfDsts[ii].GetComponent<MeshFilter>();
			
			if( mf != null )
			{
				mf.sharedMesh = buildMesh( i, mf.sharedMesh, partPos, 0.0f );
			}
			
			
			var tf = tfDsts[ii].transform;
			
			tf.rotation = tfSegment.rotation;
			
			tf.position = tfSegment.rotation * partPos + tfSegment.position;
			
		}
		
		return partObject;
		
	}
	
}






static public class PathMeshTemplateMaker3
{
	
	
	static System.Collections.Generic.Dictionary< GameObject, GameObject >	unitObjects;
	

	
	static public GameObject build( GameObject srcObject )
	{
		
		if( unitObjects == null )
		{	
			unitObjects = new System.Collections.Generic.Dictionary<GameObject, GameObject>( 64 );	
		}
		
		
		if( unitObjects.ContainsKey( srcObject ) )
		{
			
			srcObject = unitObjects[ srcObject ];
			
		}
		else
		{
			
			var unitObject = _build( srcObject );
			
			unitObjects[ srcObject ] = unitObject;
			
			srcObject = unitObject;
			
		}
		
		
		return (GameObject)GameObject.Instantiate( srcObject );
		
	}
	
	static GameObject _build( GameObject srcObject )
	{
		
		var unitObject = (GameObject)GameObject.Instantiate( srcObject );
		
		
		var tfs = unitObject.GetComponentsInChildren<Transform>();
		
		foreach( var tf in tfs )
		{
			
			var mt = tf.localToWorldMatrix;
			
			if( !mt.isIdentity )
			{
				
				var mf = tf.GetComponent<MeshFilter>();
				
				if( mf != null )
				{
					mf.sharedMesh = PathMeshTemplateMaker3Inner.buildMesh( mf.sharedMesh, ref mt );
				}
				
				var mc = tf.GetComponent<MeshCollider>();
				
				if( mc != null )
				{
					mc.enabled = false;
				}
				
			}
			
		}
		
		foreach( var tf in tfs )
		{
			
			tf.position = Vector3.zero;
			
			tf.rotation = Quaternion.identity;
			
			tf.localScale = Vector3.one;

		}
		
		
		return unitObject;
		
	}
	
	
	static public void cleanup()
	{
		if( unitObjects != null )
		{

			foreach( var unit in unitObjects.Values )
			{
				GameObject.Destroy( unit );
			}
			
			
			unitObjects.Clear();//Debug.Log( "cleanup PathUnitMeshTemplateCreator3" );


			unitObjects = null;

		}
	}
	
	
}






