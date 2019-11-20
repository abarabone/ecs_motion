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
        //public float CurrentPosition;
        //public float TotalLength;
        //public float Scale;
        public StreamTimeProgressData Timer;
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

}
