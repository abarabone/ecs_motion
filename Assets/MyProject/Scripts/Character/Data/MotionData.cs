using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abss.Motion
{
	

	public struct MotionInitializeData : IComponentData
	{
        public int MotionIndex;
        public float DelayTime;
        public float TimeScale;
        public bool IsLooping;
        public bool IsContinuous;
    }

    public struct MotionClipData : IComponentData
    {
        public BlobAssetReference<MotionBlobData> ClipData;
    }

    public struct MotionInfoData : IComponentData
	{
        public int MotionIndex;
    }

    public struct MotionStreamLinkData : IComponentData
    {
        public Entity PositionStreamTop;
        public Entity RotationStreamTop;
    }

    //public struct MotionATag : IComponentData// MotionB と区別するため暫定
    //{ }
    public struct MotionCursorData : IComponentData// MotionB 用
    {
        public float CurrentPosition;//CurrentPosition;
        public float TotalLength;
        public float Scale;
        //public StreamTimeProgressData Timer;
    }
    public struct MotionProgressTimerTag : IComponentData// MotionB 用
    { }




    public struct MotionBlend2WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
    }
    public struct MotionBlend3WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
        public float WeightNormalized2;
    }

    public struct BlendBoneFadeData : IComponentData
    {
        public float FadeTime;
    }

    

    static public class MotionExtension
    {
        /// <summary>
        /// モーション初期化セット、disable があれば消す
        /// </summary>
        static public void Start
            ( Entity MotionEntity, EntityCommandBuffer.Concurrent cmd, int motionIndex, float timeScale, float delayTime = 0.0f )
        {

        }
        /// <summary>
        /// モーションとストリームに disable
        /// </summary>
        static public void Stop( Entity MotionEntity, EntityCommandBuffer.Concurrent cmd )
        {

        }
        /// <summary>
        /// スケールを 0 にする
        /// </summary>
        static public void Pause( Entity MotionEntity, EntityCommandBuffer.Concurrent cmd )
        {

        }


        static public void SetWeight( ref this MotionBlend2WeightData data, float weight0, float weight1 )
        {
            data.WeightNormalized0 = weight0 / ( weight0 + weight1 );
            data.WeightNormalized1 = 1.0f - data.WeightNormalized0;
        }
        static public void SetWeight( ref this MotionBlend3WeightData data, float weight0, float weight1, float weight2 )
        {
            var totalWeight = weight0 + weight1 + weight2;
            data.WeightNormalized0 = weight0 / totalWeight;
            data.WeightNormalized1 = weight1 / totalWeight;
            data.WeightNormalized2 = 1.0f - ( data.WeightNormalized0 + data.WeightNormalized1 );
        }
    }


    static class MotionUtility
    {


        static public void InitializeCursor(
            ref this MotionCursorData motionCursor, ref MotionBlobUnit motionClip,
            float delayTime = 0.0f, float scale = 1.0f
        )
        {
            motionCursor.TotalLength = motionClip.TimeLength;
            motionCursor.CurrentPosition = -delayTime;
            motionCursor.Scale = scale;
        }
        

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

    }
}
