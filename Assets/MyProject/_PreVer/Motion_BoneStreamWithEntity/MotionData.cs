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

namespace Abss.Motion.WithComponent
{
	

	public struct MotionInitializeData : IComponentData
	{}
	

	public struct MotionInfoData : IComponentData
	{
		public MotionDataAccessor	DataAccessor;
		
		public Entity	BoneEntityTop;

		public int		MotionIndex;
	}

}
