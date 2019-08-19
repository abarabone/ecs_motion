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

namespace Abss.Motion
{
	
	public class BlendBoneSystem : JobComponentSystem
	{
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			return inputDeps;
		}


		[BurstCompile]
		struct BlendBoneJob : IJobProcessComponentData<BlendBoneSourceLinkData, BlendBoneWeightData, BonePostureData>
		{
			public ComponentDataFromEntity<BonePostureData>	Postures;

			public void Execute( [ReadOnly] ref BlendBoneSourceLinkData s, [ReadOnly] ref BlendBoneWeightData w, [ReadOnly] ref BonePostureData d )
			{
				var w0	= w.WeightNormalized;
				var w1	= 1.0f - w.WeightNormalized;
				
				var p0	= Postures[ s.SrcBone0 ];
				var p1	= Postures[ s.SrcBone1 ];

				d.position = ( p0.position * w0 + p1.position * w1 );
				d.rotation = ( p0.rotation.value * w0 + p1.rotation.value * w1 ).As_quaternion();
			}
		}
	}

}
