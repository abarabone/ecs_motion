using UnityEngine;
using System.Collections;
using UnityEditor;


/*
[ CustomEditor(typeof(Building3)) ]
public class Building3Inspector : Editor
{

	static float	textureSize;


	public override void OnInspectorGUI()
	{
		
		
		base.OnInspectorGUI();
		
		
		
		EditorGUILayout.BeginHorizontal();
		

		textureSize = EditorGUILayout.FloatField( "Far Texture\tSize", textureSize );

		if( GUILayout.Button( "Export" ) )
		{

			//BuildingMapCapturer.open( textureSize );
			ttt();

		}


		EditorGUILayout.EndHorizontal();
		
	}


	void ttt()
	{

		var go = new GameObject();

		var cam = go.AddComponent<Camera>();


		cam.aspect = 1.0f;

		cam.backgroundColor = Color.black;

		cam.farClipPlane = 1000.0f;

		cam.nearClipPlane = 0.01f;

		cam.orthographic = true;

		cam.clearFlags = CameraClearFlags.Color;

		Camera.SetupCurrent( cam );


		var tf = cam.transform;

		tf.position = Vector3.back * 3;//500.0f;

		tf.rotation = Quaternion.identity;


		var sp = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		
		Graphics.DrawMeshNow( sp.GetComponent<MeshFilter>().sharedMesh, Vector3.zero, Quaternion.identity );

		Application.CaptureScreenshot( "sstest.png" );


		//DestroyImmediate( sp );

		//DestroyImmediate( go );

	}



}
*/