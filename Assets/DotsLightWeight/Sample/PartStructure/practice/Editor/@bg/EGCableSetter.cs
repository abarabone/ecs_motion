using UnityEngine;
using System.Collections;
using UnityEditor;

/*
[ CustomEditor( typeof(PlotPartCableset3) ) ]
public class EGCableSetter : Editor
{
	
	

	static public Transform	tfStartSideHub;
	//public Transform	tfStartSideHub;

	bool	isContinuous;



	public void OnSceneGUI()
	{
		//Debug.Log( tfStartSideHub );

		var cableset = (PlotPartCableset3)target;


		var e = Event.current;



		if( e.type == EventType.ValidateCommand )
		{
			isContinuous = false;

			tfStartSideHub = null;
		}


		if( e.button == 1 && e.type == EventType.MouseDown )
		{

			var tfHub = EGPoleSetter.getHubTransformByMousePick();


			if( tfHub != null )
			{

				e.Use();


				tfStartSideHub = tfHub;


				if( isContinuous )
				{

					continuousSettingNextCableset();

				}

			}
			else
			{

				isContinuous = false;

				tfStartSideHub = null;

			}

		}
		else if( e.button == 1 && e.type == EventType.MouseUp )
		{

			var tfHub = EGPoleSetter.getHubTransformByMousePick();


			if( tfHub == null )
			{

				if( tfStartSideHub != null )
				{

					tfHub = getHubTransformInClonePole();

				}

			}

			if( tfHub != tfStartSideHub )
			{

				if( tfStartSideHub != null )
				{

					e.Use();


					var tfEndSideHub = tfHub;

					RecordUndoBeforeAdjust( cableset );
					Undo.RecordObject( cableset, "Cableset Link" );
					cableset.adjustCablePositions( tfStartSideHub, tfEndSideHub );


					EditorUtility.SetDirty( cableset );


					isContinuous = true;

				}

			}


			tfStartSideHub = null;

		}
		else if( e.button == 1 && e.type == EventType.MouseDrag )
		{

			if( tfStartSideHub != null )
			{

				e.Use();

			}
			
		}
		else if( e.isMouse && e.type == EventType.MouseMove )
		{



		}

	}
	




	void continuousSettingNextCableset()
	{

		var cableset = (PlotPartCableset3)target;

		var tfcableset = cableset.transform;


		var newcableset = ((PlotPartCableset3)Instantiate( cableset, tfcableset.position, tfcableset.rotation ));
		Undo.RegisterCreatedObjectUndo( newcableset.gameObject, "Clone Cableset" );
		
		newcableset.gameObject.name = cableset.gameObject.name;
		
		newcableset.transform.parent = tfcableset.parent;
		//Undo.SetTransformParent( newcableset.transform, tfcableset.parent, "Parent Cableset" );
		
	}





	Transform getHubTransformInClonePole()
	{

		var e = Event.current;


		var startPole = tfStartSideHub.GetComponentInParent<PlotPartUtlPole3>();


		var ray = HandleUtility.GUIPointToWorldRay( e.mousePosition );

		ray.origin -= tfStartSideHub.rotation * startPole.transform.InverseTransformPoint( tfStartSideHub.position );

		var res = HandleUtility.RaySnap( ray );


		if( res != null )
		{

			var endPole = (PlotPartUtlPole3)Instantiate( startPole );
			Undo.RegisterCreatedObjectUndo( endPole.gameObject, "Clone Pole" );

			endPole.gameObject.name = startPole.gameObject.name;
			
			
			var tfEndPole = endPole.transform;
			
			tfEndPole.position = ((RaycastHit)res).point;
			
			tfEndPole.rotation = tfStartSideHub.rotation;
			
			tfEndPole.parent = startPole.transform.parent;
			//Undo.SetTransformParent( tfEndPole, startPole.transform.parent, "Parent Pole" );

			
			var starHubs = startPole.GetComponentsInChildren<CableHubPoint3>();
			
			var endHubs = endPole.GetComponentsInChildren<CableHubPoint3>();
			
			for( var i = 0; i < starHubs.Length; i++ )
			{
				
				if( starHubs[ i ].transform == tfStartSideHub )
				{
					
					return endHubs[ i ].transform;

				}
				
			}
			
		}


		return null;

	}



	static public void RecordUndoBeforeAdjust( PlotPartCableset3 cableset )
	{

		var cables = cableset.GetComponentsInChildren<CablePiece3>();

		foreach( var cable in cables )
		{
			Undo.RecordObject( cable.transform, "Cable Move" );
		}


		Undo.RecordObject( cableset.transform, "Cableset Move" );
		
	}




	const string	cablePath = "Assets/20_prefab/@bg/ver3/@eg/cable ";
	const string	cableExt = ".prefab";

	static public PlotPartCableset3 createNewCableset( CableHubPoint3 startSideHub )
	{

		var newCableset = new GameObject( "cable set" ).AddComponent<PlotPartCableset3>();
		Undo.RegisterCompleteObjectUndo( newCableset.gameObject, "Create Cableset" );


		var tfNewCableset = newCableset.transform;

		var anchors = startSideHub.getComponentsInDirectChildren<Transform>();

		foreach( var anchor in anchors )
		{

			var cablePrefab = (GameObject)AssetDatabase.LoadAssetAtPath( cablePath + anchor.name + cableExt, typeof(GameObject) );

			if( cablePrefab != null )
			{

				var cableInstance = (GameObject)Instantiate( cablePrefab );

				cableInstance.transform.parent = tfNewCableset;

			}

		}


		return newCableset;

	}


}





static public class CablesetEditorExtention
{



}

*/

