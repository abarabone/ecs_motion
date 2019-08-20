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

using Abss.Geometry;
using Abss.Obj.Entities;
using Abss.Motion.WithComponent;

namespace Abss.Draw.WithEntity
{

	[UpdateAfter( typeof(MotionProgressSystem) )]
	public unsafe class CopyPosturesForDrawInstancedSkinMeshSystem : JobComponentSystem
	{

		[WriteOnly]
		internal NativeArray<Matrix4x4>	maxRootMatrices;
		[WriteOnly]
		internal NativeArray<Vector4>	maxPositions;
		[WriteOnly]
		internal NativeArray<Vector4>	maxRotations;


		
		[Inject] [ReadOnly]
		ComponentDataFromEntity<BoneEntityLinkData> boneEntityLinks;
		[Inject] [ReadOnly]
		ComponentDataFromEntity<BonePostureData>	bonePostures;
		
		
		internal int	*pDrawCounter;

		internal JobHandle	copyJob;


		protected override void OnCreateManager()
		{
			base.OnCreateManager();

			var BoneLength	= 16;

			maxRootMatrices	= new NativeArray<Matrix4x4>( 1023 / BoneLength * 1000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory );
			maxPositions	= new NativeArray<Vector4>( 1023 * 1000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory );
			maxRotations	= new NativeArray<Vector4>( 1023 * 1000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory );

			pDrawCounter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), 4, Allocator.Persistent);

			Debug.Log( "create j" );
		}

		protected override unsafe void OnDestroyManager()
		{
		//	copyJob.Complete();


			Debug.Log( "destroy j" );

			UnsafeUtility.Free(pDrawCounter, Allocator.Persistent);

			maxRootMatrices.Dispose();
			maxPositions.Dispose();
			maxRotations.Dispose();

			base.OnDestroyManager();
		}
		
		protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
		{
			//return inputDeps;
			var BoneLength	= 16;//

			*pDrawCounter = 0;
			var drawJob = new ObjectDrawSwitchJob
			{
				pCouter = pDrawCounter,
				boneLinkes  = boneEntityLinks,
				postures    = bonePostures,
				boneLength  = BoneLength,
				positions   = maxPositions,
				rotations   = maxRotations,
				roots       = maxRootMatrices,
			};
			inputDeps = drawJob.Schedule( this, inputDeps );//( this, 8, inputDeps );


			copyJob = inputDeps;

			inputDeps.Complete();
			return inputDeps;
		}

		
		[BurstCompile]
		struct ObjectDrawSwitchJob : IJobProcessComponentData<ObjectDrawTargetLabel, ObjectPostureData>
		{

			[ReadOnly]
			internal ComponentDataFromEntity<BoneEntityLinkData>	boneLinkes;
			[ReadOnly]
			internal ComponentDataFromEntity<BonePostureData>		postures;
			
			[WriteOnly] [NativeDisableParallelForRestriction]
			internal NativeArray<Vector4>	positions;
			[WriteOnly] [NativeDisableParallelForRestriction]
			internal NativeArray<Vector4>	rotations;
			[WriteOnly] [NativeDisableParallelForRestriction]
			internal NativeArray<Matrix4x4>	roots;

			internal int boneLength;

			[NativeDisableUnsafePtrRestriction]
			internal int* pCouter;


			public void Execute( [ReadOnly] ref ObjectDrawTargetLabel d, [ReadOnly] ref ObjectPostureData op )
			{

				var drawIndex = Interlocked.Increment(ref *pCouter) - 1;

				roots[drawIndex] = Matrix4x4.TRS(op.position, op.rotation, op.scale);


				var boneSerialIndex = boneLength * drawIndex;

				for ( var boneEnt = d.BoneEntityTop ; boneEnt != Entity.Null ; boneEnt = boneLinkes[ boneEnt ].NextEntity )
				{
					var bp	= postures[ boneEnt ];

					positions[boneSerialIndex] = bp.position.As_float4();
					rotations[boneSerialIndex] = bp.rotation.value;

					++boneSerialIndex;
				}
			}
		}
	}

	

	
	[UpdateAfter( typeof(CopyPosturesForDrawInstancedSkinMeshSystem) )]
	public class DrawInstancedSkinMeshSystem : ComponentSystem
	{
		
		struct Renderers
		{
			[ReadOnly]
			public SharedComponentDataArray<MeshInstanceRenderer>	renderers;
		}

		[Inject] Renderers		renderers;

		
		[Inject] [ReadOnly]
		CopyPosturesForDrawInstancedSkinMeshSystem	postures;


		//public int	DrawModelLength;
		//public int	BoneLength;
		Vector4[]	perDrawPositions;
		Vector4[]	perDrawRotations;
		Matrix4x4[]	perDrawRootMatrices;
		Matrix4x4[]	bindposes;

		readonly int	bindPoses_ShaderId	= Shader.PropertyToID("bindPoses");
		readonly int	boneLength_ShaderId	= Shader.PropertyToID("boneLength");
		readonly int	positions_ShaderId	= Shader.PropertyToID("positions");
		readonly int	rotations_ShaderId	= Shader.PropertyToID("rotations");

		protected override void OnCreateManager()
		{
			base.OnCreateManager();

			var BoneLength	= 16;
			
			perDrawRootMatrices	= new Matrix4x4 [ 1023 / BoneLength ];
			perDrawPositions	= new Vector4 [ 1023 ];
			perDrawRotations	= new Vector4 [ 1023 ];

			Debug.Log( "create" );
		}

		protected override void OnDestroyManager()
		{
			if( postures != null )
			{
				postures.copyJob.Complete();
			}

			base.OnDestroyManager();
			
			Debug.Log( "destroy" );
		}

		protected override unsafe void OnUpdate()
		{
			//return;
			if ( postures == null ) return;
			if( *postures.pDrawCounter == 0 ) return;
			postures.copyJob.Complete();

			var BoneLength	= 16;//
			
			
			var r = renderers.renderers[0];
			
			if( bindposes == null ) bindposes = r.mesh.bindposes;
			r.material.SetMatrixArray( bindPoses_ShaderId, bindposes );
			
			var boneLength	= bindposes.Length;
			r.material.SetFloat( boneLength_ShaderId, bindposes.Length );


			var totalDrawModelLength	= *postures.pDrawCounter;

			var maxModelLengthPerDraw	= 1023 / BoneLength;
			var maxBoneLengthPerDraw	= maxModelLengthPerDraw * BoneLength;
			var totalDrawBoneLength		= totalDrawModelLength * BoneLength;

			var freq	= totalDrawModelLength / maxModelLengthPerDraw;
			if( totalDrawModelLength % maxModelLengthPerDraw != 0 ) freq++;
			for( var i = 0 ; i < freq ; i++ )
			{
				
				// ボーンセット

				var finishedBoneCount	= i * maxBoneLengthPerDraw;
				var remainedBoneCount	= totalDrawBoneLength - finishedBoneCount;
				var boneLengthPerDraw	= math.min( remainedBoneCount, maxBoneLengthPerDraw );

				postures.maxPositions.Slice( finishedBoneCount, boneLengthPerDraw ).CopyToArray( perDrawPositions );
				postures.maxRotations.Slice( finishedBoneCount, boneLengthPerDraw ).CopyToArray( perDrawRotations );


				// モデルセット

				var finishedModelCount	= i * maxModelLengthPerDraw;
				var remainedModelCount	= totalDrawModelLength - finishedModelCount;
				var modelLengthPerDraw	= math.min( remainedModelCount, maxModelLengthPerDraw );

				postures.maxRootMatrices.Slice( finishedModelCount, modelLengthPerDraw ).CopyToArray( perDrawRootMatrices );


				//Debug.Log( $"{finishedBoneCount}, {finishedModelCount}, {remainedModelCount}, {remainedBoneCount}, {perDrawRootMatrices.Length}" );


				// 描画

				draw(r, modelLengthPerDraw);
			}
			
		}
		

		void draw( MeshInstanceRenderer r, int i )
		{
			if ( i == 0 ) return;
			//Debug.Log($"{i} {perDrawRootMatrices.Length}");
			
			r.material.SetVectorArray( positions_ShaderId, perDrawPositions );
			r.material.SetVectorArray( rotations_ShaderId, perDrawRotations );

			Graphics.DrawMeshInstanced( r.mesh, 0, r.material, perDrawRootMatrices, i );
		}
		

	}
	
}