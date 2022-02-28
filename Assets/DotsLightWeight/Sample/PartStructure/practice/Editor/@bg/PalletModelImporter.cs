using UnityEngine;
using System.Collections;
using UnityEditor;



public class PalletModelImporter : AssetPostprocessor
{


	public void OnPostprocessModel( GameObject go )
	{

		var mfs = go.GetComponentsInChildren<MeshFilter>();

		foreach( var mf in mfs )
		{
			Debug.Log( mf.name );
			convertColors( mf.sharedMesh );

		}

	}

	void convertColors( Mesh mesh )
	{

		var cols = mesh.colors32;


		if( cols.Length > 0 )
		{

			for( var i = 0; i < cols.Length; i++ )
			{

				var c = cols[ i ];

				var palletId = ( c.r & c.g & c.b ) != 0 ? 0 : 
					4 - (( ( c.r & 1 ) * 3 ) | ( ( c.g & 1 ) * 2 ) | ( ( c.b & 1 ) * 1 ));
				// 後日推測
				// 白：0（デフォルトパレット）、赤：1、緑：2、青：3、黒：4


				cols[ i ] = new Color32( (byte)palletId, 0, 0, c.a );
				
			}

		}
		else
		{

			cols = new Color32[ mesh.vertexCount ];

			for( var i = 0; i < cols.Length; i++ )
			{

				cols[ i ] = new Color32( 0, 0, 0, 255 );
				
			}

		}


		mesh.colors32 = cols;

	}

	public Material OnAssignMaterialModel( Material mat, Renderer r )
	{
		/*
		if( mat.shader.name != "Custom/Pallet Diffuse v3" )
		{

			var s = Shader.Find( "Custom/Pallet Diffuse v3" );

			if( s != null ) mat.shader = s;

			AssetDatabase.CreateAsset( mat, "Assets/" + mat.name + ".mat");

			return mat;
		}
*/
		return null;

	}


}







[InitializeOnLoad]
static public class PalletPropertyApplier
{
	
	static PalletPropertyApplier()
	{

		apply();


		EditorApplication.playmodeStateChanged += () =>
		{
			if( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
			{

				apply();

				Debug.Log( "palletting" );

			}
		};

	}


	static void apply()
	{

		var district = GameObject.Find( "district3" );

		if( district != null )
		{

			var mpb = new MaterialPropertyBlock();


			var ss = district.GetComponentsInChildren<_Structure3>();

			foreach( var s in ss )
			{

				if( s.colorPallets != null )
				{

					var mrs = s.GetComponentsInChildren<MeshRenderer>();//s.getComponentInDirectChildren<MeshRenderer>();
					
					mpb.Clear();
					
					s.colorPallets.setPropertyBlock( mpb );

					foreach( var mr in mrs )
					{

						mr.SetPropertyBlock( mpb );
						s.colorPallets.setProperty( mr.sharedMaterial );//

					}

				}
			}

		}

	}

}
