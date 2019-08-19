//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;

using us = Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
using usx = Unity.Collections.LowLevel.Unsafe.UnsafeUtilityEx;

using Abss.Geometry;
using Abss.Misc;
using Abss.Draw;
using Abss.Motion;
using Abss.Model;
using Abss.UtilityOther;


namespace Abss.Motion
{
	
	/// <summary>
	/// モーションを
	/// </summary>
	[AlwaysUpdateSystem]
	[UpdateAfter(typeof(DrawCullingSystem))]
	public class MotionProgressSystem : JobComponentSystem
	{

		//[Inject]
		DrawCullingSystem	cullingSystem;
		//[Inject]
		CreateModelSystem	createModelSystem;

		public readonly NativeArray<BonePostureData> ResultBones;
		//public SegmentalNativeArray<BonePostureData>	ResultBones;

		//[Inject]
		BufferFromEntity<MotionStreamElement>	streamBuffers;
		//[Inject]
		ComponentDataFromEntity<MotionInfoData>	motions;
		//[Inject]
		ComponentDataFromEntity<DrawModelInfo>	models;

		// カリングの際に必要なデータを NativeArray に隙間なく詰めてしまおうかと思ったが、
		// 詰めた後にここで１回しか使用しないので、その時のランダムアクセスと同等なんじゃないかと思い、やめた。

		
		public MotionProgressSystem()
		{
			this.ResultBones = new NativeArray<BonePostureData>( 10000, Allocator.Persistent );
			//this.ResultBones = new SegmentalNativeArray<BonePostureData>( 3, 10000, Allocator.Persistent );
		}

		protected override void OnCreateManager()
		{
		}
		protected override void OnDestroyManager()
		{
			this.ResultBones.Dispose();
		}



		/// <summary>
		/// 
		/// </summary>
		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			var targetEntities = this.cullingSystem.ResultEntities;

			var t = Time.deltaTime;

			//inputDeps = new MotionProgressJob
			//{
			//	DeltaTime			= t,
			//	SrcEntities			= targetEntities.ToDeferredJobArray(),
			//	Motions				= this.motions,
			//	MotionStreamBuffers	= this.streamBuffers,
			//	Models				= this.models,
			//	ResultBones			= new BoneTransformDst( createModelSystem.Models )
			//}
			//.Schedule( targetEntities, 1, inputDeps );

			
			inputDeps.Complete();
			return inputDeps;
		}
		
	}


	/// <summary>
	/// モーション進行、ボーン変換、を行う。
	/// モデルモーションごとに列挙される。
	/// </summary>
	//[BurstCompile]
	internal struct MotionProgressJob : IJobParallelFor
	{

		[ReadOnly]
		public NativeArray<Entity>	SrcEntities;

		[ReadOnly]
		public ComponentDataFromEntity<DrawModelInfo>	Models;
		[ReadOnly]
		public ComponentDataFromEntity<MotionInfoData>	Motions;
		[NativeDisableParallelForRestriction]
		public BufferFromEntity<MotionStreamElement>	MotionStreamBuffers;

		//[WriteOnly][NativeDisableParallelForRestriction]
		//public NativeArray<BonePostureData>				DstBones;

		//public SegmentalNativeArray<BonePostureData>	ResultBones;

		public BoneTransformDst ResultBones;
		
		public float	DeltaTime;


		public unsafe void Execute( int index )
		{
			var ent		= this.SrcEntities[ index ];
			var motion	= this.Motions[ ent ];
			var model	= this.Models[ ent ];

			//var boneLength	= motion.DataAccessor.boneLength;
			
			//var currentStreamBuffer	= this.MotionStreamBuffers[ ent ];
			//var currentBoneSlice = this.ResultBones.BoneSlicePerModels[ model.modelIndex ];
			////var currentBoneSlice = this.ResultBones.resultBonePool.Slice( 0, 16 );

			//streamToBone( ref currentStreamBuffer, ref currentBoneSlice, in motion );
		}
		
		unsafe void streamToBone
		(
			ref DynamicBuffer<MotionStreamElement>	streams,
			ref NativeSlice<BonePostureData>		bones,
			in  MotionInfoData						motion
		)
		{
			var pStreams	= streams.GetUnsafePtr();
			var pBone = bones.GetUnsafePtr();

			for( var ibone = 0 ; ibone < motion.DataAccessor.boneLength ; ++ibone )
			{
				ref var bone = ref usx.ArrayElementAsRef<BonePostureData>( pBone, ibone );
				//var bone = new BonePostureData();

				bone.position = progressStream( ibone * 2 + 0, this.DeltaTime );
				bone.rotation = progressStream( ibone * 2 + 1, this.DeltaTime );

				//bones[ ibone ] = bone;
			}
			return;


			unsafe float4 progressStream( int istream, float deltaTime )
			{

				ref var stream = ref usx.ArrayElementAsRef<MotionStreamElement>( pStreams, istream );
				

				// ストリームを進ませる

				stream.Progress.Progress( deltaTime );

				//stream.Cache.ShiftKeysIfOverKeyTimeForLooping( ref stream.Shift, ref stream.Progress );
				StreamUtility.ShiftKeysIfOverKeyTimeForLooping
					( ref stream.Cache, ref stream.Shift, ref stream.Progress );


				//// 補間する

				var normalizedProgressTime = stream.Cache.CaluclateTimeNormalized( stream.Progress.TimeProgress );

				return stream.Cache.Interpolate( normalizedProgressTime );
			}
		}

	}

	
	// モデル描画について
	// ・モデル -> モデルインスタンス（ near / far ）
	// ・near/far は、ボーン数が同じなので、描画寸前まで仕分けは遅延できる。
	// 　near には最大数があり、それを超えると near の距離にあっても far で描画される。
	// ・モデルごとに異なるもの　…　ボーン数, メッシュ, モーション, マテリアル
	// 　このうち、near/far では、メッシュ以外は共有できる。
	// ・


	// カリング、進行、トランスフォーム まではごちゃまぜ
	// 描画前で モデル 別になる。

	/// <summary>
	/// MotionProgressJob で扱うべきデータソース
	/// </summary>
	struct MotionProgressSrc
	{

		[ReadOnly]
		NativeArray<Entity>	srcEntities;
		
		[ReadOnly]
		ComponentDataFromEntity<DrawModelInfo>	models;
		[ReadOnly]
		ComponentDataFromEntity<MotionInfoData>	motions;
		[NativeDisableParallelForRestriction]
		BufferFromEntity<MotionStreamElement>	motionStreamBuffers;
		

		public MotionProgressSrc(
			in NativeArray<Entity>	srcEntities,
			in ComponentDataFromEntity<DrawModelInfo>	models,
			in ComponentDataFromEntity<MotionInfoData>	motions,
			in BufferFromEntity<MotionStreamElement>	motionStreamBuffers
		)
		{
			this.srcEntities			= srcEntities;
			this.models					= models;
			this.motions				= motions;
			this.motionStreamBuffers	= motionStreamBuffers;
		}
	}

	/// <summary>
	/// MotionProgressJob の結果を格納する。 
	/// </summary>
	struct BoneTransformDst
	{
		//[WriteOnly]
		[DeallocateOnJobCompletion]// readonly をつけると開放されなくなってしまうみたい
		[NativeDisableParallelForRestriction]
		public NativeArray<BonePostureData>	ResultBones;
		
		[NativeDisableParallelForRestriction]
		public NativeArray<(int start, int length)>	DrawInstanceCounter;

		/// <summary>
		/// 
		/// </summary>
		public BoneTransformDst( ModelInfoForEntity[] models )
		{

			this.ResultBones =
				new NativeArray<BonePostureData>( countTotal(), Allocator.TempJob );

			this.DrawInstanceCounter =
				new NativeArray<(int,int)>( models.Length, Allocator.TempJob );

			//setBoneSlices( ref this.ResultBones, ref this.BoneSlicePerModels );

			return;
			

			int countTotal()
			{
				var t = 0;

				foreach( var m in models ) t += m.ModelInstanceCount * m.BoneLength;
				
				return t;
			}
			
			void setBoneSlices(
				ref NativeArray<BonePostureData>				resultBonePool,
				ref NativeArray<NativeSlice<BonePostureData>>	boneSlicePerModels
			){
				var t = 0;
				for( var i = 0; i < models.Length; ++i )
				{
					var m		= models[i];
					var span	= m.ModelInstanceCount * m.BoneLength;

					boneSlicePerModels[i] = resultBonePool.Slice( t, span );
					
					t += span;
				}
			}
		}

		public struct ConcurrentDiscardReadOnly
		{
			[ReadOnly]
			[NativeDisableParallelForRestriction]
			[DeallocateOnJobCompletion]
			NativeArray<BonePostureData>	resultBonePool;
			
			[ReadOnly]
			[DeallocateOnJobCompletion]
			public NativeArray<NativeSlice<BonePostureData>>	BoneSlicePerModels;
		}
	}

	struct ThreadWorkingArea
	{
		[NativeDisableParallelForRestriction]
		NativeArray<BonePostureData>				tmpCalcBones;
		
		[NativeSetThreadIndex]
		int	threadId;

		//public ThreadWorkingArea()
		//{
		//	JobsUtility.MaxJobThreadCount
		//}
	}


	internal struct BoneTransformJob : IJobParallelFor
	{
		public void Execute( int index )
		{

		}
	}
}


