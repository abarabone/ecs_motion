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

using Abarabone.Utilities;
using Abarabone.Common.Extension;


namespace Abarabone.CharacterMotion
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










}