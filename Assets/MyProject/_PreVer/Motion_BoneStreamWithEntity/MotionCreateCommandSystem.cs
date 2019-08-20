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
using Abss.Obj.Entities;


namespace Abss.Motion.WithComponent
{


	[UpdateBefore( typeof(EndFrameBarrier) )]
	public class MotionCreateCommandSystem : ComponentSystem
	{

		EntityManager		em;
		
		MotionArchetypes	archetypes;

		MotionDataInNative	md;

		

		NativeQueue<MotionEntityCreateCommand>	postedCommands;

		
		//[Inject]
		//ObjectSystem	objectSystem;


		JobHandle	dependsCommandQueueJob;


		/// <summary>
		/// モーション関係一連の Entity の生成をポストするためのコマンドキューを返す。
		/// ポストされたコマンドは、EndFrameBarrier 手前のタイミングで実行される。
		/// </summary>
		public MotionEntityCreateCommandQueueConcurrent CreateCommandQueue()
		{
			return new MotionEntityCreateCommandQueueConcurrent( postedCommands );
		}
		public void SetUserJob( JobHandle commandUserJobHandle )
		{
			dependsCommandQueueJob = JobHandle.CombineDependencies( dependsCommandQueueJob, commandUserJobHandle );
		}



		// システムオーバーライド -------------------------------------

		protected override void OnCreateManager()
		{
			base.OnCreateManager();


			var resource = (GameObject)Resources.Load( "Motion clip" );
			var clip = resource.GetComponent<MotionBootStrap>().MotionClipData;
			//var clip = (MotionClip)Resources.Load( "Motion minic" );
			
			md	= new MotionDataInNative();
			md.ConvertFrom( clip );
		//	md.TransformKeyDataToWorld_();
			
			em = World.GetOrCreateManager<EntityManager>();

			archetypes = new MotionArchetypes( em );
			
			postedCommands = new NativeQueue<MotionEntityCreateCommand>( Allocator.Persistent );
		}

		protected override void OnDestroyManager()
		{
			dependsCommandQueueJob.Complete();

			if( md != null ) md.Dispose();

			postedCommands.Dispose();

			base.OnDestroyManager();
		}

		protected override void OnUpdate()
		{
			dependsCommandQueueJob.Complete();

			while( postedCommands.TryDequeue( out var command ) )
			{
				
				//var ( motionEnt, boneTopEnt ) = createMotionBoneStreamEntityGroup( command );

				//setBoneIfDrawTarget( command.drawTargetEntity, boneTopEnt, md.boneLength );
				
			}

			postedCommands.Clear();
		}

		// -----------------------------------------------------

		
		
		void setBoneIfDrawTarget( Entity drawTargetEnt, Entity boneTopEnt, int boneLength )
		{
			if( drawTargetEnt == Entity.Null ) return;
			
			em.AddComponentData<ObjectDrawTargetLabel>( drawTargetEnt,
				new ObjectDrawTargetLabel
				{
					ObjectEntity	= drawTargetEnt,
					BoneEntityTop	= boneTopEnt,
					BoneLength		= md.boneLength,
				}
			);
		}



		#if false
		/// <summary>
		/// モーションに関係する Entity を作成する。
		/// </summary>
		(Entity motion, Entity boneTop) createMotionBoneStreamEntityGroup( MotionEntityCreateCommand command )
		{
			
			var ma = md.CreateAccessor( command.motionIndex );

				
			var boneEntityTop = createBoneAndStreamEntities();
			
			return ( createMotionEntity( boneEntityTop ), boneEntityTop );



			// モーション

			Entity createMotionEntity( Entity boneTop )
			{
				var motionEntity	= em.CreateEntity( archetypes.motion );
				
				em.SetComponentData<MotionInfoData>( motionEntity,
					new MotionInfoData
					{
						MotionIndex		= command.motionIndex,
						DataAccessor	= ma,
						BoneEntityTop	= boneTop,
					}
				);
				em.SetComponentData<MotionInitializeData>( motionEntity,
					new MotionInitializeData
					{}
				);

				return motionEntity;
			}
				
				
			// ボーン＆ストリーム
				
			Entity createBoneAndStreamEntities()
			{
				using( var boneEntities	= new NativeArray<Entity>( ma.boneLength, Allocator.Temp ) )
				using( var streamEntities = new NativeArray<Entity>( ma.boneLength * 2, Allocator.Temp ) )
				{
					em.CreateEntity( archetypes.bone, boneEntities );
					em.CreateEntity( archetypes.stream, streamEntities );


					Enumerable.Zip( boneEntities, streamEntities.Buffer(2), (bone, streams) => (bone, streams) )
					.ForEach( (ents, ibone) =>
					{
						
						createBoneEntity( in ents, ibone, in boneEntities );
						

					//	if( ibone != 0 ) em.AddComponentData( ents.streams[(int)KeyStreamSection.positions], new StreamInitialDataFor1pos() );
						createStreamSectionEntity( in ents, ibone, KeyStreamSection.positions );
						createStreamSectionEntity( in ents, ibone, KeyStreamSection.rotations );
						
					} );


					return boneEntities[ 0 ];
				}
			}
			
			void createBoneEntity( in (Entity bone, IList<Entity> streams) ents, int ibone, in NativeArray<Entity> boneEntities )
			{
				var parentIndex = ma.GetParentBoneIndex( ibone );
				var siblingIndex = ibone + 1;

				em.SetComponentData( ents.bone,
					new BoneEntityLinkData
					{
						NextEntity	= siblingIndex < ma.boneLength ? boneEntities[siblingIndex] : Entity.Null,
						ParentEntity	= parentIndex > -1 ? boneEntities[parentIndex] : Entity.Null,
						positionEntity	= ents.streams[0],
						rotationEntity	= ents.streams[1],
					}
				);
				//em.SetComponentData( ents.bone,
				//	new BoneIndexData
				//	{
				//		Index		= ibone,
				//		ParentIndex	= parentIndex,
				//	}
				//);
			}

			void createStreamSectionEntity( in (Entity bone, IList<Entity> streams) ents, int ibone, KeyStreamSection streamSection )
			{
				em.SetComponentData( ents.streams[ (int)streamSection ],
					new StreamKeyShiftData
					{
						Keys	= ma.GetStreamSlice( ibone, streamSection ).Stream,
					}
				);
				em.SetComponentData( ents.streams[ (int)streamSection ],
					new StreamNearKeysCacheData
					{
						TimeLength	= ma.TimeLength,
						TimeScale	= command.timeScale,
					}
				);
			}
			
		}
		#endif
	}



	/// <summary>
	/// モーション関連 Enity の Archetype
	/// </summary>
	struct MotionArchetypes
	{
		internal readonly EntityArchetype	motion;
		internal readonly EntityArchetype	bone;
		internal readonly EntityArchetype	stream;

		public MotionArchetypes( EntityManager em )
		{

			var motionArch	= em.CreateArchetype(
				typeof(MotionInitializeData),
				typeof(MotionInfoData)
			);
			var boneArch    = em.CreateArchetype(
				typeof(BoneEntityLinkData),
				//typeof(BoneIndexData),
				typeof(BonePostureData)
			);
			var streamArch  = em.CreateArchetype(
				typeof(StreamInitialLabel),
				typeof(StreamKeyShiftData),
				typeof(StreamNearKeysCacheData),
				typeof(StreamInterpolatedData)
			);

			motion	= motionArch;
			bone	= boneArch;
			stream	= streamArch;
		}
	}



	/// <summary>
	/// コマンド
	/// </summary>
	public struct MotionEntityCreateCommand
	{
		internal int	motionIndex;
		internal float	timeScale;
		internal Entity	drawTargetEntity;
	}

	/// <summary>
	/// モーション関連 Entity 生成コマンドをポストするためのキュー。
	/// </summary>
	public struct MotionEntityCreateCommandQueueConcurrent
	{
		[WriteOnly]
		internal NativeQueue<MotionEntityCreateCommand>.Concurrent	commandQueue;
		
		internal MotionEntityCreateCommandQueueConcurrent( NativeQueue<MotionEntityCreateCommand> queue )
		{
			commandQueue = queue.ToConcurrent();
		}

		public void PostCreateMotion( in MotionEntityCreateCommand motionCreateCommand )
		{
			commandQueue.Enqueue( motionCreateCommand );
		}
	}





	/*
	public class MotionCreator
	{
		
		static public void a()
		{
			var em	= World.Active.GetOrCreateManager<EntityManager>();

			var types	= new[]
			{
				ComponentType.Create<MotionInfoData>(),
				ComponentType.Create<MotionBoneElement>(),
				ComponentType.Create<MotionStreamElement>()
			};

			var motionArche	= em.CreateArchetype( types );

			var ent	= em.CreateEntity( motionArche );
			var boneBuffer		= em.GetBuffer<MotionBoneElement>( ent );
			var streamBuffer	= em.GetBuffer<MotionStreamElement>( ent );
			for( var i=0; i<16; i++ ) boneBuffer.Add( new MotionBoneElement() );
			for( var i=0; i<16*2; i++ ) streamBuffer.Add( new MotionStreamElement() );
		}

	}
	*/

}