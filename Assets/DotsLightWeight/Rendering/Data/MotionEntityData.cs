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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.CharacterMotion
{
    using Abarabone.Geometry;
	

    static public partial class Motion
    {
        public struct InitializeData : IComponentData
        {
            public int MotionIndex;
            public float DelayTime;
            public float TimeScale;
            public bool IsLooping;
            public bool IsContinuous;
            public bool IsChangeMotion;
        }


        public struct ClipData : IComponentData
        {
            public BlobAssetReference<MotionBlobData> MotionClipData;
        }


        public struct SleepOnDrawCullingTag : IComponentData
        { }

        public struct DrawCullingData : IComponentData
        {
            public bool IsDrawTarget;
            public Entity DrawInstanceEntity;
        }

        public struct InfoData : IComponentData
        {
            public int MotionIndex;
        }

        public struct StreamLinkData : IComponentData
        {
            public Entity PositionStreamTop;
            public Entity RotationStreamTop;
        }

        //public struct MotionATag : IComponentData// MotionB と区別するため暫定
        //{ }
        public struct CursorData : IComponentData// MotionB 用
        {
            public float CurrentPosition;
            public float TotalLength;
            public float Scale;
        }
        public struct ProgressTimerTag : IComponentData// MotionB 用
        { }
    }



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

    

    public ref struct MotionOperator
    {
        Motion.InfoData info;
        //Motion.CursorData cursor;

        EntityCommandBuffer.ParallelWriter cmd;
        Entity entity;
        int jobIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MotionOperator(
            EntityCommandBuffer.ParallelWriter command,
            ComponentDataFromEntity<Motion.InfoData> motionInfos,
            ComponentDataFromEntity<Motion.CursorData> motionCursors,
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start(
            int motionIndex, bool isLooping, float delayTime = 0.0f, bool isContinuous = true, float scale = 1.0f )
        {
            if( this.info.MotionIndex == motionIndex ) return;

            this.cmd.AddComponent( this.jobIndex, this.entity, new Motion.ProgressTimerTag { } );

            this.cmd.AddComponent( this.jobIndex, this.entity,
                new Motion.InitializeData
                {
                    MotionIndex = motionIndex,
                    DelayTime = delayTime,
                    IsContinuous = isContinuous,
                    IsLooping = isLooping,
                    TimeScale = scale,
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            this.cmd.RemoveComponent<Motion.ProgressTimerTag>( this.jobIndex, this.entity );
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Change( int motionIndex, bool isContinuous = true )
        {
            cmd.AddComponent( this.jobIndex, this.entity,
                new Motion.InitializeData
                {
                    MotionIndex = motionIndex,
                    IsContinuous = isContinuous,
                    IsChangeMotion = true,
                }
            );
        }
    }

    static public class MotionOp
    {
        /// <summary>
        /// モーション初期化セット、disable があれば消す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Start( int entityIndex,
            ref EntityCommandBuffer.ParallelWriter cmd, Entity motinEntity, Motion.InfoData motionInfo,
            int motionIndex, bool isLooping, float delayTime = 0.0f, bool isContinuous = true, float scale = 1.0f
        )
        {
            if( motionInfo.MotionIndex == motionIndex ) return;

            cmd.AddComponent( entityIndex, motinEntity,
                new Motion.InitializeData
                {
                    MotionIndex = motionIndex,
                    DelayTime = delayTime,
                    IsContinuous = isContinuous,
                    IsLooping = isLooping,
                    TimeScale = scale,
                }
            );
            cmd.AddComponent( entityIndex, motinEntity, new Motion.ProgressTimerTag { } );
        }
        /// <summary>
        /// モーションとストリームに disable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Stop( int entityIndex, ref EntityCommandBuffer.ParallelWriter cmd, Entity motionEntity )
        {
            cmd.RemoveComponent<Motion.ProgressTimerTag>( entityIndex, motionEntity );
        }
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Change( int entityIndex,
            ref EntityCommandBuffer.ParallelWriter cmd, Entity motinEntity, int motionIndex, bool isContinuous = true
        )
        {
            cmd.AddComponent( entityIndex, motinEntity,
                new Motion.InitializeData
                {
                    MotionIndex = motionIndex,
                    IsContinuous = isContinuous,
                    IsChangeMotion = true,
                }
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void SetWeight( ref MotionBlend2WeightData data, float weight0, float weight1 )
        {
            data.WeightNormalized0 = weight0 / ( weight0 + weight1 );
            data.WeightNormalized1 = 1.0f - data.WeightNormalized0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void InitializeCursor(
            ref this Motion.CursorData motionCursor, ref MotionBlobUnit motionClip,
            float delayTime = 0.0f, float scale = 1.0f
        )
        {
            motionCursor.TotalLength = motionClip.TimeLength;
            motionCursor.CurrentPosition = -delayTime;
            motionCursor.Scale = scale;
        }
        

    }
}
