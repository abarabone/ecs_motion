using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

namespace Abss.m
{
	[AlwaysUpdateSystem]
	public class MotionSystem : JobComponentSystem
	{
		
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			inputDeps = new MotionJob
			{

			}
			.Schedule( this, inputDeps );

			return inputDeps;
		}

		struct MotionJob : IJobForEach<Motion.BonePostureData>
		{
			public void Execute( ref Motion.BonePostureData c0 )
			{
				c0.position.x += 0.01f;
			}
		}
	}
}
