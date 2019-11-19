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

    
	public struct BoneBlend2WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
    }
    public struct BoneBlend3WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
        public float WeightNormalized2;
    }

    public struct BlendBoneFadeData : IComponentData
	{
		public float	FadeTime;
	}

    


	static public class BlendExtension
	{
		static public void SetWeight( ref this BoneBlend2WeightData data, float weight0, float weight1 )
		{
			data.WeightNormalized0 = weight0 / ( weight0 + weight1 );
            data.WeightNormalized1 = 1.0f - data.WeightNormalized0;
        }
        static public void SetWeight( ref this BoneBlend3WeightData data, float weight0, float weight1, float weight2 )
        {
            var totalWeight = weight0 + weight1 + weight2;
            data.WeightNormalized0 = weight0 / totalWeight;
            data.WeightNormalized1 = weight1 / totalWeight;
            data.WeightNormalized2 = 1.0f - (data.WeightNormalized0 + data.WeightNormalized1);
        }
    }

}