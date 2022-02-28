using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

public class AnimationClipViewer : MonoBehaviour
{
	[MenuItem( "Tools/AnimationClip/View" )]
	static void animationclip_view()
	{
		Object[] sel = Selection.GetFiltered( typeof( AnimationClip ), SelectionMode.Unfiltered );

		foreach( AnimationClip clip in sel )
		{
			foreach( AnimationClipCurveData curve in AnimationUtility.GetAllCurves( clip ) )
			{
				var format = "path:{0}\npropertyName:{1} curve.length:{2} curve.keys.length:{3}\n\n";
				Debug.Log( string.Format( format, curve.path, curve.propertyName, curve.curve.length, curve.curve.keys.Length ) );
				
			//	Debug.Log( curve.path );
			}
		}
	}

	/*
	[MenuItem( "Tools/test enumur" )]
	static void testenumur()
	{
		Transform sel = Selection.activeTransform;
		
		foreach( var tf in sel.GetEnumerator() ) Debug.Log( tf );
	}*/
}