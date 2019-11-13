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
	
}
