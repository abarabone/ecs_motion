using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;
using System.IO;

using Abss.Utilities;


namespace Abss.Motion
{

	//[CustomEditor( typeof( UnityEngine.Object ) )]
	//public class MotionClipConverter : Editor
	//{


	//}



	// モーションクリップ（エディタ保管用） --------------------------

	//[CreateAssetMenu( menuName = "Scriptable Object/Motion Clip", fileName = "MotonClip" )]
	public class MotionClip : ScriptableObject
	{ 
	
		public string[]	ClipNames;

		public string[]	StreamPaths;
		public int[]	IndexMapFbxToMotion;

		public MotionDataInAsset	MotionData;
		
		//public BoolReactiveProperty IsWorld = new BoolReactiveProperty( false );
	}


	[Serializable]
	public struct MotionDataInAsset
	{
		public MotionDataUnit[]	Motions;
	}

	[Serializable]
	public struct MotionDataUnit
	{
		public string	MotionName;

		public float	TimeLength;
		public Bounds	LocalAABB;
		public WrapMode	WrapMode;

		public SectionDataUnit[]	Sections;
	}

	[Serializable]
	public struct SectionDataUnit
	{
		public string	SectionName;

		public StreamDataUnit[]	Streams;
	}

	[Serializable]
	public struct StreamDataUnit
	{
		public string	StreamPath;

		public KeyDataUnit[]	keys;
	}

	[Serializable]
	public struct KeyDataUnit
	{
		public float	Time;
	
		[Compact]
		public Vector4	Value;
	}

	// ----------------------------------------------------------











	// アニメーションクリップからモーションクリップへのコンバート -----------------------------

	#if UNITY_EDITOR

	public static partial class MotionClipUtility
	{

		[MenuItem( "Help/Check compute shader support" )]
		static public void CheckComputeShaderSupport()
		{
			Debug.Log( $"supported = {SystemInfo.supportsComputeShaders}" );
			Debug.Log( $"instancing = {SystemInfo.supportsInstancing}" );
		}

		[MenuItem( "Assets/Convert to MotionClips" )]
		static public void Create()
		{
			if( Selection.objects == null ) return;

			var animationClips	= extractAnimationClips( Selection.objects );
			var streamPaths		= extractStreamPaths( Selection.objects );
			var indexMapping	= makeBoneIndexMapping( Selection.objects, streamPaths );
			var motionData		= buildMotionData( animationClips );
		
			var motionClip = ScriptableObject.CreateInstance<MotionClip>();
			motionClip.StreamPaths			= streamPaths;
			motionClip.IndexMapFbxToMotion	= indexMapping;
			motionClip.ClipNames			= animationClips.Select( x => x.name ).ToArray();
			motionClip.MotionData			= motionData;
		
			save( Selection.objects, motionClip );
		/*	using( var a = new MotionDataInNative() )
			using( var f = new StreamWriter( "C:/Users/abara/Desktop/output.txt" ) )
			{
				a.ConvertFrom( motionClip );
				var q =
					from stream in a.pool.StreamSlices
					//from key in stream.Stream
					//select key
					select stream
					;
				foreach( var i in q )
				{
					//f.WriteLine( $"{i.Time} {i.Value}" );
					f.WriteLine( $"{i.Stream.Length} {i.Stream.Stride}" );
				}
			}*/
		}

		static AnimationClip[] extractAnimationClips( UnityEngine.Object[] selectedObjects )
		{
			// 選択されたアセットから AnimationClip を取得

			var clips =
				from go in selectedObjects.OfType<GameObject>()
				select AssetDatabase.GetAssetPath(go) into path
			//	where Path.GetExtension(path) == ".fbx"
			
				from clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
				where !clip.name.StartsWith("__preview__")

				select clip
				;
		
			return clips.ToArray();
		}
	
		static string[] extractStreamPaths( UnityEngine.Object[] selectedObjects )
		{
			var q = selectedObjects
				.OfType<GameObject>()
				.Children()
				.Where( go => go.name != "Mesh" )
				.Where( go => !go.name.StartsWith("_") )// _ から始まるものはマニピュレータなので除外（マイルール）
				.DescendantsAndSelf()
				.MakePath()
				;

			return q.ToArray();
		}
		static int[] makeBoneIndexMapping( UnityEngine.Object[] selectedObjects, string[] streamPaths )
		{
			var pathDicts = streamPaths
				.Select( (path,i) => (path,i) )
				.ToDictionary( x => x.path, x => x.i )
				;

			var q = selectedObjects
				.OfType<GameObject>()
				.Children()
				.Where( go => go.name == "Mesh" )
				.Children()
				.Select( go => go.GetComponent<SkinnedMeshRenderer>() )
				.Where( smr => smr != null )
				.First().bones
					.Select( bone => bone.gameObject )
					.MakePath()
					.Select( path => pathDicts.GetOrDefault(path, defaultValue:-1) )	// ストリームにない場合は -1
				;

			return q.ToArray();
		}
		

		static MotionDataInAsset buildMotionData( AnimationClip[] animationClips )
		{
		
			return  new MotionDataInAsset
			{
				Motions = queryMotions( animationClips ).ToArray()
			};


			IEnumerable<MotionDataUnit> queryMotions( AnimationClip[] clips ) =>
		
				// アニメーションクリップを列挙
				from clip in clips

				select new MotionDataUnit
				{
					MotionName	= clip.name,
					LocalAABB	= clip.localBounds,
					WrapMode	= clip.wrapMode,
					TimeLength	= clip.length,

					Sections	= buildSections( clip ).ToArray()
				};
			

			IEnumerable<SectionDataUnit> buildSections( AnimationClip clip ) =>
				
				// 全プロパティストリームを列挙（ .x .y などの要素レベルの binding ）
				from binding in AnimationUtility.GetCurveBindings(clip)
				where !binding.path.StartsWith("_")
				
				// セクションでグループ化（ m_LocalPosition など）＆ソート
				group binding by Path.GetFileNameWithoutExtension(binding.propertyName) into section_group
				orderby section_group.Key

				select new SectionDataUnit
				{
					SectionName	= section_group.Key,

					Streams		= queryKeyStreams( clip, section_group ).ToArray()
				};
			

			IEnumerable<StreamDataUnit> queryKeyStreams( AnimationClip clip, IGrouping<string,EditorCurveBinding> section_group ) =>
		
				// ストリームでグループ化＆ソート
				from binding in section_group
				group binding by binding.path into stream_group
				orderby stream_group.Key

				select new StreamDataUnit
				{
					StreamPath	= stream_group.Key,

					keys		= queryKeys( clip, stream_group ).ToArray()		
				};
		

			IEnumerable<KeyDataUnit> queryKeys( AnimationClip clip, IGrouping<string,EditorCurveBinding> stream_group ) =>
		
				// キーを列挙
				from stream in stream_group
				from keyframe in AnimationUtility.GetEditorCurve(clip,stream).keys
				select ( element:Path.GetExtension(stream.propertyName)[1], keyFrame:keyframe ) into keyBind

				// 時間でグループ化＆ソート
				group keyBind by keyBind.keyFrame.time into time_group
				orderby time_group.Key

				// 要素で辞書化し、添え字アクセスできるようにする。
				let elements = time_group.ToDictionary( x => x.element, x => x.keyFrame.value )
			
				select new KeyDataUnit
				{
					Time	= time_group.Key,

					Value	= elements.ContainsKey('w')
						? new Vector4( elements['x'], elements['y'], elements['z'], elements['w'] )
						: new Vector4( elements['x'], elements['y'], elements['z'], 0.0f )
				};	
		
		}
	


		/// <summary>
		/// モーションクリップをアセットとして保存する。
		/// </summary>
		static void save( UnityEngine.Object[] selectedObjects, MotionClip motionClip )
		{
		
			// 渡されたアセットと同じ場所のパス生成
		
			var srcFilePath	= AssetDatabase.GetAssetPath( selectedObjects.OfType<GameObject>().First() );

			var folderPath	= Path.GetDirectoryName( srcFilePath );

			var fileName	= Path.GetFileNameWithoutExtension( srcFilePath );

			var dstFilePath	= folderPath + $"/Motion {fileName}.asset";
		
		
			// アセットとして MotionClip を生成
		
			AssetDatabase.CreateAsset( motionClip, dstFilePath );

			AssetDatabase.Refresh();

		}

	}

	
	

	#endif

		// ----------------------------------------------------------

}