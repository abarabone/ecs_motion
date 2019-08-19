using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
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

namespace Abss.Obj.Entities
{
	
	public struct ObjectInitializeLabel : IComponentData
	{
		public Entity	ObjectEntity;
	}

	public struct ObjectPostureData : IComponentData
	{
		public float3		position;
		public quaternion	rotation;
		public float3		scale;
	}
	
	public struct ObjectDrawTargetLabel : IComponentData
	{
		public Entity	ObjectEntity;
		public Entity	BoneEntityTop;
		public int		BoneLength;
	}

	public struct ObjectDrawingInFrameLabel : IComponentData
	{
		public int  orderId;
	}
	
}