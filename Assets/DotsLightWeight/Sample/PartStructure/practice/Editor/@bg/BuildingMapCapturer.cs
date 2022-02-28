using UnityEngine;
using System.Collections;
using UnityEditor;

/*
public class BuildingMapCapturer : EditorWindow
{



	static public void open( float textreSize )
	{

		var window = CreateInstance<BuildingMapCapturer>(); 


		var w = Screen.width;

		var h = Screen.height;

		var rect = new Rect( w * 0.5f - textreSize * 0.5f, h * 0.5f - textreSize * 0.5f, textreSize, textreSize );

		window.position = rect;


		window.ShowPopup();

	}


	void OnGUI()
	{

		if( Event.current.control ) this.Close();


		var go = GameObject.CreatePrimitive( PrimitiveType.Sphere );

		Graphics.DrawMeshNow( go.GetComponent<MeshFilter>().sharedMesh, Vector3.zero, Quaternion.identity );

		DestroyImmediate( go );

	}


}
*/