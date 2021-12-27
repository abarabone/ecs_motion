using UnityEngine;
using System.Collections;


static public class PathMeshTemplateMaker3InEditor
{
	
	static public GameObject build( GameObject srcObject )
	{
		
		var unitObject = (GameObject)GameObject.Instantiate( srcObject, Vector3.zero, Quaternion.identity );


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
					mc.sharedMesh = PathMeshTemplateMaker3Inner.buildMesh( mc.sharedMesh, ref mt );
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
	

	static public void cleanup( GameObject unitObject )
	{

		unitObject.SetActive( false );//

		GameObject.DestroyImmediate( unitObject );

	}

}


public class PathMeshMaker3InEditor : _PathMeshMaker3
{
	
	
	
	public override GameObject build( int i )
	{
		
		var partObject = (GameObject)GameObject.Instantiate( unitObject );
		
		
		var tfSrcs = unitObject.GetComponentsInChildren<Transform>();
		
		var tfDsts = partObject.GetComponentsInChildren<Transform>();
		
		for( var ii = 0; ii < tfSrcs.Length; ii++ )
		{
			
			var mrSrc = tfSrcs[ii].GetComponent<MeshRenderer>();
			
			var partPos = interpolatePosition3d( i, mrSrc != null ? mrSrc.bounds.center : tfSrcs[ii].position, 0.0f );
			
			
			var mf = tfDsts[ii].GetComponent<MeshFilter>();
			
			if( mf != null )
			{
				mf.sharedMesh = buildMesh( i, mf.sharedMesh, partPos, 0.0f );
			}
			
			var mc = tfDsts[ii].GetComponent<MeshCollider>();
			
			if( mc != null )
			{
				mc.sharedMesh = buildMesh( i, mc.sharedMesh, partPos, 0.0f );//-0.01f );
			}
			
			
			var tf = tfDsts[ii].transform;
			
			tf.rotation = tfSegment.rotation;
			
			tf.position = tfSegment.rotation * partPos + tfSegment.position;
			
		}
		
		partObject.tag = "EditorOnly";
		
		partObject.AddComponent<EditorOnlyObject>();
		
		
		return partObject;
		
	}

}
