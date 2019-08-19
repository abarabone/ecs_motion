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
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;

using Abss.Motion;
using Abss.Geometry;
using Abss.Obj.Entities;
using Abss.Misc;
using Unity.Jobs.LowLevel.Unsafe;

namespace Abss.Draw
{

	[AlwaysUpdateSystem]
	public class DrawCullingSystem : JobComponentSystem
	{

		NativeList<Entity>			resultEntities;
		public NativeList<Entity>	ResultEntities => this.resultEntities;


		protected override void OnCreateManager()
		{
			this.resultEntities = new NativeList<Entity>( 10000, Allocator.Persistent );
		}
		protected override void OnDestroyManager()
		{
			this.resultEntities.Dispose();
		}


		protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
		{
			//this.resultEntities = new NativeList<Entity>( 10000, Allocator.TempJob );
			this.ResultEntities.Clear();


			var coneJob = new DrawCurringJobByCone
			{
				dstDrawTargetEntities = this.resultEntities.ToConcurrent()
			}
			.Schedule( this, inputDeps );

			var aabbJob = new DrawCurringJobByFrustum
			{
				dstDrawTargetEntities = this.resultEntities.ToConcurrent()
			}
			.Schedule( this, inputDeps );


			return JobHandle.CombineDependencies( coneJob, aabbJob );
			//var a = JobHandle.CombineDependencies( coneJob, aabbJob );
			//a.Complete();
			//return a;
		}

		


	}
	

	[BurstCompile]
	struct DrawCurringJobByCone : IJobProcessComponentDataWithEntity<DrawTargetSphere>
	{
		public NativeListConcurrent<Entity> dstDrawTargetEntities;
		
		public void Execute( Entity ent, int index, [ReadOnly] ref DrawTargetSphere target )
		{
			dstDrawTargetEntities.Add( in ent );
		}
	}
	
	[BurstCompile]
	struct DrawCurringJobByFrustum : IJobProcessComponentDataWithEntity<DrawTargetAabb>
	{
		public NativeListConcurrent<Entity> dstDrawTargetEntities;
		
		public void Execute( Entity ent, int index, [ReadOnly] ref DrawTargetAabb target )
		{
			dstDrawTargetEntities.Add( in ent );
		}
	}

}
