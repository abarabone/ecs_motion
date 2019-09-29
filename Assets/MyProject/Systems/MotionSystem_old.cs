using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;

using Abss.Geometry;


namespace Abss.Motion.WithComponent
{
	
	
	
	/// <summary>
	/// モーションを
	/// </summary>
	public class MotionProgressSystem : JobComponentSystem
	{
		
		[Inject]
		EndFrameBarrier streamInitialBarrier;

		[Inject] [ReadOnly]
		ComponentDataFromEntity<MotionInfoData> motions;

		[Inject] [ReadOnly]
		ComponentDataFromEntity<BoneEntityLinkData> boneEntityLinks;
		[Inject]
		ComponentDataFromEntity<BonePostureData>	bonePostures;
		
		[Inject] [ReadOnly]
		public ComponentDataFromEntity<StreamInterpolatedData>	interpolateds;

		


		struct MotionGroup
		{
			[ReadOnly]
			public ComponentDataArray<MotionInfoData>	motions;
		}

		[Inject]
		MotionGroup	motionGroup;


		struct StreamInitialGroup
		{
			[ReadOnly]
			public ComponentDataArray<StreamInitialLabel>	Inits;
			[ReadOnly]
			public EntityArray	Entities;
		}

		[Inject]
		StreamInitialGroup	streamInitialGroup;



		/// <summary>
		/// 
		/// </summary>
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			//return inputDeps;

			var deltaTime = Time.deltaTime;

			var streamStartJob = new StreamStartJob();
			inputDeps = streamStartJob.Schedule( this, inputDeps );//( this, 64, inputDeps );

			var streamInitRemoveJob	= new StreamRemoveInitialJob
			{
				Commands	= streamInitialBarrier.CreateCommandBuffer().ToConcurrent(),
				entities	= streamInitialGroup.Entities,
			};
			inputDeps = streamInitRemoveJob.Schedule( streamInitialGroup.Entities.Length, 16, inputDeps );
			//Debug.Log( $"{streamInitRemoveJob.entities.Length} {streamInitRemoveJob.Length}" );

			//var streamStartJob2 = new StreamStartFor1posJob();
			//inputDeps = streamStartJob2.Schedule( this, 64, inputDeps );
			//var streamStartJob3 = new StreamStartFor1posJob2
			//{
			//	Commands	= streamInitialBarrier.CreateCommandBuffer()
			//};
			//inputDeps = streamStartJob3.Schedule( this, 64, inputDeps );


			var interpolateJob = new StreamInterpolateJob
			{
				DeltaTime	= deltaTime
			};
			inputDeps = interpolateJob.Schedule( this, inputDeps );//( this, 64, inputDeps );


			var aggregateToBoneJob = new BoneAggregationJob
			{
				Interpolateds   = interpolateds
			};
			inputDeps = aggregateToBoneJob.Schedule( this, inputDeps );//( this, 32, inputDeps );


			//return inputDeps;

			var boneTransformJob = new BoneTransformJob
			{
				boneEntityLinks = boneEntityLinks,
				bonePostures    = bonePostures,
			};
			inputDeps = boneTransformJob.Schedule( this, inputDeps );//( this, 4, inputDeps );

			//var bones = new NativeArray<BonePostureData>( 1023 * 100, Allocator.TempJob, NativeArrayOptions.UninitializedMemory );
			//var boneCopyAndTransformJob = new BoneTransformWithTempBufferJob
			//{
			//	motions			= motionGroup.motions,
			//	boneEntityLinks	= boneEntityLinks,
			//	bonePostures	= bonePostures,
			//	dstbones		= bones,
			//};
			//inputDeps = boneCopyAndTransformJob.ScheduleBatch( motionGroup.motions.Length * 16, 16, inputDeps );
			//var boneTransformCopyJob = new BoneTransformCopyFromTempBufferJob
			//{
			//	BoneStride	= 16,
			//	srcbones	= bones,
			//};
			//inputDeps = boneTransformCopyJob.Schedule( this, 32, inputDeps );

			inputDeps.Complete();
			return inputDeps;
			
		}





		/// <summary>
		/// ストリームの初期化
		/// </summary>
		//[BurstCompile]
		struct StreamRemoveInitialJob : IJobParallelFor
		{
			public EntityCommandBuffer.Concurrent	Commands;

			[ReadOnly]
			public EntityArray	entities;
			
			public void Execute( int i )
			{
				Commands.RemoveComponent<StreamInitialLabel>( 0, entities[ i ] );
			}
		}
		[BurstCompile]
		struct StreamStartJob : IJobProcessComponentData<StreamInitialLabel, StreamKeyShiftData, StreamNearKeysCacheData>
		{
			public void Execute( [ReadOnly] ref StreamInitialLabel init, ref StreamKeyShiftData shiftInfo, [WriteOnly] ref StreamNearKeysCacheData nearKeys )
			{
				StreamExtentions.InitializeKeys( ref nearKeys, ref shiftInfo );
			}
		}
		[BurstCompile]
		struct StreamStartFor1posJob : IJobProcessComponentData<StreamInitialLabelFor1pos, StreamKeyShiftData, StreamInterpolatedData>
		{
			public void Execute( [ReadOnly] ref StreamInitialLabelFor1pos inits, [ReadOnly] ref StreamKeyShiftData shifter, [WriteOnly] ref StreamInterpolatedData dst )
			{
				dst.Value = shifter.Keys[0].Value;
			}
		}
		//[BurstCompile]
		struct StreamStartFor1posJob2 : IJobParallelFor
		{
			public EntityCommandBuffer.Concurrent	Commands;

			[ReadOnly]
			public EntityArray	entities;
			
			public void Execute( int i )
			{
				Commands.RemoveComponent<StreamInitialLabelFor1pos>( 0, entities[ i ] );
				Commands.RemoveComponent<StreamKeyShiftData>( 0, entities[ i ] );
				Commands.RemoveComponent<StreamNearKeysCacheData>( 0, entities[ i ] );
			}
		}


		

		/// <summary>
		/// ストリーム回転　→補間→　ボーン
		/// </summary>
		[BurstCompile]
		struct StreamInterpolateJob : IJobProcessComponentData<StreamKeyShiftData, StreamNearKeysCacheData, StreamInterpolatedData>
		{
			
			public float	DeltaTime;


			public void Execute( ref StreamKeyShiftData shiftInfo, ref StreamNearKeysCacheData nearKeys, [WriteOnly] ref StreamInterpolatedData dst )
			{
				nearKeys.Progress( DeltaTime );

				StreamExtentions.ShiftKeysIfOverKeyTimeForLooping( ref nearKeys, ref shiftInfo );

				var timeProgressNormalized	= nearKeys.CaluclateTimeNormalized();

				dst.Value = nearKeys.Interpolate( timeProgressNormalized );
			}

			//void endStreamByTime( ref StreamComponentData nearKeys )
			//{
			//	if( nearKeys.TimeProgress < nearKeys.TimeLength ) return;

			//	// コマンドでリムーブする？
			//}
			
		}
		


		/// <summary>
		/// ストリーム → ボーンへ値を収集
		/// </summary>
		[BurstCompile]
		struct BoneAggregationJob : IJobProcessComponentData<BoneEntityLinkData, BonePostureData>
		{

			[ReadOnly]
			public ComponentDataFromEntity<StreamInterpolatedData>	Interpolateds;

			
			public void Execute( [ReadOnly] ref BoneEntityLinkData boneLinker, [WriteOnly] ref BonePostureData bonePosture )
			{
				bonePosture.position = Interpolateds[ boneLinker.positionEntity ].Value.As_float3();
				bonePosture.rotation = Interpolateds[ boneLinker.rotationEntity ].Value.As_quaternion();
			}
			
		}
		
		/// <summary>
		/// ボーンのトランスフォーム
		/// </summary>
		[BurstCompile]
		struct BoneTransformJob : IJobProcessComponentData<MotionInfoData>
		{
			[ReadOnly]
			public ComponentDataFromEntity<BoneEntityLinkData>	boneEntityLinks;
			
			[NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<BonePostureData>		bonePostures;
			
			public void Execute( [ReadOnly] ref MotionInfoData motion )
			{
				
				for( var boneEnt = motion.BoneEntityTop; boneEnt != Entity.Null; boneEnt = boneEntityLinks[boneEnt].NextEntity )
				{
					
					var parentEnt = boneEntityLinks[ boneEnt ].ParentEntity;
					
					if( parentEnt == Entity.Null ) continue;
					
					var bone		= bonePostures[ boneEnt ];
					var parentBone	= bonePostures[ parentEnt ];
					
					var lpos	= bone.position;
					var lrot	= bone.rotation;
						
					var ppos	= parentBone.position;
					var prot	= parentBone.rotation;

					//var mpos    = math.mul( prot, lpos ) + ppos;
					//var mrot    = math.mul( prot, lrot );

					//bone.position = mpos;
					//bone.rotation = mrot;
					//bonePostures[boneEnt] = bone;

					// なぜかこうすると動く、コンパイラの問題？
					// →環境によってもちがう、なんだろう simd 関係とか？？？
					bonePostures[ boneEnt ] = new BonePostureData
					{
						position = math.mul( prot, lpos ) + ppos,
						rotation = math.mul( prot, lrot ),
					};

					//Debug.Log( $"{bones[boneEnt].boneIndex} p:{ppos.ToString("f2")} l:{lpos.ToString("f2")} m:{mpos.ToString("f2")}" );

				}
				
			}
		}
		/*
	//	[BurstCompile]
		struct BoneTransformWithTempBufferJob : IJobParallelForBatch
		{
			[ReadOnly]
			public ComponentDataArray<MotionComponentData>		motions;
			[ReadOnly]
			public ComponentDataFromEntity<BoneEntityLinkData>	boneEntityLinks;
			[ReadOnly]
			public ComponentDataFromEntity<BonePostureData>		bonePostures;
			
			public NativeArray<BonePostureData>	dstbones;

			public void Execute( int startIndex, int count )
			{
				var imotion	= startIndex / count;
				var ibone	= startIndex;
				for( var boneEnt = motions[imotion].BoneTop; boneEnt != Entity.Null; boneEnt = boneEntityLinks[boneEnt].Sibling )
				{
					if( ibone >= startIndex + count ) return;

					var parentIndex = boneEntityLinks[ boneEnt ].ParentIndex;

					if( parentIndex == -1 )
					{
						dstbones[ ibone++ ] = bonePostures[ boneEnt ];
						continue;
					}

					var parentPosture	= dstbones[ startIndex + parentIndex ];
					var thisPosture		= bonePostures[ boneEnt ];
					
					var lpos	= thisPosture.position;
					var lrot	= thisPosture.rotation;
						
					var ppos	= parentPosture.position;
					var prot	= parentPosture.rotation;

					var mpos    = math.mul( prot, lpos ) + ppos;
					var mrot    = math.mul( prot, lrot );

					dstbones[ ibone++ ] = new BonePostureData
					{
						position = mpos,
						rotation = mrot,
					};
				}
			}
			public void Execute_( int startIndex, int count )
			{
				var imotion	= startIndex / count;
				var ibone	= startIndex;
				for( var boneEnt = motions[imotion].BoneTop; boneEnt != Entity.Null; boneEnt = boneEntityLinks[boneEnt].Sibling )
				{
					if( ibone >= startIndex + count ) return;

					dstbones[ ibone++ ] = bonePostures[ boneEnt ];
					
				}
				for( var i = startIndex ; i < ibone ; i++ )
				{
					var p = dstbones[ startIndex ];
					dstbones[ startIndex ] = new BonePostureData
					{
						position = dstbones[ i++ ].position,
						rotation = p.rotation
					};
				}
			}
		}
		[BurstCompile]
		struct BoneTransformCopyFromTempBufferJob : IJobProcessComponentData<BoneDrawTargetLabel, BonePostureData>
		{
			[ReadOnly] [DeallocateOnJobCompletion]
			public NativeArray<BonePostureData>	srcbones;

			[ReadOnly]
			public int	BoneStride;

			public void Execute( ref BoneDrawTargetLabel drawer, ref BonePostureData posture )
			{
				//var ibone = drawer.drawingOrder * BoneStride + drawer.boneIndex;
				//posture = srcbones[ ibone ];
				posture = srcbones[ drawer.drawingOrder ];
			}
		}
		*/
	}
	




}

