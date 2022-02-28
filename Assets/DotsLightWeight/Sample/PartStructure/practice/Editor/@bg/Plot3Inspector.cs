using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


[ CustomEditor(typeof(Plot3)) ]
public class Plot3Inspector : Editor
{


	PathEditorModule		path;

	UtilityPoleEditorModule	uPole;


	public void OnEnable()
	{

		path = new PathEditorModule( this );

		uPole = new UtilityPoleEditorModule( this );
		
	}

	public void OnDisable()
	{

		uPole.exit();

	}


	public override void OnInspectorGUI()
	{

		
		base.OnInspectorGUI();


		var plot = getPlot();


		path.onInspector( plot );

		uPole.onInspector( plot );
		
	}


	public void OnSceneGUI()
	{


		var plot = getPlot();


		uPole.onScene( plot );

	}




	Plot3 getPlot()
	{
		var go = ((Plot3)target).gameObject;
		
		return go.GetComponent<Plot3>();
	}

	




	
	











	// ���H ==============================================

	struct PathEditorModule
	{

		public PathEditorModule( Editor editor )
		{
 
		}



		public void onInspector( Plot3 plot )
		{

			EditorGUILayout.BeginHorizontal();


			EditorGUILayout.LabelField( "Editor Temporary Path Mesh" );

			if( GUILayout.Button( "build" ) )
			{

				resetEditorMesh( plot );

				createEditorMesh( plot );

				fittingTerrain( plot );

			}

			if( GUILayout.Button( "create" ) )
			{

				resetEditorMesh( plot );

				createEditorMesh( plot );

			}

			if( GUILayout.Button( "adjust" ) )
			{

				fittingTerrain( plot );

			}

			if( GUILayout.Button( "remove" ) )
			{

				resetEditorMesh( plot );

			}


			EditorGUILayout.EndHorizontal();

	

		}

		void createEditorMesh( Plot3 plot )
		{

			var paths = plot.GetComponentsInChildren<PathSegment3>();

			foreach( var path in paths )
			{

				path.buildInEditor();

			}

		}



		void resetEditorMesh( Plot3 plot )
		{

			var eos = plot.GetComponentsInChildren<EditorOnlyObject>( true );

			foreach( var eo in eos )
			{

				DestroyImmediate( eo.gameObject );

			}

		}

		void fittingTerrain( Plot3 plot )
		{
			
			var district = plot.GetComponentInParent<District3>();

			if( district == null ) return;


			var tr = district.GetComponentInChildren<Terrain>();

			var field = new TerrainOperator( tr );


			var parts = plot.GetComponentsInChildren<PathSegment3>();//RoadPart3>();

			foreach( var part in parts )
			{

				var mcs = part.GetComponentsInChildren<MeshCollider>();

				foreach( var mc in mcs )
				{
					field.adjustMesh( mc );
				}

			}

			tr.Flush();

		}


	}


	// �d�� ==============================================

	struct UtilityPoleEditorModule
	{
 
		bool	isPlantMode;

		UtilityPolePlantEditor	plantEditor;

		
		public UtilityPoleEditorModule( ScriptableObject editor )
		{

			isPlantMode = false;

			plantEditor = new UtilityPolePlantEditor( editor );

		}

		public void exit()
		{
			if( isPlantMode )
			{
				plantEditor.restoreTool();
			}
		}

		public void onScene( Plot3 plot )
		{
			if( isPlantMode )
			{
				plantEditor.saveTool();

				plantEditor.onScene( plot.transform );
			}
		}

		public void onInspector( Plot3 plot )
		{

			//isPlantMode = GUILayout.Toggle( isPlantMode, "Plant Mode", GUI.skin.button );
			isPlantMode = true;
			
			if( isPlantMode )
			{
				plantEditor.saveTool();

				plantEditor.onEditor();
			}
			else
			{
				plantEditor.restoreTool();
			}



			EditorGUILayout.BeginHorizontal();


			EditorGUILayout.LabelField( "cable location" );

			if( GUILayout.Button( "adjust" ) )
			{

				adjustCables( plot );

			}

			EditorGUILayout.EndHorizontal();



		}

		void adjustCables( Plot3 plot )
		{

			var cablesets = plot.GetComponentsInChildren<PlotPartCableset3>();

			foreach( var cableset in cablesets )
			{

				cableset.adjustCablePositions();

			}

		}


	}



	public//
	struct UtilityPolePlantEditor
	{

		public UPoleEntrys	data;


		RegistedObjectsHolder	pole;

		RegistedObjectsHolder	cable;


		UtilityPolePlanter	planter;


		Tool	savedTool;


		public UtilityPolePlantEditor( ScriptableObject editor )
		{

			data = null;
			savedTool = Tool.None;

			pole = new RegistedObjectsHolder();
			cable = new RegistedObjectsHolder();
			planter = new UtilityPolePlanter();

			data = getScript( editor );

			pole = new RegistedObjectsHolder( data.poles );
			cable = new RegistedObjectsHolder( data.cablesets );

		}


		public void onScene( Transform tfParent )
		{
			//saveTool();

			if( pole.selectedId >= 0 && cable.selectedId >= 0 )
			{
				planter.plant( tfParent, pole.getSelected(), cable.getSelected() );
			}
		}

		public void onEditor()
		{
			//saveTool();

			//selectByKey();

			selectImageGui( ref pole );

			selectImageGui( ref cable );
		}

		public void saveTool()
		{
			if( Tools.current != Tool.None )
			{
				if( savedTool == Tool.None )
				{
					savedTool = Tools.current;
				}

				Tools.current = Tool.None;
			}
		}

		public void restoreTool()
		{
			if( savedTool != Tool.None )
			{
				Tools.current = savedTool;

				savedTool = Tool.None;
			}
		}

		void selectByKey()
		{
			var e = Event.current;

			if( e.type == EventType.KeyDown )
			{
				switch( e.keyCode )
				{
					case KeyCode.D: pole.selectPrev(); e.Use(); break;
					case KeyCode.F: pole.selectNext(); e.Use(); break;
					case KeyCode.A: cable.selectPrev(); e.Use(); break;
					case KeyCode.S: cable.selectNext(); e.Use(); break;
				}
			}
		}



		void selectImageGui( ref RegistedObjectsHolder target )
		{


			GUILayout.BeginVertical( GUI.skin.box );

			var style = new GUIStyle( GUI.skin.button );
			style.alignment = TextAnchor.UpperCenter;
			style.clipping = TextClipping.Overflow;
			style.imagePosition = ImagePosition.ImageAbove;
			style.fixedHeight = 96.0f;

			target.selectedId = GUILayout.SelectionGrid( target.selectedId, target.images, 4, style );
			
			GUILayout.EndVertical();




			GUILayout.BeginHorizontal();


			GUILayout.BeginVertical( GUI.skin.button );
			
			target.newSelected = (GameObject)EditorGUILayout.ObjectField( "Add", target.newSelected, typeof( GameObject ), false );

			GUILayout.EndVertical();

			if( target.newSelected )
			{
				target.addSelected();

				EditorUtility.SetDirty( data );
			}


			if( GUILayout.Button( "Remove" ) )
			{
				target.removeSeleted();

				EditorUtility.SetDirty( data );
			}


			GUILayout.EndHorizontal();

		}


		UPoleEntrys getScript( ScriptableObject editor )
		// �X�N���v�g�Ɠ����A�Z�b�g�K�w�ɂ���A�f�[�^�ۑ��p�I�u�W�F�N�g��擾����B
		{

			var editorPath = AssetDatabase.GetAssetPath( MonoScript.FromScriptableObject( editor ) );

			var folderPath = Path.GetDirectoryName( editorPath );

			var objectPath = folderPath + "/UPoleSetterInfo.asset";


			var so = (UPoleEntrys)AssetDatabase.LoadAssetAtPath( objectPath, typeof( UPoleEntrys ) );

			if( so == null )
			{
				so = ScriptableObject.CreateInstance<UPoleEntrys>();

				AssetDatabase.CreateAsset( so, objectPath );


				ScriptableObject sobj = (ScriptableObject)AssetDatabase.LoadAssetAtPath( objectPath, typeof( ScriptableObject ) );

				string[] labels = { "Data", "ScriptableObject" };

				AssetDatabase.SetLabels( sobj, labels );

				EditorUtility.SetDirty( sobj );
			}


			return so;

		}

	}



	// --------------------------------------------

	struct RegistedObjectsHolder
	{

		public GUIContent[] images { get; private set; }

		public GameObject	newSelected;

		public int			selectedId;



		public List<GUIContent> textures { get; private set; }

		public List<GameObject> objects { get; private set; }


		public RegistedObjectsHolder( List<GameObject> dataList )
		{

			selectedId = 0;
			newSelected = null;
			textures = null;
			objects = null;
			images = null;

			textures = new List<GUIContent>();

			load( dataList );

			selectedId = objects.Count > 0 ? 0 : -1;

		}

		public GameObject getSelected()
		{

			return selectedId >= 0 ? objects[ selectedId ] : null;

		}

		public void selectNext()
		{
			if( ++selectedId >= objects.Count ) selectedId = 0;
		}
		public void selectPrev()
		{
			if( --selectedId < 0 ) selectedId = objects.Count - 1;
		}


		void load( List<GameObject> dataList )
		{

			objects = dataList;

			for( var i = 0 ; i < objects.Count ; i++ )
			{

				textures.Add( createTexture( objects[i] ) );

			}

			images = textures.ToArray();

		}

		void add( GameObject prefab )
		{

			var tex = createTexture( prefab );

			if( tex.image )
			{
				objects.Add( prefab );

				textures.Add( tex );
			}

		}

		public void add( Object[] objs )
		{

			foreach( var obj in objs )
			{
				if( obj is GameObject )
				{
					add( (GameObject)obj );
				}
			}

			images = textures.ToArray();

		}


		public void addSelected()
		{
			add( newSelected );

			newSelected = null;

			selectedId = objects.Count - 1;

			images = textures.ToArray();
		}

		public void removeSeleted()
		{
			objects.RemoveAt( selectedId );

			textures.RemoveAt( selectedId );

			if( selectedId >= objects.Count )
			{
				selectedId = objects.Count - 1;
			}

			images = textures.ToArray();
		}


		GUIContent createTexture( GameObject prefab )
		{

			var editor = Editor.CreateEditor( prefab );

			var tex = editor.RenderStaticPreview( null, null, 128, 128 );

			var content = new GUIContent( prefab.name, tex );

			return content;

		}


	}


	// -------------------------------

	struct UtilityPolePlanter
	{

		bool	isPlantDraging;	// �̈悩��̃h���b�O�Ƃ��h�~�i�O�̂��߁j


		Transform	tfHubForcus;


		bool	isPlantMode;

		bool	isNewPole;

		int		undoGroup;


		public void plant( Transform tfParent, GameObject polePrefab, GameObject cablePrefab )
		{

			var e = Event.current;// Debug.Log(e.keyCode);


			if( e.type == EventType.Layout )
			{
				HandleUtility.AddDefaultControl( GUIUtility.GetControlID( FocusType.Passive ) );
			}



			if( e.isMouse )
			{

				if( e.button == 0 && e.type == EventType.MouseDown )
				{

					var tfHub = getHubTransformByMousePick();

					if( tfHub )
					{
						// �P�[�u���~�݃��[�h�i�n�u��N���b�N�����j

						isNewPole = false;

					}
					else
					{
						// �d���ݒu���[�h�i�n�u�̂Ȃ��Ƃ����N���b�N�����j

						tfHub = instantiatePole( polePrefab, tfParent );

						if( tfHub ) Undo.RegisterCreatedObjectUndo( getPoleObject( tfHub ), "Create UPole" );

						isNewPole = true;

					}


					if( tfHub )
					{
						isPlantDraging = true;
					}

					tfHubForcus = tfHub;

					e.Use();

				}
				else if( e.button == 0 && e.type == EventType.MouseDrag )
				{

					if( isPlantDraging ) e.Use();

				}
				else if( e.button == 0 && e.type == EventType.MouseUp )
				{

					if( isPlantDraging )
					{

						var tfHub = getHubTransformByMousePick();

						if( tfHub != tfHubForcus )//&& e.delta.sqrMagnitude > 1.0f * 1.0f )
						{

							if( tfHub )
							{
								// �����d���֕~��


							}
							else
							{
								// �d���ݒu

								tfHub = instantiatePole( polePrefab, tfParent );

								if( tfHub ) Undo.RegisterCreatedObjectUndo( getPoleObject( tfHub ), "Create UPole" );

							}

							if( tfHub )
							{
								// �P�[�u���ݒu

								var deleteCableset = deleteSameCableSet( tfParent, tfHubForcus, tfHub );

								if( deleteCableset ) Undo.DestroyObjectImmediate( deleteCableset.gameObject );

								var newCableset = createCablesetAndAdjust( tfParent, tfHubForcus, tfHub, cablePrefab );

								if( newCableset ) Undo.RegisterCreatedObjectUndo( newCableset.gameObject, "Create Cables" );

							}

						}
						else
						{

							// �d���I��

							if( tfHub && isNewPole == false )
							{
								Selection.activeObject = getPoleObject( tfHub );
							}

						}


						isPlantDraging = false;

						e.Use();

					}

				}
				else
				{

					if( e.button == 0 ) e.Use();

					isPlantDraging = false;

				}

			}

		}


		Transform getHubTransformByMousePick()
		{

			var gos = HandleUtility.PickRectObjects( createHubHitRect(), false );

			if( gos == null ) return null;



			foreach( var go in gos )
			{
				var hub = go.GetComponentInParent<CableHubPoint3>();

				if( hub != null ) return hub.transform;
			}


			return null;

		}

		Rect createHubHitRect()
		{
			var o = 20.0f;
			var mp = Event.current.mousePosition;
			var rc = new Rect( mp.x - o * 0.5f, mp.y - o * 0.5f, o, o );

			return rc;
		}

		GameObject getPoleObject( Transform tfHub )
		{
			var pole = tfHub.GetComponentInParent<PlotPartUtlPole3>();

			return pole ? pole.gameObject : null;
		}



		Transform instantiatePole( GameObject prefab ,Transform tfParent )
		// �v���n�u����d����}�E�X�s�b�N�ʒu�ɍ쐬����i�d���̍�����l�����Ă��炷�j�B
		{

			var offset = getHubSamplePoleOffset( prefab );
			
			var rot = tfHubForcus ? tfHubForcus.rotation : Quaternion.identity;

			var	ray = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

			ray.origin -= rot * offset;


			var	hit = new RaycastHit();

			if( Physics.Raycast( ray, out hit, float.PositiveInfinity ) )
			{

				var newPole = (GameObject)GameObject.Instantiate( prefab, hit.point, rot );

				newPole.transform.SetParent( tfParent, true );

				var newHub = newPole.GetComponentInChildren<CableHubPoint3>();
				
				return newHub ? newHub.transform : null;

			}


			return null;

		}

		Vector3 getHubSamplePoleOffset( GameObject prefab )
		// �v���n�u���猩�������ŏ��̃n�u�̈ʒu��I�t�Z�b�g�ʒu�Ƃ��ĕԂ��B
		{
			var hub = prefab.GetComponentInChildren<CableHubPoint3>();

			if( hub )
			{
				return hub.transform.position;
			}
			else
			{
				// �v���n�u���Ɖ��̊K�w�܂ł݂�Ȃ���������B

				var samplePole = (GameObject)GameObject.Instantiate( prefab );

				var sampleHub = samplePole.GetComponentInChildren<CableHubPoint3>();

				var res = sampleHub ? sampleHub.transform.position : Vector3.zero;
				
				DestroyImmediate( samplePole );

				return res;
			}
		}



		PlotPartCableset3 deleteSameCableSet( Transform tfParent, Transform tfStart, Transform tfEnd )
		// ���[�̓����P�[�u���Z�b�g��T���A�폜����B
		{

			if( tfStart == tfEnd ) return null;


			var cablesets = tfParent.GetComponentsInChildren<PlotPartCableset3>();

			foreach( var cs in cablesets )
			{
				if( cs.tfStartSideHub == tfStart && cs.tfEndSideHub == tfEnd || cs.tfEndSideHub == tfStart && cs.tfStartSideHub == tfEnd )
				{

					//GameObject.DestroyImmediate( cs.gameObject );

					return cs;

				}
			}

			return null;

		}

		PlotPartCableset3 createCablesetAndAdjust( Transform tfParent, Transform tfStart, Transform tfEnd, GameObject prefab )
		{

			if( tfStart == tfEnd ) return null;


			var cableset = instantiateCableset( prefab, tfStart, tfEnd );

			cableset.transform.SetParent( tfParent );

			cableset.adjustCablePositions( tfStart, tfEnd );

			return cableset;

		}

		PlotPartCableset3 instantiateCableset( GameObject prefab, Transform tfStart, Transform tfEnd )
		{

			var cableset = new GameObject( "cableset " + prefab.name );

			var cs = cableset.AddComponent<PlotPartCableset3>();

			var tfCableset = cableset.transform;


			var anchorLength = Mathf.Min( countAnchor( tfStart ), countAnchor( tfEnd ) );

			for( var i = 0 ; i < anchorLength ; i++ )
			{
				var cable = GameObject.Instantiate<GameObject>( prefab );

				if( cable.GetComponent<CablePiece3>() == null ) cable.AddComponent<CablePiece3>();

				cable.transform.SetParent( tfCableset );
			}


			return cs;

		}

		int countAnchor( Transform tfHub )
		{
			return tfHub.childCount;//.GetComponentsInChildren<CableHubPoint3>().Length;	
		}

	}






}
