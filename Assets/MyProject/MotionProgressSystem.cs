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
using System.Runtime.InteropServices;

namespace Abss.Motion
{
	

	
	public class MotionProgressSystem : JobComponentSystem
	{
		protected override void OnCreate()
		{
			
		}

		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			return inputDeps;
		}
	}


	public static class MotionCreator
	{
		static public void Create( EntityCommandBuffer ecb )
		{

			ecb.CreateEntity( archetype, new NativeArray<int>() )
				var em	= world.GetOrCreateManager<EntityManager>();

				var ent = em.CreateEntity( this.motionArchetype );
			
				var ma	= modelData.CreateMotionAccessor( motionIndex );


				em.SetComponentData<MotionInfoData>
				(
					ent,
					new MotionInfoData
					{ 
						MotionIndex		= 0,
						DataAccessor	= ma
					}
				);
				//em.SetComponentData<DrawTargetSphere>
				//(
				//	ent,
				//	new DrawTargetSphere
				//	{
				//		center	= 0,
				//		radius	= 1
				//	}
				//);
				//em.SetComponentData<DrawModelInfo>
				//(
				//	ent,
				//	new DrawModelInfo
				//	{
				//		modelIndex	= 0
				//	}
				//);
				var buf = em.GetBuffer<MotionStreamElement>( ent );
				for( var i = 0; i < ma.boneLength * 2 ; ++i )
				{
					var stream = new MotionStreamElement();

					stream.Progress.TimeLength	= ma.TimeLength;
					stream.Progress.TimeScale	= 1.0f;
					stream.Shift.Keys = ma.GetStreamSlice( i >> 2, KeyStreamSection.positions + (i & 1) ).Keys;

					StreamUtility.InitializeKeys( ref stream.Cache, ref stream.Shift, ref stream.Progress );

					buf.Add( stream );
				}
			}
		

		}

	
		//static EntityArchetype CreateArchetypes( EntityManager em ) =>
		//	em.CreateArchetype
		//	(
		//		ComponentType..Create<MotionInfoData>(),
		//		ComponentType.Create<MotionStreamElement>(),
		//		ComponentType.Create<DrawTargetSphere>(),
		//		ComponentType.Create<DrawModelInfo>()
		//	);
	}



	/// <summary>
	/// ストリーム回転　→補間→　ボーン
	/// </summary>
	[BurstCompile]
	struct StreamInterpolateJob : IJobForEach
		<StreamTimeProgressData, StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
	{
			
		public float	DeltaTime;


		public void Execute
			(
				ref StreamTimeProgressData timer,
				ref StreamKeyShiftData shiftInfo,
				ref StreamNearKeysCacheData nearKeys,
				[WriteOnly] ref StreamInterpolatedData dst
			)
		{
			timer.Progress( DeltaTime );

			nearKeys.ShiftKeysIfOverKeyTimeForLooping( ref shiftInfo, ref timer );
			
			var timeProgressNormalized	= nearKeys.CaluclateTimeNormalized( timer.TimeProgress );

			dst.Value = nearKeys.Interpolate( timeProgressNormalized );
		}
			
	}
		
}
