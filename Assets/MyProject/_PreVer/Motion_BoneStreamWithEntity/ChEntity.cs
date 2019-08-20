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
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

using Abss.Motion.WithComponent;

namespace Abss.Obj.Entities.WithComponent
{

	
	[UpdateBefore( typeof(MotionProgressSystem) )]
	public class ObjectSystem : JobComponentSystem
	{

		[Inject]
		EndFrameBarrier	endFrameBarrier;

		[Inject]
		MotionCreateCommandSystem	motionCreateCommandSystem;
		

		
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			//return inputDeps;

			var motionStartJob = new ObjectMotionStartJob
			{
				MotionCommand   = motionCreateCommandSystem.CreateCommandQueue(),
				Command			= endFrameBarrier.CreateCommandBuffer().ToConcurrent(),
			};
			inputDeps = motionStartJob.Schedule( this, inputDeps );//( this, 8, inputDeps );
			
			motionCreateCommandSystem.SetUserJob( inputDeps );

			inputDeps.Complete();
			return inputDeps;
		}
		

		//[BurstCompile]
		struct ObjectMotionStartJob : IJobProcessComponentData<ObjectInitializeLabel>
		{
			
			[WriteOnly]
			public MotionEntityCreateCommandQueueConcurrent	MotionCommand;

			[WriteOnly]
			public EntityCommandBuffer.Concurrent			Command;


			public void Execute( [ReadOnly] ref ObjectInitializeLabel o )
			{
				
				MotionCommand.PostCreateMotion(
					new MotionEntityCreateCommand
					{
						motionIndex	= 5,
						timeScale	= 0.1f,

						drawTargetEntity	= o.ObjectEntity,
					}
				);
				
				Command.RemoveComponent<ObjectInitializeLabel>( 0, o.ObjectEntity );
			}
		}
		
	}

}