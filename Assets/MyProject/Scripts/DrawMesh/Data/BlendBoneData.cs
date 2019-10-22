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

namespace Abss.Motion
{


	public struct BlendBoneSourceLinkData : IComponentData
	{
		public Entity	SrcBone0;
		public Entity	SrcBone1;
	}

	public struct BlendBoneWeightData : IComponentData
	{
		public float	WeightNormalized;
	}

	public struct BlendBoneFadeData : IComponentData
	{
		public float	FadeTime;
	}




	static public class BlendExtension
	{
		static public void SetWeight( ref this BlendBoneWeightData data, float weight0, float weight1 )
		{
			data.WeightNormalized = weight0 / ( weight0 + weight1 );
		}
	}

}