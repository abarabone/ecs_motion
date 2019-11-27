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
        public float CurrentPosition;
        public float TotalLength;
        public float Scale;
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

    

    public struct MotionOperator
    {
        MotionInfoData info;
        //MotionCursorData cursor;

        EntityCommandBuffer.Concurrent cmd;
        Entity entity;
        int jobIndex;

        public MotionOperator(
            EntityCommandBuffer.Concurrent command,
            ComponentDataFromEntity<MotionInfoData> motionInfos,
            ComponentDataFromEntity<MotionCursorData> motionCursors,
            Entity motionEntity, int jobIndex
        )
        {
            this.info = motionInfos[ motionEntity ];
            //this.cursor = motionCursors[ motionEntity ];
            this.entity = motionEntity;
            this.jobIndex = jobIndex;
            this.cmd = command;
        }


        /// <summary>
        /// モーション初期化セット
        /// </summary>
        public void Start( int motionIndex, bool isLooping, float delayTime = 0.0f, bool isContinuous = true )
        {
            if( this.info.MotionIndex == motionIndex ) return;

            this.cmd.AddComponent( this.jobIndex, this.entity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    DelayTime = delayTime,
                    IsContinuous = isContinuous,
                    IsLooping = isLooping,
                }
            );
            cmd.AddComponent( this.jobIndex, this.entity, new MotionProgressTimerTag { } );
        }

        /// <summary>
        /// モーションとストリームに disable
        /// </summary>
        public void Stop( int entityIndex, ref EntityCommandBuffer.Concurrent cmd, Entity motionEntity )
        {
            this.cmd.RemoveComponent<MotionProgressTimerTag>( this.jobIndex, this.entity );
        }

        /// <summary>
        /// 
        /// </summary>
        public void Change( int motionIndex, bool isContinuous = true )
        {
            cmd.AddComponent( this.jobIndex, this.entity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    IsContinuous = isContinuous,
                }
            );
        }
    }

    static public class MotionOp
    {
        /// <summary>
        /// モーション初期化セット、disable があれば消す
        /// </summary>
        static public void Start( int entityIndex,
            ref EntityCommandBuffer.Concurrent cmd, Entity motinEntity, MotionInfoData motionInfo,
            int motionIndex, bool isLooping, float delayTime = 0.0f, bool isContinuous = true
        )
        {
            if( motionInfo.MotionIndex == motionIndex ) return;

            cmd.AddComponent( entityIndex, motinEntity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    DelayTime = delayTime,
                    IsContinuous = isContinuous,
                    IsLooping = isLooping,
                }
            );
            cmd.AddComponent( entityIndex, motinEntity, new MotionProgressTimerTag { } );
        }
        /// <summary>
        /// モーションとストリームに disable
        /// </summary>
        static public void Stop( int entityIndex, ref EntityCommandBuffer.Concurrent cmd, Entity motionEntity )
        {
            cmd.RemoveComponent<MotionProgressTimerTag>( entityIndex, motionEntity );
        }
        /// <summary>
        /// 
        /// </summary>
        static public void Change( int entityIndex,
            ref EntityCommandBuffer.Concurrent cmd, Entity motinEntity, int motionIndex, bool isContinuous = true
        )
        {
            cmd.AddComponent( entityIndex, motinEntity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    IsContinuous = isContinuous,
                }
            );
        }


        static public void SetWeight( ref MotionBlend2WeightData data, float weight0, float weight1 )
        {
            data.WeightNormalized0 = weight0 / ( weight0 + weight1 );
            data.WeightNormalized1 = 1.0f - data.WeightNormalized0;
        }
        static public void SetWeight( ref MotionBlend3WeightData data, float weight0, float weight1, float weight2 )
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
        

    }
}
