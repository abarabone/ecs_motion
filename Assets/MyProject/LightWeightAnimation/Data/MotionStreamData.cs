using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abarabone.Geometry;
using System.Runtime.InteropServices;

namespace Abarabone.Motion
{



    //public struct StreamInitializeTag : IComponentData
    //{
    //       public Entity MotionEntity;
    //       //public int BoneIndex;
    //       public BlobArray<KeyBlobUnit> Keys;
    //   }

    //public struct StreamInitialFor1posTag : IComponentData
    //{}

    public struct StreamDrawTargetData : IComponentData
    {
        public bool IsDrawTarget;
    }

    public struct StreamRelationData : IComponentData
    {
        public int BoneId;
        public Entity NextStreamEntity;
    }

    public struct StreamDrawLinkData : IComponentData
    {
        public Entity DrawEntity;
    }
    public struct StreamMotionLinkData : IComponentData// MotionB 用
    {
        public Entity MotionEntity;
    }

    /// <summary>
    /// 最適化用
    /// </summary>
    //public struct StreamDirectDrawTargetIndexData : IComponentData
    //{
    //    public int DrawInstanceVectorIndex;
    //}


    /// <summary>
    /// 現在キーの位置と、ストリームデータへの参照を保持する。
    /// </summary>
    unsafe public struct StreamKeyShiftData : IComponentData
    {
        public KeyBlobUnit* Keys;
        public int KeyLength;

        public int KeyIndex_Next;
    }

    /// <summary>
    /// 時間は deltaTime を加算して進める。
    /// （スタート時刻と現在時刻を比較する方法だと、速度変化や休止ができないため）
    /// ※ただし現在時刻法だと、不要な更新をなくせるので、こちらのほうがいい面も多い
    /// </summary>
    public struct StreamCursorData : IComponentData
    {
        public MotionCursorData Cursor;
    }

    /// <summary>
    /// 現在キー周辺のキーキャッシュデータ。
    /// キーがシフトしたときのみ、次のキーを読めば済むようにする。
    /// </summary>
    public struct StreamNearKeysCacheData : IComponentData
    {
        public float Time_From;
        public float Time_To;
        public float Time_Next;

        // 補間にかけるための現在キー周辺４つのキー
        public float4 Value_Prev;
        public float4 Value_From;   // これが現在キー
        public float4 Value_To;
        public float4 Value_Next;
    }

    /// <summary>
    /// 現在キー周辺のキーと現在時間から保管した計算結果。
    /// </summary>
    public struct StreamInterpolatedData : IComponentData
    {
        public float4 Value;
    }



    // ストリーム拡張 -----------------------------------------------------------------------------------------------

    static public class StreamUtility
    {

        ///// <summary>
        ///// キーバッファをストリーム先頭に初期化する。
        ///// </summary>
        //unsafe static public void InitializeKeys(
        //    ref this StreamNearKeysCacheData nearKeys,
        //    ref StreamKeyShiftData shift,
        //    ref StreamTimeProgressData progress,
        //    float timeOffset = 0.0f
        //)
        //{
        //    var index0 = 0;
        //    var index1 = math.min( 1, shift.KeyLength - 1 );
        //    var index2 = math.min( 2, shift.KeyLength - 1 );

        //    nearKeys.Time_From = shift.Keys[ index0 ].Time.x;
        //    nearKeys.Time_To = shift.Keys[ index1 ].Time.x;
        //    nearKeys.Time_Next = shift.Keys[ index2 ].Time.x;

        //    nearKeys.Value_Prev = shift.Keys[ index0 ].Value;
        //    nearKeys.Value_From = shift.Keys[ index0 ].Value;
        //    nearKeys.Value_To = shift.Keys[ index1 ].Value;
        //    nearKeys.Value_Next = shift.Keys[ index2 ].Value;

        //    shift.KeyIndex_Next = index2;

        //    progress.TimeProgress = timeOffset;
        //}

        //unsafe static public void InitializeKeysContinuous(
        //    ref this StreamNearKeysCacheData nearKeys,
        //    ref StreamKeyShiftData shift,
        //    ref StreamTimeProgressData progress,
        //    float delayTimer = 0.0f// 再検討の余地あり（変な挙動あり）
        //)
        //{
        //    var index0 = 0;
        //    var index1 = math.min( 1, shift.KeyLength - 1 );

        //    nearKeys.Time_From = -delayTimer;
        //    nearKeys.Time_To = shift.Keys[ index0 ].Time.x;
        //    nearKeys.Time_Next = shift.Keys[ index1 ].Time.x;

        //    nearKeys.Value_To = shift.Keys[ index0 ].Value;
        //    nearKeys.Value_Next = shift.Keys[ index1 ].Value;

        //    shift.KeyIndex_Next = index1;

        //    progress.TimeProgress = -delayTimer;
        //}

        /// <summary>
        /// キーバッファをストリーム先頭に初期化する。
        /// </summary>
        static public unsafe void InitializeKeys(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift
        )
        {
            var index0 = 0;
            var index1 = math.min( 1, shift.KeyLength - 1 );
            var index2 = math.min( 2, shift.KeyLength - 1 );

            nearKeys.Time_From = shift.Keys[ index0 ].Time.x;
            nearKeys.Time_To = shift.Keys[ index1 ].Time.x;
            nearKeys.Time_Next = shift.Keys[ index2 ].Time.x;

            nearKeys.Value_Prev = shift.Keys[ index0 ].Value;
            nearKeys.Value_From = shift.Keys[ index0 ].Value;
            nearKeys.Value_To = shift.Keys[ index1 ].Value;
            nearKeys.Value_Next = shift.Keys[ index2 ].Value;

            shift.KeyIndex_Next = index2;
        }

        static public unsafe void InitializeKeysContinuous(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift,
            float delayTimer = 0.0f// 再検討の余地あり（変な挙動あり）
        )
        {
            var index0 = 0;
            var index1 = math.min( 1, shift.KeyLength - 1 );

            nearKeys.Time_From = -delayTimer;
            nearKeys.Time_To = shift.Keys[ index0 ].Time.x;
            nearKeys.Time_Next = shift.Keys[ index1 ].Time.x;

            nearKeys.Value_To = shift.Keys[ index0 ].Value;
            nearKeys.Value_Next = shift.Keys[ index1 ].Value;

            shift.KeyIndex_Next = index1;
        }


        /// <summary>
        /// キーバッファを次のキーに移行する。終端まで来たら、最後のキーのままでいる。
        /// </summary>
        unsafe static public void ShiftKeysIfOverKeyTime(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift,
            in MotionCursorData cursor
        )
        {
            if( cursor.CurrentPosition < nearKeys.Time_To ) return;


            var nextIndex = math.min( shift.KeyIndex_Next + 1, shift.KeyLength - 1 );
            var nextKey = shift.Keys[ nextIndex ];

            nearKeys.Time_From = nearKeys.Time_To;
            nearKeys.Time_To = nearKeys.Time_Next;
            nearKeys.Time_Next = nextKey.Time.x;

            nearKeys.Value_Prev = nearKeys.Value_From;
            nearKeys.Value_From = nearKeys.Value_To;
            nearKeys.Value_To = nearKeys.Value_Next;
            nearKeys.Value_Next = nextKey.Value;

            shift.KeyIndex_Next = nextIndex;
        }

        /// <summary>
        /// キーバッファを次のキーに移行する。ループアニメーション対応版。
        /// </summary>
        unsafe static public void ShiftKeysIfOverKeyTimeForLooping(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift,
            ref MotionCursorData cursor
        )
        {
            //if( cursor.CurrentPosition < nearKeys.Time_To ) return;
            while( cursor.CurrentPosition >= nearKeys.Time_To )
            {
                var isEndOfStream = cursor.CurrentPosition >= cursor.TotalLength;

                var timeOffset = getTimeOffsetOverLength( in cursor, isEndOfStream );

                var nextIndex = getNextKeyIndex( in shift, isEndOfStream );
                var nextKey = shift.Keys[ nextIndex ];

                var time_from = nearKeys.Time_To;
                var time_to = nearKeys.Time_Next;
                var time_next = nextKey.Time.x;

                nearKeys.Time_From = time_from - timeOffset;
                nearKeys.Time_To = time_to - timeOffset;
                nearKeys.Time_Next = time_next;

                nearKeys.Value_Prev = nearKeys.Value_From;
                nearKeys.Value_From = nearKeys.Value_To;
                nearKeys.Value_To = nearKeys.Value_Next;
                nearKeys.Value_Next = nextKey.Value;

                shift.KeyIndex_Next = nextIndex;

                cursor.CurrentPosition -= timeOffset;
            }
            
            return;


            float getTimeOffsetOverLength( in MotionCursorData cursor_, bool isEndOfStream_ )
            {
                return math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
            }

            int getNextKeyIndex( in StreamKeyShiftData shift_, bool isEndOfStream_ )
            {
                var iKeyLast = shift_.KeyLength - 1;
                var iKeyNextNext = shift_.KeyIndex_Next + 1;

                var isEndOfKey = iKeyNextNext > iKeyLast;

                var iWhenStayInnerKey = math.min( iKeyNextNext, iKeyLast );
                var iWhenOverLastKey = iKeyNextNext - math.select( 0, iKeyLast, isEndOfKey );

                return math.select( iWhenStayInnerKey, iWhenOverLastKey, isEndOfStream_ );
                // こうなってくると、素直に分岐したほうがいいんだろうかｗ←いや、はやかった
            }
        }

        /// <summary>
        /// 時間を進める。
        /// </summary>
        static public void Progress( ref this MotionCursorData cursor, float deltaTime )
        {
            cursor.CurrentPosition += deltaTime * cursor.Scale;
        }

        static public float CaluclateTimeNormalized
            ( ref this StreamNearKeysCacheData nearKeys, float timeProgress )
        {
            var progress = timeProgress - nearKeys.Time_From;
            var length = nearKeys.Time_To - nearKeys.Time_From;

            var progress_div_length = math.saturate( progress * math.rcp( length ) );
            //var progress_div_length = progress * math.rcp( length );

            return math.select( progress_div_length, 1.0f, length == 0.0f );// select( 偽, 真, 条件 );
        }

        /// <summary>
        /// 補完する。
        /// </summary>
        static public float4 Interpolate
            ( ref this StreamNearKeysCacheData nearKeys, float normalizedTimeProgress )
        {

            //var s = math.sign( math.dot( nearKeys.Value_From, nearKeys.Value_To ) );

            var v = VectorUtility.Interpolate(
                nearKeys.Value_Prev,
                nearKeys.Value_From,
                nearKeys.Value_To,// * s,
                nearKeys.Value_Next,// * s,
                normalizedTimeProgress
            );

            return v;// math.normalize( v );
        }
    }

    // --------------------------------------------------------------------------------------------------------------

}