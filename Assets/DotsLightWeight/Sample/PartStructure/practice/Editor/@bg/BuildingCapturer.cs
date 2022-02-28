using UnityEngine;
using System.Collections;
using UnityEditor;

/*
[ CustomEditor(typeof(StructurePartContentsSource)) ]
public class BuildingCapturer : Editor
{

	static int	textureSize = 256;


	int	partsLength;

	
	public override void OnInspectorGUI()
	{
		
		
		base.OnInspectorGUI();
		
		
		
		EditorGUILayout.BeginHorizontal();
		
		
		textureSize = EditorGUILayout.IntField( "Far Texture\tSize", textureSize );
		
		if( GUILayout.Button( "Export" ) )
		{

			capture();

		}
		
		
		EditorGUILayout.EndHorizontal();



		EditorGUILayout.BeginHorizontal();
		
		
		EditorGUILayout.IntField( "Parts Length", partsLength );
		
		if( GUILayout.Button( "Get Parts Count" ) )
		{
			
			partsLength = countParts();
			
		}
		
		
		EditorGUILayout.EndHorizontal();


	}



	int countParts()
	{
		
		var contents = (StructurePartContentsSource)target;

		var parts = contents.GetComponentsInChildren<_StructurePart3>();

		return parts.Length;

	}




	void capture()
	{

		var bounds = calculateBounds();

		var cam = createCamera();

		captureScreen( cam, bounds, "front", Vector3.forward, Vector3.up, bounds.size.x, bounds.size.y );
		captureScreen( cam, bounds, "right", Vector3.right, Vector3.up, bounds.size.z, bounds.size.y );
		captureScreen( cam, bounds, "top", Vector3.up, Vector3.back, bounds.size.x, bounds.size.z );
		captureScreen( cam, bounds, "rear", Vector3.back, Vector3.up, bounds.size.x, bounds.size.y );
		captureScreen( cam, bounds, "left", Vector3.left, Vector3.up, bounds.size.z, bounds.size.y );
		captureScreen( cam, bounds, "bottom", Vector3.down, Vector3.forward, bounds.size.x, bounds.size.z );

		//DestroyImmediate( cam.gameObject );

	}



	Bounds calculateBounds()
	{
		
		var contents = (StructurePartContentsSource)target;

		var mrs = contents.GetComponentsInChildren<MeshRenderer>();


		var bounds = mrs[ 0 ].bounds;

		for( var i = 1; i < mrs.Length; i++ )
		{

			bounds.Encapsulate( mrs[ i ].bounds );

		}

		return bounds;
	}



	Camera createCamera()
	{

		var cam = new GameObject( "capture camera" ).AddComponent<Camera>();

		cam.orthographic = true;

		cam.farClipPlane = 100.0f;

		cam.nearClipPlane = -100.0f;


		cam.backgroundColor = Color.white;

		cam.clearFlags = CameraClearFlags.Color;


		return cam;

	}


	void captureScreen( Camera cam, Bounds bounds, string side, Vector3 campos, Vector3 up, float wsize, float hsize )
	{

		var w = cam.pixelWidth;//Screen.width;

		var h = cam.pixelHeight;//Screen.height;

		Debug.Log( w +" "+ h );


		
		cam.orthographicSize = hsize * 0.5f * ( h / textureSize );
		
		cam.aspect = ( w / h ) * ( wsize / hsize );



		var tfCam = cam.transform;
		
		tfCam.position = bounds.center + campos * 1.0f;

		tfCam.LookAt( bounds.center, up );
		
		cam.Render();



		var start = new Vector2( (w - textureSize) * 0.5f + 1.0f, (h - textureSize) * 0.5f + 1.0f );

		var size = new Vector2( textureSize, textureSize );

		var caprect = new Rect( start.x, start.y, size.x, size.y );

		Debug.Log(caprect);


		var sstex = new Texture2D( textureSize, textureSize, TextureFormat.RGB24, false );

		sstex.ReadPixels( caprect, 0, 0 );

		sstex.Apply();

		var bytes = sstex.EncodeToPNG();

		DestroyImmediate( sstex );


		System.IO.File.WriteAllBytes( target.name + "_" + side + ".png", bytes );

		//Application.CaptureScreenshot( "sss.png" );

	}

}
*/