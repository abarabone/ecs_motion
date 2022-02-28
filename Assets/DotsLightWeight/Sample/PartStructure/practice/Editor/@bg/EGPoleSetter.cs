using UnityEngine;
using System.Collections;
using UnityEditor;


/*

[ CustomEditor( typeof(PlotPartUtlPole3) ) ]
public class EGPoleSetter : Editor
{

	



	Transform	tfStartSideHub;
	
	void OnSceneGUI()
	{

		
		var pole = (PlotPartUtlPole3)target;

		var e = Event.current;



		if( e.button == 0 && e.type == EventType.MouseDown )
		{

			RecordUndoBforeAdjustCables();

		}
		else if( pole.transform.hasChanged )//GUI.changed )
		{

			adjustLinkedCablesets();Debug.Log("hasc");
			
			pole.transform.hasChanged = false;

		}



		if( e.button == 1 && e.type == EventType.MouseDown )
		{

			tfStartSideHub = getHubTransformByMousePick();

			if( tfStartSideHub != null )
			{

				var newCableset = new GameObject( "cable set" ).AddComponent<PlotPartCableset3>();
				Undo.RegisterCompleteObjectUndo( newCableset.gameObject, "Create Cableset" );
				
				Selection.activeObject = newCableset;
				
				//newCableset.tfStartSideHub = tfStartSideHub;
				EGCableSetter.tfStartSideHub = tfStartSideHub;

			}

		}


	}



	void RecordUndoBforeAdjustCables()
	{
		var pole = (PlotPartUtlPole3)target;

		if( pole.transform.parent != null )
		{
			
			var cablesets = pole.transform.parent.GetComponentsInChildren<PlotPartCableset3>();
			
			foreach( var cableset in cablesets )
			{
				EGCableSetter.RecordUndoBeforeAdjust( cableset );// 無意味かも　現状、Undo 誤に adjustLinkedCablesets() が反応してる
			}
			
		}
	}

	void adjustLinkedCablesets()
	{

		var pole = (PlotPartUtlPole3)target;


		if( pole.transform.parent != null )
		{
			
			var cablesets = pole.transform.parent.GetComponentsInChildren<PlotPartCableset3>();
			
			foreach( var cableset in cablesets )
			{
				
				var isStartSidePole = cableset.tfStartSideHub != null && cableset.tfStartSideHub.GetComponentInParent<PlotPartUtlPole3>() == pole;
				
				var isEndSidePole = cableset.tfEndSideHub != null && cableset.tfEndSideHub.GetComponentInParent<PlotPartUtlPole3>() == pole;
				
				if( isStartSidePole || isEndSidePole )
				{
					//Undo.RecordObject( cableset.transform, "Cableset Move" );
					cableset.adjustCablePositions();
				}
				
			}
			
		}

	}



	static public Transform getHubTransformByMousePick()
	{
		
		var o = 60.0f * 0.5f;
		var mp = Event.current.mousePosition;
		var rc = new Rect( mp.x - o, mp.y - o, o * 2.0f, o * 2.0f );
		
		
		var gos = HandleUtility.PickRectObjects( rc, false );//Debug.Log( gos.Length );
		
		if( gos == null ) return null;
		
		
		
		CableHubPoint3	hub = null;
		
		foreach( var go in gos )
		{
			//var tf = go.transform;
			
			//hub = tf.GetComponent<CableHubPoint3>() ?? ( tf.parent != null ? tf.parent.GetComponent<CableHubPoint3>() : hub );
			hub = go.GetComponent<CableHubPoint3>() ?? go.GetComponentInParent<CableHubPoint3>() ?? hub;

			if( hub != null ) break;
		}
		
		
		return hub != null ? hub.transform : null;
		
	}



}



static public class UtlPoleEditorExtention
{


	
}


*/





