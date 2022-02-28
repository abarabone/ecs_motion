using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;

[ CustomEditor(typeof(MeshFilter)) ]
public class MqoExporter : Editor
{

	static float	scale = 100.0f;

	static string	saveFolderPath;


	public override void OnInspectorGUI()
	{
		
		
		base.OnInspectorGUI();
		
		
		
		EditorGUILayout.BeginHorizontal();
		

		scale = EditorGUILayout.FloatField( "MQO\tScale", scale );

		if( GUILayout.Button( "Export" ) )
		{

			export();

		}
		
		EditorGUILayout.EndHorizontal();

	}



	void export()
	{

		var mf = (MeshFilter)target;

		if( mf != null )
		{

			var filename = EditorUtility.SaveFilePanel( "Select Export File", saveFolderPath, mf.name + ".mqo", "mqo" );


			if( filename.Length != 0 )
			{

				saveFolderPath = System.IO.Path.GetDirectoryName( filename );


				var mqo = formMqoString( mf );

				System.IO.File.WriteAllText( filename, mqo, Encoding.ASCII );

			}

		}

	}


	string formMqoString( MeshFilter mf )
	{

		var  s = new StringBuilder( 65536 );

		s.AppendLine( "Metasequoia Document").AppendLine( "Format Text Ver 1.0" ).AppendLine();

		addMaterials( s, mf );

		addObject( s, mf );

		return s.ToString();

	}

	void addMaterials( StringBuilder s, MeshFilter mf )
	{

		var mesh = mf.sharedMesh;

		if( mesh != null )
		{

			s.Append( "Material " ).Append( mesh.subMeshCount.ToString() );

			s.AppendLine( " {" );

			
			var mr = mf.GetComponent<MeshRenderer>();

			var mats = mr != null ? mr.sharedMaterials : null;

			for( var i = 0; i < mesh.subMeshCount; i++ )
			{

				var subId = i.ToString();


				s.Append( "\t\"mat" ).Append( subId ).Append( "\" " );


				if( mats != null )
				{

					var mat = mats[ i ];


					if( mat.HasProperty( "_Color" ) )
					{
						var c = mat.color;

						s.Append( "col(" );
						s.Append( c.r ).Append( " " );
						s.Append( c.g ).Append( " " );
						s.Append( c.b ).Append( " " );
						s.Append( c.a );
						s.Append( ") " );
					}


					var tex = (Texture2D)mat.mainTexture;

					if( tex != null )
					{
						s.Append( "tex(\"" );
						addTexture( s, tex, mf.name + subId );
						s.Append( "\")" );
					}

				}

				s.AppendLine();

			}

			s.AppendLine( "}" ).AppendLine();

		}

	}

	void addObject( StringBuilder s, MeshFilter mf )
	{

		var mesh = mf.sharedMesh;

		if( mesh != null )
		{

			s.Append( "Object \"" ).Append( mf.name ).Append( "\"" );

			s.AppendLine( " {" );

			addVertices( s, mesh );

			addFaces( s, mesh );

			s.AppendLine( "}" );

		}

	}

	void addVertices( StringBuilder s, Mesh mesh )
	{
		s.Append( "\tvertex " ).Append( mesh.vertexCount.ToString() );
		
		s.AppendLine( " {" );

		var vtxs = mesh.vertices;

		foreach( var vtx in vtxs )
		{

			s.Append( "\t\t" );
			s.Append( ( -vtx.x * scale ).ToString() ).Append( " " );
			s.Append( ( vtx.y * scale ).ToString() ).Append( " " );
			s.Append( ( vtx.z * scale ).ToString() );
			s.AppendLine();

		}

		s.AppendLine( "\t}" );

	}

	void addFaces( StringBuilder s, Mesh mesh )
	{

		s.Append( "\tface " ).Append( (mesh.triangles.Length / 3).ToString() );
		
		s.AppendLine( " {" );



		var cols = mesh.colors32;

		var uvs = mesh.uv;

		 uvs = new Vector2[ mesh.vertexCount ];


		for( var isub = 0; isub < mesh.subMeshCount; isub++ )
		{

			var idxs = mesh.GetTriangles( isub );

			for( var i = 0; i < idxs.Length; i += 3 )
			{

				var i0 = idxs[ i + 0 ];
				var i1 = idxs[ i + 1 ];
				var i2 = idxs[ i + 2 ];


				s.Append( "\t\t3 V(" );
				s.Append( i0.ToString() ).Append( " " );
				s.Append( i1.ToString() ).Append( " " );
				s.Append( i2.ToString() );
				s.Append( ")" );

				s.Append( " M(" ).Append( isub.ToString() ).Append( ")" );

				if( uvs.Length != 0 )
				{
					s.Append( " UV(" );
					s.Append( uvs[i0].x.ToString() ).Append( " " ).Append( (1.0f-uvs[i0].y).ToString() ).Append( " " );
					s.Append( uvs[i1].x.ToString() ).Append( " " ).Append( (1.0f-uvs[i1].y).ToString() ).Append( " " );
					s.Append( uvs[i2].x.ToString() ).Append( " " ).Append( (1.0f-uvs[i2].y).ToString() );
					s.Append( ")" );
				}

				if( cols.Length != 0 )
				{
					s.Append( " COL(" );
					s.Append( ( cols[i0].a << 24 | cols[i0].b << 16 | cols[i0].g << 8 | cols[i0].r ).ToString() ).Append( " " );
					s.Append( ( cols[i1].a << 24 | cols[i1].b << 16 | cols[i1].g << 8 | cols[i1].r ).ToString() ).Append( " " );
					s.Append( ( cols[i2].a << 24 | cols[i2].b << 16 | cols[i2].g << 8 | cols[i2].r ).ToString() );
					s.Append( ")" );
				}

				s.AppendLine();

			}

		}

		s.AppendLine( "\t}" );

	}

	void addTexture( StringBuilder s, Texture2D tex, string subTexName )
	{

		if( tex.name != "" )
		{

			s.Append( Application.dataPath.Replace("/Assets","") ).Append( "/" ).Append( AssetDatabase.GetAssetPath(tex) );

		}
		else
		{

			try
			{

				var filename = saveFolderPath + "/" + subTexName + ".png";

				var entity = tex.EncodeToPNG();

				System.IO.File.WriteAllBytes( filename, entity );

				s.Append( filename );

			}
			catch( System.Exception e )
			{

				Debug.Log( e.Message );//"This Texture is not readable." );

			}

		}

	}

}
