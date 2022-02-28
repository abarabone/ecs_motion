using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class UPoleSetter : EditorWindow
{

	[MenuItem( "Tools/Utility Pole Setter" )]
	static void open()
	{

		var window = EditorWindow.GetWindow<UPoleSetter>( "Utility Pole Setter" );

		window.init();

	}




	Plot3Inspector.UtilityPolePlantEditor	plantEditor;



	void init()
	{
		plantEditor = new Plot3Inspector.UtilityPolePlantEditor( this );

		SceneView.onSceneGUIDelegate += OnSceneView;
	}
	void fin()
	{
		SceneView.onSceneGUIDelegate -= OnSceneView;
	}



	void OnGUI()
	{

		plantEditor.onEditor();

	}

	void OnSceneView( SceneView sceneView )
	{
		
		plantEditor.onScene( Selection.activeTransform );

	}

	void OnSelectionChange()
	{
		Debug.Log("a");
	}


	void OnDestroy()
	{
		fin();
	}







}
