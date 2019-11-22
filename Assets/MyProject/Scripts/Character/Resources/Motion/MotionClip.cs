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

    
    public enum KeyStreamSection
    {
        positions,
        rotations,
        scales,
        length
    }


    // モーションクリップ（エディタ保管用） --------------------------

    //[CreateAssetMenu( menuName = "Scriptable Object/Motion Clip", fileName = "MotonClip" )]
    public class MotionClip : ScriptableObject
    {

        public string[] ClipNames;

        public string[] StreamPaths;
        //public int[] IndexMapFbxToMotion;// mesh.bones のインデックス → モーションストリームのインデックス

        public MotionDataInAsset MotionData;

        //public BoolReactiveProperty IsWorld = new BoolReactiveProperty( false );
    }


    [Serializable]
    public struct MotionDataInAsset
    {
        public MotionDataUnit[] Motions;
    }

    [Serializable]
    public struct MotionDataUnit
    {
        public string MotionName;

        public float TimeLength;
        public Bounds LocalAABB;
        public WrapMode WrapMode;

        public SectionDataUnit[] Sections;
    }

    [Serializable]
    public struct SectionDataUnit
    {
        public string SectionName;

        public StreamDataUnit[] Streams;
    }

    [Serializable]
    public struct StreamDataUnit
    {
        public string StreamPath;

        public KeyDataUnit[] keys;
    }

    [Serializable]
    public struct KeyDataUnit
    {
        public float Time;

        [Compact]
        public Vector4 Value;
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

            var smr = getFirstSkinnedMeshRenderer( Selection.objects );
            var animationClips = getAllAnimationClips( Selection.objects );

            var streamPaths = makeStreamPaths( smr.bones );
            var motionData = buildMotionData( animationClips, streamPaths );

            var motionClip = ScriptableObject.CreateInstance<MotionClip>();
            motionClip.StreamPaths = streamPaths;
            //motionClip.IndexMapFbxToMotion = indexMapping;
            motionClip.ClipNames = animationClips.Select( x => x.name ).ToArray();
            motionClip.MotionData = motionData;

            save( Selection.objects, motionClip );
            makeMotionEnum( motionClip );
        }

        static SkinnedMeshRenderer getFirstSkinnedMeshRenderer( UnityEngine.Object[] selectedObjects )
        {
            return selectedObjects
                .OfType<GameObject>()
                .Children()
                .Where( go => go.name == "Mesh" )
                .Children()
                .Select( go => go.GetComponent<SkinnedMeshRenderer>() )
                .Where( smr => smr != null )
                .First();
        }
        static AnimationClip[] getAllAnimationClips( UnityEngine.Object[] selectedObjects )
        {
            return selectedObjects
                .OfType<GameObject>()
                .Select( go => AssetDatabase.GetAssetPath( go ) )
                .Where( path => Path.GetExtension( path ) == ".fbx" )
                .SelectMany( path => AssetDatabase.LoadAllAssetsAtPath( path ) )
                .OfType<AnimationClip>()
                .Where( clip => !clip.name.StartsWith( "__preview__" ) )
                .ToArray();
        }
        // _ から始まるボーン名は無視される。
        static string[] makeStreamPaths( Transform[] tfBones )
        {
            return tfBones
                .Select( tfBone => tfBone.gameObject )
                .Where( go => !go.name.StartsWith( "_" ) )
                .MakePath()
                .ToArray();
        }

        static MotionDataInAsset buildMotionData( AnimationClip[] animationClips, string[] streamPaths )
        {

            return new MotionDataInAsset
            {
                Motions = queryMotions( animationClips ).ToArray()
            };


            IEnumerable<MotionDataUnit> queryMotions( AnimationClip[] clips ) =>

                // アニメーションクリップを列挙
                from clip in clips

                select new MotionDataUnit
                {
                    MotionName = clip.name,
                    LocalAABB = clip.localBounds,
                    WrapMode = clip.wrapMode,
                    TimeLength = clip.length,

                    Sections = buildSections( clip ).ToArray()
                };


            IEnumerable<SectionDataUnit> buildSections( AnimationClip clip ) =>

                // 全プロパティストリームを列挙（ .x .y などの要素レベルの binding ）
                from binding in AnimationUtility.GetCurveBindings( clip )
                where !binding.path.StartsWith( "_" )// _ から始まるものはマニピュレータなので除外（マイルール）

                // セクションでグループ化（ m_LocalPosition など）＆ソート
                group binding by Path.GetFileNameWithoutExtension( binding.propertyName ) into section_group
                orderby section_group.Key

                select new SectionDataUnit
                {
                    SectionName = section_group.Key,
                    Streams = queryKeyStreams( clip, section_group ).ToArray()
                };


            // セクション内のストリームをクエリする。並び順は、メッシュのボーンと同じ順序となる。
            IEnumerable<StreamDataUnit> queryKeyStreams
                ( AnimationClip clip, IGrouping<string, EditorCurveBinding> section_group ) =>
                
                from stream_path in streamPaths
                join stream_group in
                    from binding in section_group
                    group binding by binding.path
                        on stream_path equals stream_group.Key
                into x
                from stream_group in x.DefaultIfEmpty() // 空の場合どうする？？とりあえず空の配列にするけど
                select new StreamDataUnit
                {
                    StreamPath = stream_path,
                    keys = queryKeys( clip, stream_group ).ToArray()
                };


            IEnumerable<KeyDataUnit> queryKeys
                ( AnimationClip clip, IGrouping<string, EditorCurveBinding> stream_group ) =>

                // キーを列挙
                from stream in stream_group
                from keyframe in AnimationUtility.GetEditorCurve( clip, stream ).keys
                select (element: Path.GetExtension( stream.propertyName )[ 1 ], keyFrame: keyframe) into keyBind

                // 時間でグループ化＆ソート
                group keyBind by keyBind.keyFrame.time into time_group
                orderby time_group.Key

                // 要素で辞書化し、添え字アクセスできるようにする。
                let elements = time_group.ToDictionary( x => x.element, x => x.keyFrame.value )

                select new KeyDataUnit
                {
                    Time = time_group.Key,

                    Value = elements.ContainsKey( 'w' )
                        ? new Vector4( elements[ 'x' ], elements[ 'y' ], elements[ 'z' ], elements[ 'w' ] )
                        : new Vector4( elements[ 'x' ], elements[ 'y' ], elements[ 'z' ], 0.0f )
                };

        }



        /// <summary>
        /// モーションクリップをアセットとして保存する。
        /// </summary>
        static void save( UnityEngine.Object[] selectedObjects, MotionClip motionClip )
        {

            // 渡されたアセットと同じ場所のパス生成

            var srcFilePath = AssetDatabase.GetAssetPath( selectedObjects.OfType<GameObject>().First() );

            var folderPath = Path.GetDirectoryName( srcFilePath );

            var fileName = Path.GetFileNameWithoutExtension( srcFilePath );

            var dstFilePath = folderPath + $"/Motion {fileName}.asset";


            // アセットとして MotionClip を生成

            AssetDatabase.CreateAsset( motionClip, dstFilePath );

            AssetDatabase.Refresh();

        }

        static void makeMotionEnum( MotionClip motionClip )
        {
            var qNameList =
                from x in motionClip.ClipNames
                select x.Replace( ' ', '_' ).Replace( '(', '_' ).Replace( ')', '_' ).Replace( '[', '_' ).Replace( ']', '_' )
                ;

            var name = motionClip.name
                .Replace( ' ', '_' ).Replace( '(', '_' ).Replace( ')', '_' ).Replace( '[', '_' ).Replace( ']', '_' );

            EnumCreator.Create( $"{name}", qNameList, $"Assets/Enum_{name}.cs" );
        }
    }




#endif

    // ----------------------------------------------------------

}