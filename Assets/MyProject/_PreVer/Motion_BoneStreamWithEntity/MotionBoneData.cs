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
using Abss.Obj.Entities;

namespace Abss.Motion.WithComponent
{


	//public struct BoneMotionLabel : IComponentData
	//{}

	//public struct BoneDrawableLabel : IComponentData
	//{}

	public struct BoneEntityLinkData : IComponentData
	{
		public Entity	NextEntity;
		public Entity	ParentEntity;

		public Entity	positionEntity;
		public Entity	rotationEntity;
	}
	
	//public struct BoneIndexData : IComponentData	//初期化時だけあればいいか？
	//{
	//	public int  Index;
	//	public int  ParentIndex;
	//}

	public struct BonePostureData : IComponentData
	{
		public float3		position;
		public quaternion	rotation;
	//	public float3		scale;
	}

	public struct BoneDrawingInFrameLabel : IComponentData
	{
		public int	boneSerialIndex;	// drawModelIndex * boneLength + boneIndex
	}
	

	

}