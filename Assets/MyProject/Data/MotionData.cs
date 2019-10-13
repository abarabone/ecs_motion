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
        //public int MotionIndex;
        //public MotionDataInNative MotionData;
    }

    public struct MotionClipData : IComponentData
    {
        public BlobAssetReference<MotionBlobData> ClipData;
    }

    public struct MotionInfoData : IComponentData
	{
        public int MotionIndex;
    }

	//public struct MotionProgressData : IComponentData
	//{
	//	public float4x4	Matrix;
	//}



	//[InternalBufferCapacity(16*2)]
	//public struct MotionStreamElement : IBufferElementData
	//{
	//	public StreamKeyShiftData		Shift;
	//	public StreamTimeProgressData	Progress;
	//	public StreamNearKeysCacheData	Cache;
	//}
	
	
}
