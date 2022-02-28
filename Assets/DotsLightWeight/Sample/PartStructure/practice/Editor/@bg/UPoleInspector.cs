using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof( PlotPartUtlPole3 ) )]
public class UPoleInspector : Editor
{

	//Plot3Inspector.UtilityPolePlantEditor	plantEditor;


	public void OnEnable()
	{

		//plantEditor = new Plot3Inspector.UtilityPolePlantEditor( this );

	}


	public override void OnInspectorGUI()
	{

		base.OnInspectorGUI();


		//plantEditor.onEditor();

	}


	public void OnSceneGUI()
	{

		//var tfParent = getParent();

		//plantEditor.onScene( tfParent );


		var pole = getPole();

		if( pole.transform.hasChanged )//GUI.changed )
		{

			adjustLinkedCablesets();

			pole.transform.hasChanged = false;

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
					cableset.adjustCablePositions();
				}

			}

		}

	}




	Transform getParent()
	{
		var go = ( (PlotPartUtlPole3)target ).gameObject;

		return go.transform.parent;
	}

	PlotPartUtlPole3 getPole()
	{
		return ( (PlotPartUtlPole3)target );
	}
	


}